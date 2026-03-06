using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repository.Models;

namespace Repositories.Interfaces
{
    public interface IAdminInterface
    {
        // Query Management
        Task<List<Query>> GetAllQueries();
        Task<List<Query>> GetAllQueriesSolved();
        Task<List<Query>> GetAllQueriesInProgress();
        Task<List<Query>> GetAllQueriesOpen();

        // Dashboard
        Task<Dictionary<string,int>> GetDashboardCards();
        Task<List<EmployeePerformance>> GetEmployeePerformance();

        // User Management
        Task<List<User>> GetAllUsers();
        Task<User> GetUserDetails(int id);

        // User Queries
        Task<List<Query>> GetSubmittedQueries(int id);

        // Query Assignment
        Task<int> AssignEmployee(int queryId,int empId);
    }
}