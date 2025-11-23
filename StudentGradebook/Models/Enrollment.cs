using System.ComponentModel.DataAnnotations;

namespace StudentGradebook.Models
{
    public class Enrollment
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        [DataType(DataType.Date)]
        public DateTime EnrollmentDate { get; set; } = DateTime.Now;

        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Enrolled;
    }

    public enum EnrollmentStatus
    {
        Enrolled,
        Completed,
        Dropped,
        Withdrawn
    }
}