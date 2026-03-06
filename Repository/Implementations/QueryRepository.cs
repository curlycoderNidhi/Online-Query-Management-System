using Npgsql;
using querymanagment.Models;
namespace querymanagment.Models;

public class QueryRepository : IQueryInterface
{
    private readonly NpgsqlConnection _conn;
    public QueryRepository(NpgsqlConnection conn)
    {
        _conn = conn;
    }
    public async Task<int> Create(Query query)
    {
        string qry = "INSERT INTO t_queries (c_userid, c_title, c_description, c_priority) VALUES (@c_userid, @c_title, @c_description, @c_priority);";

        try
        {
            using (NpgsqlCommand cmd = new NpgsqlCommand(qry, _conn))
            {
                cmd.Parameters.AddWithValue("@c_userid", Convert.ToInt32(query.UserId));
                cmd.Parameters.AddWithValue("@c_title", query.Title);
                cmd.Parameters.AddWithValue("@c_description", query.Description);
                cmd.Parameters.AddWithValue("@c_priority", query.Priority);
                await _conn.OpenAsync();
                int row = await cmd.ExecuteNonQueryAsync();

                return row > 0 ? 1 : 0;
            }
        }
        catch (Exception e)
        {
            System.Console.WriteLine("Error in create query repo:" + e.Message);
            return 0;
        }
        finally
        {
            await _conn.CloseAsync();
        }
    }

    public async Task<int> Delete(int id)
    {
        string qry = "DELETE FROM t_queries WHERE c_queryid = @c_queryid";

        try
        {
            using (var cmd = new NpgsqlCommand(qry, _conn))
            {
                cmd.Parameters.AddWithValue("c_queryid", Convert.ToInt32(id));
                await _conn.OpenAsync();
                int row = await cmd.ExecuteNonQueryAsync();
                return row;
            }
        }
        catch (Exception e)
        {
            System.Console.WriteLine("Error in delete query repo:" + e.Message);
            return 0;
        }
        finally
        {
            await _conn.CloseAsync();
        }
    }

    public async Task<List<Query>> GetAll()
    {
        List<Query> queries = new List<Query>();

        string qry = @"SELECT 
                    c_queryid,
                    c_userid,
                    c_title,
                    c_description,
                    c_priority,
                    c_querydate,
                    c_empid,
                    c_status,
                    c_comments
                 FROM t_queries;";

        try
        {
            using (var cmd = new NpgsqlCommand(qry, _conn))
            {
                await _conn.OpenAsync();
                var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    queries.Add(new Query
                    {
                        QueryId = Convert.ToInt32(reader["c_queryid"]),
                        UserId = Convert.ToInt32(reader["c_userid"]),
                        Title = reader["c_title"].ToString(),
                        Description = reader["c_description"].ToString(),
                        Priority = reader["c_priority"].ToString(),
                        QueryDate = Convert.ToDateTime(reader["c_querydate"]),
                        EmpId = reader["c_empid"] == DBNull.Value ? null : Convert.ToInt32(reader["c_empid"]),
                        Status = reader["c_status"].ToString(),
                        Comments = reader["c_comments"] == DBNull.Value ? null : reader["c_comments"].ToString()
                    });
                }

                return queries;
            }
        }
        catch (Exception e)
        {
            System.Console.WriteLine("Error in Get all query repo: " + e.Message);
            queries = null;
            return queries;
        }
        finally
        {
            await _conn.CloseAsync();
        }
    }

    public async Task<List<Query>> GetByEmployeeId(int empid)
    {
        List<Query> queriesByEmp = new List<Query>();
        string qry = @"SELECT 
                    c_queryid,
                    c_userid,
                    c_title,
                    c_description,
                    c_priority,
                    c_querydate,
                    c_empid,
                    c_status,
                    c_comments
                 FROM t_queries
                 WHERE c_empid = @c_empid;";

        try
        {
            using (var cmd = new NpgsqlCommand(qry, _conn))
            {
                cmd.Parameters.AddWithValue("c_empid", Convert.ToInt32(empid));
                await _conn.OpenAsync();
                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    queriesByEmp.Add(new Query
                    {
                        QueryId = Convert.ToInt32(reader["c_queryid"]),
                        UserId = Convert.ToInt32(reader["c_userid"]),
                        Title = reader["c_title"].ToString(),
                        Description = reader["c_description"].ToString(),
                        Priority = reader["c_priority"].ToString(),
                        QueryDate = Convert.ToDateTime(reader["c_querydate"]),
                        EmpId = reader["c_empid"] == DBNull.Value ? null : Convert.ToInt32(reader["c_empid"]),
                        Status = reader["c_status"].ToString(),
                        Comments = reader["c_comments"] == DBNull.Value ? null : reader["c_comments"].ToString()
                    });
                }

                return queriesByEmp;
            }
        }
        catch (Exception e)
        {
            System.Console.WriteLine("Error in Query by emp repo :" + e.Message);
            return null;
        }
        finally
        {
            await _conn.CloseAsync();
        }
    }

    public async Task<Query> GetById(int id)
    {
        Query query = new Query();

        string qry = @"SELECT 
                    c_queryid,
                    c_userid,
                    c_title,
                    c_description,
                    c_priority,
                    c_querydate,
                    c_empid,
                    c_status,
                    c_comments
                 FROM t_queries
                 WHERE c_queryid = @c_queryid;";

        try
        {
            using (var cmd = new NpgsqlCommand(qry, _conn))
            {
                cmd.Parameters.AddWithValue("c_queryid", Convert.ToInt32(id));

                await _conn.OpenAsync();
                var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    query = new Query
                    {
                        QueryId = Convert.ToInt32(reader["c_queryid"]),
                        UserId = Convert.ToInt32(reader["c_userid"]),
                        Title = reader["c_title"].ToString(),
                        Description = reader["c_description"].ToString(),
                        Priority = reader["c_priority"].ToString(),
                        QueryDate = Convert.ToDateTime(reader["c_querydate"]),
                        EmpId = reader["c_empid"] == DBNull.Value ? null : Convert.ToInt32(reader["c_empid"]),
                        Status = reader["c_status"].ToString(),
                        Comments = reader["c_comments"] == DBNull.Value ? null : reader["c_comments"].ToString()
                    };
                }
                else
                {
                    query = null;
                }

                return query;
            }
        }
        catch (Exception e)
        {
            System.Console.WriteLine("Error in Get query by id repo :" + e.Message);
            return null;
        }
        finally
        {
            await _conn.CloseAsync();
        }
    }

    public async Task<List<Query>> GetByUserId(int userid)
    {
        List<Query> queriesByUser = new List<Query>();

        string qry = @"SELECT 
                    c_queryid,
                    c_userid,
                    c_title,
                    c_description,
                    c_priority,
                    c_querydate,
                    c_empid,
                    c_status,
                    c_comments
                 FROM t_queries
                 WHERE c_userid = @c_userid;";

        try
        {
            using (var cmd = new NpgsqlCommand(qry, _conn))
            {
                cmd.Parameters.AddWithValue("c_userid", userid);
                await _conn.OpenAsync();
                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    queriesByUser.Add(new Query
                    {
                        QueryId = Convert.ToInt32(reader["c_queryid"]),
                        UserId = Convert.ToInt32(reader["c_userid"]),
                        Title = reader["c_title"].ToString(),
                        Description = reader["c_description"].ToString(),
                        Priority = reader["c_priority"].ToString(),
                        QueryDate = Convert.ToDateTime(reader["c_querydate"]),
                        EmpId = reader["c_empid"] == DBNull.Value ? null : Convert.ToInt32(reader["c_empid"]),
                        Status = reader["c_status"].ToString(),
                        Comments = reader["c_comments"] == DBNull.Value ? null : reader["c_comments"].ToString()
                    });
                }

                return queriesByUser;
            }
        }
        catch (Exception e)
        {
            System.Console.WriteLine("Error in Get query by user repo :" + e.Message);
            return null;
        }
        finally
        {
            await _conn.CloseAsync();
        }
    }

    // this is for user
    public async Task<int> Update(Query query)
    {

        string qry = @"UPDATE t_queries
                           SET c_title = @c_title,
                               c_description = @c_description,
                               c_priority = @c_priority
                           WHERE c_queryid = @c_queryid;";

        try
        {
            using (var cmd = new NpgsqlCommand(qry, _conn))
            {
                cmd.Parameters.AddWithValue("@c_title", query.Title);
                cmd.Parameters.AddWithValue("@c_description", query.Description);
                cmd.Parameters.AddWithValue("@c_priority", query.Priority);
                cmd.Parameters.AddWithValue("@c_queryid", query.QueryId);

                await _conn.OpenAsync();

                int row = await cmd.ExecuteNonQueryAsync();

                return row > 0 ? 1 : 0;
            }
        }
        catch (Exception e)
        {
            System.Console.WriteLine("error while updating a query");
            return 0;
        }
        finally
        {
            await _conn.CloseAsync();
        }
    }

    // this is for Employee
    public async Task<int> UpdateStatus(int queryid,int empid, string status,string? comment)
    {
        string getQueryStatus = "SELECT c_status FROM t_queries WHERE c_queryid = @c_queryid AND c_empid = @c_empid;";
        string updateQuery = "UPDATE t_queries SET c_status = @c_status , c_comments = @c_comments WHERE c_queryid = @c_queryid AND c_empid = @c_empid;";
        string currentStatus = "";

        // First try block - just read current status
        try
        {
            await _conn.OpenAsync();
            using var GetStatuscmd = new NpgsqlCommand(getQueryStatus, _conn);
            GetStatuscmd.Parameters.AddWithValue("@c_queryid", queryid);
            GetStatuscmd.Parameters.AddWithValue("@c_empid", empid);
            

            var reader = await GetStatuscmd.ExecuteScalarAsync();

            if (reader == null) return 0;
            currentStatus = reader.ToString();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error reading status: " + e.Message);
            return 0;
        }
        finally
        {
            await _conn.CloseAsync();
        }

        // Employee cannot skip the steps
        if (currentStatus == "Open" && status != "In Progress") return 0;
        if (currentStatus == "In Progress" && status != "Solved") return 0;
        if(currentStatus == "Solved")return 0;

        // Second try block - actual update
        try
        {
            await _conn.OpenAsync();
            using var updateCmd = new NpgsqlCommand(updateQuery, _conn);
            updateCmd.Parameters.AddWithValue("@c_status", status);
            updateCmd.Parameters.AddWithValue("@c_queryid", queryid);
            updateCmd.Parameters.AddWithValue("@c_empid" , empid);
            updateCmd.Parameters.AddWithValue("@c_comments" , comment);

            int row = await updateCmd.ExecuteNonQueryAsync();
            return row > 0 ? 1 : 0;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error updating status: " + e.Message);
            return 0;
        }
        finally
        {
            await _conn.CloseAsync();
        }
    }

    // this is for employee
    // public async Task<int> AddComment(int queryid, string comment)
    // {
    //     string qry = @"UPDATE t_queries
    //                  SET c_comments = @c_comments
    //                  WHERE c_queryid = @c_queryid;";

    //     try
    //     {
    //         using (var cmd = new NpgsqlCommand(qry, _conn))
    //         {
    //             cmd.Parameters.AddWithValue("c_comments", comment);
    //             cmd.Parameters.AddWithValue("c_queryid", queryid);
    //             await _conn.OpenAsync();
    //             int row = await cmd.ExecuteNonQueryAsync();
    //             return row > 0 ? 1 : 0;
    //         }
    //     }
    //     catch (Exception e)
    //     {
    //         System.Console.WriteLine("error in add comment repo :" + e.Message);
    //         return 0;
    //     }
    //     finally
    //     {
    //         await _conn.CloseAsync();
    //     }
    // }

    // this is for admin
    public async Task<int> AssignEmployee(int queryid, int empid)
    {
        string query = @"UPDATE t_queries
                     SET c_empid = @c_empid
                     WHERE c_queryid = @c_queryid";

        try
        {
            using var cmd = new NpgsqlCommand(query, _conn);

            cmd.Parameters.AddWithValue("@c_empid", empid);
            cmd.Parameters.AddWithValue("@c_queryid", queryid);
            await _conn.OpenAsync();

            int row = await cmd.ExecuteNonQueryAsync();

            return row > 0 ? 1 : 0;
        }
        catch (Exception e)
        {
            System.Console.WriteLine("error in assign Employee repo");
            return 0;
        }
        finally
        {
            await _conn.CloseAsync();
        }
    }

    public async Task<List<Query>> GetUnassignedQueries()
    {
        List<Query> queries = new List<Query>();

        string query = @"SELECT 
                        c_queryid,
                        c_userid,
                        c_title,
                        c_description,
                        c_priority,
                        c_querydate,
                        c_empid,
                        c_status,
                        c_comments
                     FROM t_queries
                     WHERE c_empid IS NULL
                     AND c_status = 'Open';";

        try
        {
            using var cmd = new NpgsqlCommand(query, _conn);
            await _conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                queries.Add(new Query
                {
                    QueryId = Convert.ToInt32(reader["c_queryid"]),
                    UserId = Convert.ToInt32(reader["c_userid"]),
                    Title = reader["c_title"].ToString(),
                    Description = reader["c_description"].ToString(),
                    Priority = reader["c_priority"].ToString(),
                    QueryDate = Convert.ToDateTime(reader["c_querydate"]),
                    EmpId = reader["c_empid"] == DBNull.Value ? null : Convert.ToInt32(reader["c_empid"]),
                    Status = reader["c_status"].ToString(),
                    Comments = reader["c_comments"] == DBNull.Value ? null : reader["c_comments"].ToString()
                });
            }

            return queries;
        }
        catch (Exception e)
        {
            System.Console.WriteLine("error in get unassign query repo :" + e.Message);
            return null;
        }
        finally
        {
            await _conn.CloseAsync();
        }
    }
}