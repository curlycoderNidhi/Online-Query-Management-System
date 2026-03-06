using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using Repositories.Interfaces;
using Repository.Models;
using Repository.Models.Enums;

namespace Repositories.Implementations
{
    public class AdminRepository : IAdminInterface
    {
        private readonly NpgsqlConnection _conn;

        public AdminRepository(NpgsqlConnection conn)
        {
            _conn=conn;
        }

        public async Task<List<Query>> GetAllQueriesSolved()
        {
            List<Query> queries = new List<Query>();
            string qry="SELECT * FROM t_queries q JOIN t_employee e on q.c_empid=e.c_empid JOIN t_users u on q.c_userid=u.c_userid WHERE c_status='Solved' ";
            var cmd= new NpgsqlCommand(qry,_conn);

            // var reader = await cmd.ExecuteReaderAsync();
            _conn.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                queries.Add(
                    new Query()
                    {
                        QueryId=(int)reader["c_queryid"],
                        Username=reader["c_companyname"].ToString(),
                        Title=reader["c_title"].ToString(),
                        Description=reader["c_description"].ToString(),
                        Priority=(Priority)(int)reader["c_priority"],
                        EmployeeName=reader["c_empname"].ToString(),
                        Status=(QueryStatus)(int)reader["c_status"],
                        Comments=reader["c_comments"].ToString()
                    }
                );
            }
            _conn.Close();
            return queries;
        }

        public async Task<List<Query>> GetAllQueriesInProgress()
        {
            List<Query> queries = new List<Query>();
            string qry="SELECT * FROM t_queries q JOIN t_employee e on q.c_empid=e.c_empid JOIN t_users u on q.c_userid=u.c_userid WHERE c_status='InProgress' ";
            var cmd= new NpgsqlCommand(qry,_conn);

            // var reader = await cmd.ExecuteReaderAsync();
            _conn.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                queries.Add(
                    new Query()
                    {
                        QueryId=(int)reader["c_queryid"],
                        Username=reader["c_companyname"].ToString(),
                        Title=reader["c_title"].ToString(),
                        Description=reader["c_description"].ToString(),
                        Priority=(Priority)(int)reader["c_priority"],
                        EmployeeName=reader["c_empname"].ToString(),
                        Status=(QueryStatus)(int)reader["c_status"],
                        Comments=reader["c_comments"].ToString()
                    }
                );
            }
            _conn.Close();
            return queries;
        }
        public async Task<List<Query>> GetAllQueriesOpen()
        {
            List<Query> queries = new List<Query>();
            string qry="SELECT * FROM t_queries q JOIN t_employee e on q.c_empid=e.c_empid JOIN t_users u on q.c_userid=u.c_userid WHERE c_status='Open'";
            var cmd= new NpgsqlCommand(qry,_conn);

            // var reader = await cmd.ExecuteReaderAsync();
            _conn.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                queries.Add(
                    new Query()
                    {
                        QueryId=(int)reader["c_queryid"],
                        Username=reader["c_companyname"].ToString(),
                        Title=reader["c_title"].ToString(),
                        Description=reader["c_description"].ToString(),
                        Priority=(Priority)(int)reader["c_priority"],
                        EmployeeName=reader["c_empname"].ToString(),
                        Status=(QueryStatus)(int)reader["c_status"],
                        Comments=reader["c_comments"].ToString()
                    }
                );
            }
            _conn.Close();
            return queries;
        }

        public async Task<List<Query>> GetAllQueries()
        {
            List<Query> queries = new List<Query>();
            string qry="SELECT * FROM t_queries q JOIN t_employee e on q.c_empid=e.c_empid JOIN t_users u on q.c_userid=u.c_userid";
            var cmd= new NpgsqlCommand(qry,_conn);

            // var reader = await cmd.ExecuteReaderAsync();
            _conn.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                queries.Add(
                    new Query()
                    {
                        QueryId=(int)reader["c_queryid"],
                        Username=reader["c_companyname"].ToString(),
                        Title=reader["c_title"].ToString(),
                        Description=reader["c_description"].ToString(),
                        Priority=(Priority)(int)reader["c_priority"],
                        EmployeeName=reader["c_empname"].ToString(),
                        Status=(QueryStatus)(int)reader["c_status"],
                        Comments=reader["c_comments"].ToString()
                    }
                );
            }
            _conn.Close();
            return queries;
        }

        public async Task<List<User>> GetAllUsers()
        {
            List<User> users = new List<User>();
            var qry = "SELECT * FROM t_users";
            var cmd = new NpgsqlCommand(qry,_conn);
            _conn.Open();
            var reader= cmd.ExecuteReader();

            while (reader.Read())
            {
                new User()
                {
                    UserId = (int)reader[0],
                    CompanyName = reader[1].ToString(),
                    Email = reader[2].ToString()
                };
            }
            _conn.Close();
            return users;
        }

        public async Task<User> GetUserDetails(int id)
        {
            User users = new User();
            var qry = "SELECT * FROM t_users WHERE c_userid=@id";
            var cmd = new NpgsqlCommand(qry,_conn);
            cmd.Parameters.AddWithValue("id",id);
            _conn.Open();
            var reader= cmd.ExecuteReader();

            if(reader.Read())
            {
                new User()
                {
                    UserId = (int)reader[0],
                    CompanyName = reader[0].ToString(),
                    Email = reader[0].ToString()
                };
            }
            _conn.Close();
            return users;
        }


            public async Task<List<Query>> GetSubmittedQueries(int id)
            {
            List<Query> queries = new List<Query>();
            string qry="SELECT * FROM t_queries q JOIN t_users u on q.c_userid=u.c_userid WHERE u.c_userid=@id";
            var cmd= new NpgsqlCommand(qry,_conn);
            cmd.Parameters.AddWithValue("id",id);

            // var reader = await cmd.ExecuteReaderAsync();
            _conn.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                queries.Add(
                    new Query()
                    {
                        QueryId=(int)reader["c_queryid"],
                        Username=reader["c_companyname"].ToString(),
                        Title=reader["c_title"].ToString(),
                        Description=reader["c_description"].ToString(),
                        Priority=(Priority)(int)reader["c_priority"],
                        Status=(QueryStatus)(int)reader["c_status"],
                        Comments=reader["c_comments"].ToString()
                    }
                );
            }
            _conn.Close();
            return queries;
        }

        public async Task<int> AssignEmployee(int queryId,int empId)
        {
            var qry="UPDATE t_queries SET c_empid=@empid WHERE queryId=@qid";
            var cmd = new NpgsqlCommand(qry,_conn);
            cmd.Parameters.AddWithValue("empid",empId);
            cmd.Parameters.AddWithValue("qid",queryId);

            _conn.Open();
            int status=cmd.ExecuteNonQuery();
            _conn.Close();
            return status;
        }



public async Task<Dictionary<string,int>> GetDashboardCards()
{
    Dictionary<string,int> dict = new Dictionary<string, int>();

    _conn.Open();

    // Total Queries
    var totalCmd = new NpgsqlCommand("SELECT COUNT(*) FROM t_queries",_conn);
    dict["TotalQueries"] = Convert.ToInt32(totalCmd.ExecuteScalar());

    // Today's Queries
    var todayCmd = new NpgsqlCommand("SELECT COUNT(*) FROM t_queries WHERE DATE(c_querydate)=CURRENT_DATE",_conn);
    dict["TodayQueries"] = Convert.ToInt32(todayCmd.ExecuteScalar());

    // Solved All
    var solvedCmd = new NpgsqlCommand($"SELECT COUNT(*) FROM t_queries WHERE c_status={(int)QueryStatus.Solved}",_conn);
    
    dict["SolvedQueries"] = Convert.ToInt32(solvedCmd.ExecuteScalar());

    // Solved Today
    var solvedTodayCmd = new NpgsqlCommand($"SELECT COUNT(*) FROM t_queries WHERE c_status={(int)QueryStatus.Solved} AND DATE(c_querydate)=CURRENT_DATE",_conn);
    dict["SolvedToday"] = Convert.ToInt32(solvedTodayCmd.ExecuteScalar());

    // Pending All
    var pendingCmd = new NpgsqlCommand($"SELECT COUNT(*) FROM t_queries WHERE c_status={(int)QueryStatus.InProgress}",_conn);
    dict["PendingQueries"] = Convert.ToInt32(pendingCmd.ExecuteScalar());

    // Pending Today
    var pendingTodayCmd = new NpgsqlCommand($"SELECT COUNT(*) FROM t_queries WHERE c_status={(int)QueryStatus.InProgress} AND DATE(c_querydate)=CURRENT_DATE",_conn);
    dict["PendingToday"] = Convert.ToInt32(pendingTodayCmd.ExecuteScalar());

    // Pending All
    var OpenCmd = new NpgsqlCommand($"SELECT COUNT(*) FROM t_queries WHERE c_status={(int)QueryStatus.Open}",_conn);
    dict["PendingQueries"] = Convert.ToInt32(OpenCmd.ExecuteScalar());

    // Pending Today
    var OpenTodayCmd = new NpgsqlCommand($"SELECT COUNT(*) FROM t_queries WHERE c_status={(int)QueryStatus.Open} AND DATE(c_querydate)=CURRENT_DATE",_conn);
    dict["PendingToday"] = Convert.ToInt32(OpenTodayCmd.ExecuteScalar());

    _conn.Close();

    return dict;
}


public async Task<List<EmployeePerformance>> GetEmployeePerformance()
{
    List<EmployeePerformance> list = new List<EmployeePerformance>();

    string qry = @"SELECT e.c_empname, COUNT(q.c_queryid) as resolvedcount
                   FROM t_employee e
                   LEFT JOIN t_queries q 
                   ON e.c_empid=q.c_empid 
                   AND q.c_status='Solved'
                   GROUP BY e.c_empname";

    var cmd = new NpgsqlCommand(qry,_conn);

    _conn.Open();

    var reader = cmd.ExecuteReader();

    while(reader.Read())
    {
        list.Add(new EmployeePerformance
        {
            EmployeeName = reader["c_empname"].ToString(),
            ResolvedQueries = Convert.ToInt32(reader["resolvedcount"])
        });
    }

    _conn.Close();

    return list;
}

    }
}