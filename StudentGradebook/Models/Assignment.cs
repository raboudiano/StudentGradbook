using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace StudentGradebook.Models
{
    public class Assignment
    {
        public int Id { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Range(0, 1000)]
        public decimal MaxPoints { get; set; } = 100;

        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(7);

        [Range(0, 100)]
        public decimal Weight { get; set; } = 10;

        public AssignmentType Type { get; set; } = AssignmentType.Homework;

        // Navigation properties
        public ICollection<Grade> Grades { get; set; } = new List<Grade>();
    }

    public enum AssignmentType
    {
        Homework,
        Quiz,
        Exam,
        Project,
        Participation
    }
}