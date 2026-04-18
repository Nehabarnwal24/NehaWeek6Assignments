using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StudentInquiryAssistancePortal.Models;
public class Payment
{
    [Key]
    public int PaymentID { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; }

    [Range(1, 1000000)]
    public int Amount { get; set; }

    [Required, StringLength(50)]
    public string PaymentMode { get; set; }

    [ForeignKey("Student")]
    public int StudentId { get; set; }

    [ForeignKey("Admission")]
    public int AdmissionID { get; set; }

    public Student? Student { get; set; }
    public Admission? Admission { get; set; }
}