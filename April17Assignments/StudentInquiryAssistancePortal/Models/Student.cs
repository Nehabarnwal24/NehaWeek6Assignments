namespace StudentInquiryAssistancePortal.Models
{
    public class Student
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentEmailId { get; set; }
        public long UserId { get; set; }
        public ICollection<Course> Courses { get; set; }
        public ICollection<Enquiry> Enquiries { get; set; }
        public ICollection<Admission> Admissions { get; set; }
        public User User { get; set; }
    }
}
