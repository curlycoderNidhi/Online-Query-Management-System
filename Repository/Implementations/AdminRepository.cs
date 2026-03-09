using System;
using System.Collections.Generic;
using Npgsql;
using Repositories.Interfaces;
using Repository.Models;
using Repository.Models.Enums;

namespace Repositories.Implementations
{
    public class AdminRepository : IAdminInterface
    {
        private readonly NpgsqlConnection _conn;

        public AdminRepository(NpgsqlConnection conn) => _conn = conn;

        public async Task<List<Employee>> GetAllEmployees()
        {
            var employees = new List<Employee>();
            string sql = "SELECT c_empid, c_empname FROM t_employee ORDER BY c_empname";
            
            _conn.Open();
            using (var cmd = new NpgsqlCommand(sql, _conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    employees.Add(new Employee
                    {
                        EmpId = Convert.ToInt32(reader["c_empid"]),
                        EmpName = reader["c_empname"]?.ToString() ?? ""
                    });
                }
            }
            _conn.Close();
            return employees;
        }

   
        public async Task<List<AdminQuery>> GetAllQueries() => await GetQueries("");
        public async Task<List<AdminQuery>> GetAllQueriesOpen() => await GetQueries("Open");
        public async Task<List<AdminQuery>> GetAllQueriesInProgress() => await GetQueries("InProgress");
        public async Task<List<AdminQuery>> GetAllQueriesSolved() => await GetQueries("Solved");

        private async Task<List<AdminQuery>> GetQueries(string status = "")
        {
            var queries = new List<AdminQuery>();
            string where = string.IsNullOrEmpty(status) ? "" : $"WHERE q.c_status = '{status}'";
            
            string sql = $@"
                SELECT q.*, COALESCE(e.c_empname, '') as c_empname, u.c_companyname
                FROM t_queries q
                LEFT JOIN t_employee e ON q.c_empid = e.c_empid
                JOIN t_users u ON q.c_userid = u.c_userid
                {where}
                ORDER BY q.c_queryid DESC";

            _conn.Open();
            using (var cmd = new NpgsqlCommand(sql, _conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    queries.Add(new AdminQuery
                    {
                        QueryId = Convert.ToInt32(reader["c_queryid"]),
                        Username = reader["c_companyname"]?.ToString() ?? "",
                        Title = reader["c_title"]?.ToString() ?? "",
                        Description = reader["c_description"]?.ToString() ?? "",
                        // 🔥 FIXED: Priority is STRING "High"/"Medium"/"Low"
                        Priority = GetPriority(reader["c_priority"]?.ToString()),
                        EmployeeName = reader["c_empname"]?.ToString() ?? "",
                        Status = GetQueryStatus(reader["c_status"]?.ToString()),
                        Comments = reader["c_comments"]?.ToString() ?? ""
                    });
                }
            }
            _conn.Close();
            return queries;
        }

      
        private Priority GetPriority(string priority)
        {
            return priority?.ToLowerInvariant() switch
            {
                "high" => Priority.High,
                "medium" => Priority.Medium,
                "low" => Priority.Low,
                _ => Priority.Low
            };
        }

        
        private QueryStatus GetQueryStatus(string status)
        {
            return status?.ToLowerInvariant() switch
            {
                "solved" => QueryStatus.Solved,
                "inprogress" => QueryStatus.InProgress,
                _ => QueryStatus.Open
            };
        }

             public async Task<List<User>> GetAllUsers()
        {
            var users = new List<User>();
            string sql = "SELECT c_userid, c_companyname, c_email FROM t_users ORDER BY c_companyname";

            _conn.Open();
            using (var cmd = new NpgsqlCommand(sql, _conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    users.Add(new User
                    {
                        UserId = Convert.ToInt32(reader["c_userid"]),
                        CompanyName = reader["c_companyname"]?.ToString() ?? "",
                        Email = reader["c_email"]?.ToString() ?? ""
                    });
                }
            }
            _conn.Close();
            return users;
        }

        public async Task<User> GetUserDetails(int id)
        {
            string sql = "SELECT c_userid, c_companyname, c_email FROM t_users WHERE c_userid = @id";
            _conn.Open();
            using (var cmd = new NpgsqlCommand(sql, _conn))
            {
                cmd.Parameters.AddWithValue("id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        _conn.Close();
                        return new User
                        {
                            UserId = Convert.ToInt32(reader["c_userid"]),
                            CompanyName = reader["c_companyname"]?.ToString() ?? "",
                            Email = reader["c_email"]?.ToString() ?? ""
                        };
                    }
                }
            }
            _conn.Close();
            return null;
        }


        public async Task<int> AssignEmployee(int queryId, int empId)
        {
            string sql = "UPDATE t_queries SET c_empid = @empid WHERE c_queryid = @qid";
            _conn.Open();
            using (var cmd = new NpgsqlCommand(sql, _conn))
            {
                cmd.Parameters.Add("empid", NpgsqlTypes.NpgsqlDbType.Integer).Value =
                    empId > 0 ? empId : DBNull.Value;
                cmd.Parameters.AddWithValue("qid", queryId);
                int result = cmd.ExecuteNonQuery();
                _conn.Close();
                return result;
            }
        }

        public async Task<Dictionary<string, int>> GetDashboardCards()
        {
            var cards = new Dictionary<string, int>();
            _conn.Open();

            string[] names = { "TotalQueries", "SolvedQueries", "OpenQueries", "InProgressQueries" };
            string[] queries = {
                "SELECT COUNT(*) FROM t_queries",
                "SELECT COUNT(*) FROM t_queries WHERE c_status = 'Solved'",
                "SELECT COUNT(*) FROM t_queries WHERE c_status = 'Open'",
                "SELECT COUNT(*) FROM t_queries WHERE c_status = 'InProgress'"
            };

            for (int i = 0; i < names.Length; i++)
            {
                using (var cmd = new NpgsqlCommand(queries[i], _conn))
                {
                    cards[names[i]] = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            _conn.Close();
            return cards;
        }

       
        public async Task<List<EmployeePerformance>> GetEmployeePerformance()
        {
            var list = new List<EmployeePerformance>();
            string sql = @"
                SELECT e.c_empname, COUNT(q.c_queryid) as solved_count
                FROM t_employee e
                LEFT JOIN t_queries q ON e.c_empid = q.c_empid AND q.c_status = 'Solved'
                GROUP BY e.c_empname 
                ORDER BY solved_count DESC";

            _conn.Open();
            using (var cmd = new NpgsqlCommand(sql, _conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new EmployeePerformance
                    {
                        EmployeeName = reader["c_empname"]?.ToString() ?? "",
                        ResolvedQueries = Convert.ToInt32(reader["solved_count"])
                    });
                }
            }
            _conn.Close();
            return list;
        }

           public async Task<List<AdminQuery>> GetSubmittedQueries(int id)
        {
            var queries = new List<AdminQuery>();
            string sql = @"
                SELECT q.*, u.c_companyname 
                FROM t_queries q JOIN t_users u ON q.c_userid = u.c_userid 
                WHERE u.c_userid = @id";

            _conn.Open();
            using (var cmd = new NpgsqlCommand(sql, _conn))
            {
                cmd.Parameters.AddWithValue("id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        queries.Add(new AdminQuery
                        {
                            QueryId = Convert.ToInt32(reader["c_queryid"]),
                            Username = reader["c_companyname"]?.ToString() ?? "",
                            Title = reader["c_title"]?.ToString() ?? "",
                            Priority = GetPriority(reader["c_priority"]?.ToString()), // 🔥 FIXED
                            Status = GetQueryStatus(reader["c_status"]?.ToString()),
                            Comments = reader["c_comments"]?.ToString() ?? ""
                        });
                    }
                }
            }
            _conn.Close();
            return queries;
        }
    }
}
