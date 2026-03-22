using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repository.Models;

namespace Repository.Interfaces
{
    public interface IElasticSearchService
    {
        Task EnsureIndexAsync();
        Task IndexQueryAsync(Query query);
        Task UpdateQueryAsync(Query query);
        Task UpsertAdminQueryAsync(AdminQuery query);
        Task DeleteQueryAsync(int queryId);

        // Used by employee-side search (returns base Query — no company/emp name needed there)
        Task<List<Query>> SearchByTitleAsync(string titleKeyword);
        Task<List<Query>> SearchEmployeeQueriesAsync(int empId, string keyword);
        Task<List<Query>> FilterByStatusAsync(string status);
        Task<List<Query>> FilterByDateRangeAsync(DateTime from, DateTime to);

        // Used by admin dashboard search (returns AdminQuery — includes CompanyName + EmployeeName)
        Task<List<AdminQuery>> AdminSearchAsync(string? keyword, string? status, DateTime? from, DateTime? to);
    }
}