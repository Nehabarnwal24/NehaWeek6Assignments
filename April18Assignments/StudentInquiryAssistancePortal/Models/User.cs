using StudentInquiryAssistancePortal.Models;
using System.ComponentModel.DataAnnotations;

public class User
{
    [Key]
    public long UserId { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }

    [Required, StringLength(100)]
    public string Password { get; set; }

    [Required, StringLength(100)]
    public string Username { get; set; }

    [Required, StringLength(15)]
    public string MobileNumber { get; set; }

    [Required, StringLength(20)]
    public string UserRole { get; set; }

    public Student? Student { get; set; }

    public ICollection<Course>? Courses { get; set; }
}