using System.ComponentModel.DataAnnotations;

namespace StudentGradebook.Models
{
    public class Grade
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; }

        public int AssignmentId { get; set; }
        public Assignment Assignment { get; set; }

        [Range(0, 1000)]
        public decimal PointsEarned { get; set; }

        [DataType(DataType.Date)]
        public DateTime GradedDate { get; set; } = DateTime.Now;

        public string Notes { get; set; }

        public decimal Percentage => Assignment?.MaxPoints > 0 ? (PointsEarned / Assignment.MaxPoints) * 100 : 0;
    }
}