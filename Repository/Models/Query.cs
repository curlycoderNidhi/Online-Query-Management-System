using System;
using System.ComponentModel.DataAnnotations;
namespace querymanagment.Models;
public class Query
{
    [Key]
    public int QueryId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Priority is required")]
    [RegularExpression("Low|Medium|High", ErrorMessage = "Priority must be Low, Medium, or High")]
    public string Priority { get; set; }

//when query is created
    public DateTime QueryDate { get; set; } = DateTime.Now;

//assign by admin
    public int? EmpId { get; set; }

    [RegularExpression("Open|In Progress|Solved", ErrorMessage = "Invalid status")]
    public string Status { get; set; } = "Open";

//add by employee after solving query
    public string? Comments { get; set; }
}