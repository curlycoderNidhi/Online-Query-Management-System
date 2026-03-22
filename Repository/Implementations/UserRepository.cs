using BCrypt.Net;
using Npgsql;
using Repository.Interfaces;
using Repository.Models;
using Repository.Models.Enums;

namespace Repository.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly NpgsqlConnection _connection;

        public UserRepository(NpgsqlConnection connection)
        {
            _connection = connection;
        }

        // ---------------- REGISTER ----------------
        public async Task<int> Register(User user)
        {
            try
            {
                string query = @"INSERT INTO t_users
                                (c_companyname, c_email, c_password)
                                VALUES (@CompanyName, @Email, @Password)
                                RETURNING c_userid";

                // 🔐 HASH PASSWORD
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);

                await _connection.OpenAsync();

                using var cmd = new NpgsqlCommand(query, _connection);

                cmd.Parameters.AddWithValue("@CompanyName", user.CompanyName);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@Password", hashedPassword);

                int userId = (int)await cmd.ExecuteScalarAsync();

                return userId;
            }
            catch
            {
                return -1;
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }


        // ---------------- LOGIN ----------------

        public async Task<User?> Login(UserLoginModel model)
        {
            try
            {
                string query = @"SELECT c_userid, c_companyname, c_email, c_password
                                 FROM t_users
                                 WHERE c_email = @Email";

                await _connection.OpenAsync();

                using var cmd = new NpgsqlCommand(query, _connection);

                cmd.Parameters.AddWithValue("@Email", model.Email);

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    string storedPassword = reader.GetString(3);
                    bool isValid = false;
                    try
                    {
                        // Try BCrypt first
                        isValid = BCrypt.Net.BCrypt.Verify(model.Password, storedPassword);
                    }
                    catch (SaltParseException)
                    {
                        // Fallback to plain-text equality for legacy rows
                        isValid = string.Equals(model.Password ?? string.Empty, storedPassword, StringComparison.Ordinal);
                    }

                    if (!isValid)
                        return null;

                    return new User
                    {
                        UserId = reader.GetInt32(0),
                        CompanyName = reader.GetString(1),
                        Email = reader.GetString(2)
                    };
                }

                return null;
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }


public async Task<User> GetByEmail(string email)
{
    
    string query = "SELECT * FROM t_users WHERE c_email=@Email";

    NpgsqlCommand cmd = new NpgsqlCommand(query, _connection);
    cmd.Parameters.AddWithValue("@Email", email);

    await _connection.OpenAsync();
    var reader = await cmd.ExecuteReaderAsync();

    if (reader.Read())
    {
        return new User
        {
            UserId = (int)reader["c_userid"],
            Email = reader["c_email"].ToString(),
            CompanyName = reader["c_companyname"].ToString()
        };
    }

    return null;
}

public async Task UpdatePassword(string email, string password)
{
    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

    using NpgsqlCommand cmd = new NpgsqlCommand(
        "UPDATE t_users SET c_password=@Password WHERE c_email=@Email",
        _connection
    );

    cmd.Parameters.AddWithValue("@Password", hashedPassword);
    cmd.Parameters.AddWithValue("@Email", email);

    await _connection.OpenAsync();
    await cmd.ExecuteNonQueryAsync();
}

        public async Task<User?> GetById(int userId)
        {
            string query = "SELECT * FROM t_users WHERE c_userid=@UserId";

            NpgsqlCommand cmd = new NpgsqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@UserId", userId);

            await _connection.OpenAsync();
            var reader = await cmd.ExecuteReaderAsync();

            if (reader.Read())
            {
                return new User
                {
                    UserId = (int)reader["c_userid"],
                    Email = reader["c_email"].ToString(),
                    CompanyName = reader["c_companyname"].ToString()
                };
            }

            return null;
        }

        // ---------------- SUBMIT QUERY ----------------

        // public async Task<int> SubmitQuery(Query query)
        // {
        //     try
        //     {
        //         string sql = @"INSERT INTO t_queries
        //                       (c_userid, c_title, c_description, c_priority)
        //                       VALUES (@UserId, @Title, @Description, @Priority)
        //                       RETURNING c_queryid";

        //         await _connection.OpenAsync();

        //         using var cmd = new NpgsqlCommand(sql, _connection);

        //         cmd.Parameters.AddWithValue("@UserId", query.UserId);
        //         cmd.Parameters.AddWithValue("@Title", query.Title);
        //         cmd.Parameters.AddWithValue("@Description", query.Description ?? (object)DBNull.Value);
        //         cmd.Parameters.AddWithValue("@Priority", query.Priority.ToString());

        //         int id = (int)await cmd.ExecuteScalarAsync();

        //         return id;
        //     }
        //     finally
        //     {
        //         await _connection.CloseAsync();
        //     }
        // }


        // // ---------------- GET USER QUERIES ----------------

        // public async Task<List<Query>> GetUserQueries(int userId)
        // {
        //     List<Query> queries = new();

        //     try
        //     {
        //         string sql = "SELECT * FROM t_queries WHERE c_userid=@userid";

        //         await _connection.OpenAsync();

        //         using var cmd = new NpgsqlCommand(sql, _connection);
        //         cmd.Parameters.AddWithValue("@userid", userId);

        //         using var reader = await cmd.ExecuteReaderAsync();

        //         while (await reader.ReadAsync())
        //         {
        //             queries.Add(new Query
        //             {
        //                 QueryId = reader.GetInt32(reader.GetOrdinal("c_queryid")),
        //                 UserId = reader.GetInt32(reader.GetOrdinal("c_userid")),
        //                 Title = reader.GetString(reader.GetOrdinal("c_title")),
        //                 Description = reader.IsDBNull(reader.GetOrdinal("c_description"))
        //                     ? null
        //                     : reader.GetString(reader.GetOrdinal("c_description")),

        //                 Priority = Enum.Parse<Priority>(reader.GetString(reader.GetOrdinal("c_priority"))),

        //                 QueryDate = reader.GetDateTime(reader.GetOrdinal("c_querydate")),

        //                 EmpId = reader.IsDBNull(reader.GetOrdinal("c_empid"))
        //                     ? null
        //                     : reader.GetInt32(reader.GetOrdinal("c_empid")),

        //                 Status = Enum.Parse<QueryStatus>(reader.GetString(reader.GetOrdinal("c_status"))),

        //                 Comments = reader.IsDBNull(reader.GetOrdinal("c_comments"))
        //                     ? null
        //                     : reader.GetString(reader.GetOrdinal("c_comments"))
        //             });
        //         }

        //         return queries;
        //     }
        //     finally
        //     {
        //         await _connection.CloseAsync();
        //     }
        // }


        // ---------------- UPDATE QUERY ----------------

        // public async Task<bool> UpdateQuery(Query query)
        // {
        //     try
        //     {
        //         string sql = @"UPDATE t_queries
        //                        SET c_title=@title,
        //                            c_description=@desc,
        //                            c_priority=@priority
        //                        WHERE c_queryid=@id
        //                        AND c_status <> 'Solved'";

        //         await _connection.OpenAsync();

        //         using var cmd = new NpgsqlCommand(sql, _connection);

        //         cmd.Parameters.AddWithValue("@title", query.Title);
        //         cmd.Parameters.AddWithValue("@desc", query.Description ?? (object)DBNull.Value);
        //         cmd.Parameters.AddWithValue("@priority", query.Priority.ToString());
        //         cmd.Parameters.AddWithValue("@id", query.QueryId);

        //         int rows = await cmd.ExecuteNonQueryAsync();

        //         return rows > 0;
        //     }
        //     finally
        //     {
        //         await _connection.CloseAsync();
        //     }
        // }


        // ---------------- DELETE QUERY ----------------

        // public async Task<bool> DeleteQuery(int queryId)
        // {
        //     try
        //     {
        //         string sql = @"DELETE FROM t_queries
        //                        WHERE c_queryid=@id
        //                        AND c_status <> 'Solved'";

        //         await _connection.OpenAsync();

        //         using var cmd = new NpgsqlCommand(sql, _connection);
        //         cmd.Parameters.AddWithValue("@id", queryId);

        //         int rows = await cmd.ExecuteNonQueryAsync();

        //         return rows > 0;
        //     }
        //     finally
        //     {
        //         await _connection.CloseAsync();
        //     }
        // }
    }
}
