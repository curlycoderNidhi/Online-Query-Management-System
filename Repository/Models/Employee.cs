using System.ComponentModel.DataAnnotations;
using Repository.Models.Enums;

namespace Repository.Models
{
    public class Employee
    {
        public int EmpId { get; set; }

        [Required(ErrorMessage = "Employee name is required.")]
        [StringLength(100, ErrorMessage = "Employee name cannot exceed 100 characters.")]
        public string EmpName { get; set; } = "";

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(255)]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Role is required.")]
        public Role Role { get; set; }
    }
}