using System.ComponentModel.DataAnnotations;

namespace StudentGradebook.Models
{
    public class Enrollment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a student.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a student.")]
        [Display(Name = "Student")]
        public int StudentId { get; set; }

        public Student? Student { get; set; }

        [Required(ErrorMessage = "Please select a course.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a course.")]
        [Display(Name = "Course")]
        public int CourseId { get; set; }

        public Course? Course { get; set; }

        [Required(ErrorMessage = "Please enter an enrollment date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Enrollment Date")]
        public DateTime EnrollmentDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Please select a status.")]
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