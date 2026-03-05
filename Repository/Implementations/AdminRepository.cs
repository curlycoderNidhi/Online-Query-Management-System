using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using repositories.Interfaces;
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


    }
}