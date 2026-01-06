using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentGradebook.Models
{
    public class Assignment
    {
        public int Id { get; set; }

        
        public int CourseId { get; set; }

        // Navigation property
        public virtual Course Course { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Range(0, 1000)]
        public decimal MaxPoints { get; set; }

        [Range(0, 100)]
        public decimal Weight { get; set; }

        [Required]
        public string Type { get; set; }

        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        // Navigation property for Grades
        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
    }
}