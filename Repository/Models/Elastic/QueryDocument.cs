using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Models.Elastic
{
    public class QueryDocument
    {
        public int QueryId { get; set; }
        public int UserId { get; set; }
        public string? CompanyName { get; set; }   // populated from AdminQuery.Username (t_users.c_companyname)
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string Priority { get; set; } = "";
        public DateTime QueryDate { get; set; }
        public int? EmpId { get; set; }
        public string? EmployeeName { get; set; }  // populated from AdminQuery.EmployeeName (t_employee.c_empname)
        public string Status { get; set; } = "Open";
        public string? Comments { get; set; }
    }
}