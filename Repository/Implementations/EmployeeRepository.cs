using Npgsql;
using Repository.Interfaces;
using Repository.Models;
using Repository.Models.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Implementations
{
    public class EmployeeRepository : IEmployeeInterface
    {
        private readonly NpgsqlConnection _conn;

        public EmployeeRepository(NpgsqlConnection conn)
        {
            _conn = conn;
        }

        // 1️⃣ Get Unassigned Queries
        // 2️⃣ Employee Assigned Queries
        public async Task<List<Query>> GetEmployeeQueries(int empid)
        {
            List<Query> list = new List<Query>();

            string query = @"SELECT *
                            FROM t_queries
                            WHERE c_empid = @empid
                            AND c_status != 'Solved'
                            ORDER BY c_querydate DESC; ";

            try
            {
                await _conn.OpenAsync();

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, _conn))
                {
                    cmd.Parameters.AddWithValue("@empid", empid);

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            list.Add(new Query
                            {
                                QueryId = Convert.ToInt32(dr["c_queryid"]),
                                UserId = Convert.ToInt32(dr["c_userid"]),
                                Title = dr["c_title"].ToString(),
                                Description = dr["c_description"].ToString(),
                                Priority = Enum.Parse<Priority>(dr["c_priority"].ToString()),
                                QueryDate = Convert.ToDateTime(dr["c_querydate"]),
                                EmpId = Convert.ToInt32(dr["c_empid"]),
                                Status = Enum.Parse<QueryStatus>((dr["c_status"]?.ToString() ?? "Open").Replace(" ", ""), true),
                                Comments = dr["c_comments"]?.ToString()
                            });
                        }
                    }
                }

                await _conn.CloseAsync();
            }
            catch
            {
                await _conn.CloseAsync();
                throw;
            }

            return list;
        }

        // 3️⃣ Take Query
        // 4️⃣ Update Query Status
        public async Task<bool> UpdateQueryStatus(Query model)
        {
            string query = @"UPDATE t_queries
                             SET c_status = @status,
                                 c_comments = @comments,
                                 c_querydate = CASE
                                     WHEN @status = 'Solved' THEN CURRENT_TIMESTAMP
                                     ELSE c_querydate
                                 END
                             WHERE c_queryid = @queryid";

            await _conn.OpenAsync();

            using (NpgsqlCommand cmd = new NpgsqlCommand(query, _conn))
            {
                cmd.Parameters.AddWithValue("@status", model.Status == QueryStatus.InProgress ? "In Progress" : model.Status.ToString());
                cmd.Parameters.AddWithValue("@comments", model.Comments ?? "");
                cmd.Parameters.AddWithValue("@queryid", model.QueryId);

                int rows = await cmd.ExecuteNonQueryAsync();

                await _conn.CloseAsync();

                return rows > 0;
            }
        }

        // 5️⃣ Total Resolved by Employee
        public async Task<int> GetResolvedCount(int empid)
        {
            string query = @"SELECT COUNT(*)
                             FROM t_queries
                             WHERE c_empid = @empid
                             AND c_status = 'Solved'";

            await _conn.OpenAsync();

            using (NpgsqlCommand cmd = new NpgsqlCommand(query, _conn))
            {
                cmd.Parameters.AddWithValue("@empid", empid);

                int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                await _conn.CloseAsync();

                return count;
            }
        }

        // 6️⃣ Pending Count
        public async Task<int> GetPendingCount(int empid)
        {
            string query = @"SELECT COUNT(*)
                             FROM t_queries
                             WHERE c_empid = @empid
                             AND c_status <> 'Solved'";

            await _conn.OpenAsync();

            using (NpgsqlCommand cmd = new NpgsqlCommand(query, _conn))
            {
                cmd.Parameters.AddWithValue("@empid", empid);

                int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                await _conn.CloseAsync();

                return count;
            }
        }

        public async Task<int> GetAssignedCount(int empid)
        {
            const string query = @"SELECT COUNT(*)
                                   FROM t_queries
                                   WHERE c_empid = @empid";

            await _conn.OpenAsync();

            using (NpgsqlCommand cmd = new NpgsqlCommand(query, _conn))
            {
                cmd.Parameters.AddWithValue("@empid", empid);

                int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                await _conn.CloseAsync();

                return count;
            }
        }

        // 7️⃣ Today's Resolved
        public async Task<int> GetTodayResolvedCount(int empid)
        {
            string query = @"SELECT COUNT(*)
                             FROM t_queries
                             WHERE c_empid = @empid
                               AND c_status = 'Solved'
                               AND DATE(c_querydate) = CURRENT_DATE";

            await _conn.OpenAsync();

            using (NpgsqlCommand cmd = new NpgsqlCommand(query, _conn))
            {
                cmd.Parameters.AddWithValue("@empid", empid);

                int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                await _conn.CloseAsync();

                return count;
            }
        }

        public async Task<Employee?> Login(string empName, string password)
        {
            try
            {
                await _conn.OpenAsync();

                const string query = @"SELECT c_empid, c_empname, c_password, c_role
                                       FROM t_employee
                                       WHERE LOWER(TRIM(c_empname)) = LOWER(TRIM(@empname))
                                         AND c_password = @password
                                       LIMIT 1";

                using var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("@empname", (empName ?? string.Empty).Trim());
                cmd.Parameters.AddWithValue("@password", password);

                using var dr = await cmd.ExecuteReaderAsync();
                if (!await dr.ReadAsync())
                    return null;

                var roleValue = dr["c_role"]?.ToString() ?? "employee";
                if (!Enum.TryParse<Role>(roleValue, true, out var role))
                    role = Role.employee;

                return new Employee
                {
                    EmpId = Convert.ToInt32(dr["c_empid"]),
                    EmpName = dr["c_empname"]?.ToString() ?? string.Empty,
                    Password = dr["c_password"]?.ToString() ?? string.Empty,
                    Role = role
                };
            }
            finally
            {
                if (_conn.State != System.Data.ConnectionState.Closed)
                    await _conn.CloseAsync();
            }
        }
    }
}

