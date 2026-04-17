using StudentInquiryAssistancePortal.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Course
{
    [Key]
    public int CourseID { get; set; }

    [Required, StringLength(100)]
    public string CourseName { get; set; }

    [StringLength(500)]
    public string Description { get; set; }

    [Required, StringLength(50)]
    public string Duration { get; set; }

    [Range(0, 1000000)]
    public int FeesAmount { get; set; }

    public ICollection<Student>? Students { get; set; }
    public ICollection<Enquiry>? Enquiries { get; set; }
    public ICollection<Admission>? Admissions { get; set; }
    public ICollection<Payment>? Payments { get; set; }
}