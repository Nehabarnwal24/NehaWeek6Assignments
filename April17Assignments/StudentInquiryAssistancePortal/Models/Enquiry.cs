using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StudentInquiryAssistancePortal.Models;
public class Enquiry
{
    [Key]
    public int EnquiryID { get; set; }

    [Required]
    public DateTime EnquiryDate { get; set; }

    [Required, StringLength(150)]
    public string Title { get; set; }

    [StringLength(500)]
    public string Description { get; set; }

    [Required, StringLength(50)]
    public string EnquiryType { get; set; }

    [ForeignKey("Student")]
    public int StudentId { get; set; }

    [ForeignKey("Course")]
    public int CourseID { get; set; }

    public Student? Student { get; set; }
    public Course? Course { get; set; }
}