using System.ComponentModel.DataAnnotations;

namespace Repository.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Employee name is required.")]
        public string EmpName { get; set; } = string.Empty;

        // Backward compatibility for hot-reload/runtime caches still looking for get_Email().
        public string Email
        {
            get => EmpName;
            set => EmpName = value ?? string.Empty;
        }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
