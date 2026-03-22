using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repository.Models;

namespace Repository.Interfaces
{
    public interface IEmployeeInterface
    {
        Task<List<Query>> GetEmployeeQueries(int empid);

        Task<bool> UpdateQueryStatus(Query model);

        Task<int> GetResolvedCount(int empid);

        Task<int> GetPendingCount(int empid);

        Task<int> GetAssignedCount(int empid);

        Task<int> GetTodayResolvedCount(int empid);

        Task<Employee?> Login(string empName, string password);
        Task<Query?> GetQueryById(int queryId);
    }
}
