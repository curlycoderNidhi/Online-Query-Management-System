using System.Collections.Generic;
using System.Threading.Tasks;
using Repository.Models;

namespace Repositories.Interfaces
{
    public interface IAdminInterface
    {
    
        Task<List<AdminQuery>> GetAllQueries();
        Task<List<AdminQuery>> GetAllQueriesSolved();
        Task<List<AdminQuery>> GetAllQueriesInProgress();
        Task<List<AdminQuery>> GetAllQueriesOpen();


        Task<Dictionary<string,int>> GetDashboardCards();
        Task<List<EmployeePerformance>> GetEmployeePerformance();

     
        Task<List<User>> GetAllUsers();
        Task<User> GetUserDetails(int id);
       Task<List<AdminQuery>> GetSubmittedQueries(int id);

      
        Task<List<Employee>> GetAllEmployees();
        Task<int> CreateEmployee(Employee employee);

       
        Task<int> AssignEmployee(int queryId, int empId);
    }
}
