using System;
using System.ComponentModel.DataAnnotations;
using Repository.Models.Enums;

namespace Repository.Models
{
    public class Query
    {
        public int QueryId { get; set; }

        [Required]
        public int UserId { get; set; }

        public string? Username{get;set;}
        public string? EmployeeName{get;set;}

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        public string? Description { get; set; }

        [Required]
        public Priority Priority { get; set; }

        public DateTime QueryDate { get; set; }

        public int? EmpId { get; set; }

        public QueryStatus Status { get; set; } = QueryStatus.Open;

        public string? Comments { get; set; }
    }
}