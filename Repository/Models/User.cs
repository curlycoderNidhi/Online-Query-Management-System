using System.ComponentModel.DataAnnotations;

namespace Repository.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Company name is required.")]
        [StringLength(150, ErrorMessage = "Company name cannot be longer than 150 characters.")]
        public string CompanyName { get; set; } = "";

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 255 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}   