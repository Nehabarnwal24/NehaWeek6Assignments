using StudentInquiryAssistancePortal.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Admission
{
    [Key]
    public int AdmissionID { get; set; }

    [Required]
    public DateTime AdmissionDate { get; set; }

    [Required, StringLength(50)]
    public string Status { get; set; }

    [ForeignKey("Student")]
    public int StudentId { get; set; }

    [ForeignKey("Course")]
    public int CourseID { get; set; }

    public Student? Student { get; set; }
    public Course? Course { get; set; }

    public ICollection<Payment>? Payments { get; set; }
}