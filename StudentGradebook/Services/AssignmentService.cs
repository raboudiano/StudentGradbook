using Microsoft.EntityFrameworkCore;
using StudentGradebook.Data;
using StudentGradebook.Models;

namespace StudentGradebook.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly ApplicationDbContext _context;

        public AssignmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Assignment>> GetAllAssignmentsAsync()
        {
            return await _context.Assignments
                .Include(a => a.Course)
                .OrderBy(a => a.Course.CourseCode)
                .ThenBy(a => a.DueDate)
                .ToListAsync();
        }

        public async Task<Assignment> GetAssignmentByIdAsync(int id)
        {
            return await _context.Assignments
                .Include(a => a.Course)
                .Include(a => a.Grades)  // This will work after adding Grades to Assignment model
                .ThenInclude(g => g.Student)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<Assignment>> GetAssignmentsByCourseIdAsync(int courseId)
        {
            return await _context.Assignments
                .Include(a => a.Course)
                .Where(a => a.CourseId == courseId)
                .OrderBy(a => a.DueDate)
                .ToListAsync();
        }

        public async Task<bool> AddAssignmentAsync(Assignment assignment)
        {
            try
            {
                _context.Assignments.Add(assignment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAssignmentAsync(Assignment assignment)
        {
            try
            {
                Console.WriteLine("=== UPDATE ASSIGNMENT SERVICE ===");
                Console.WriteLine($"Updating assignment ID: {assignment.Id}");
                Console.WriteLine($"CourseId: {assignment.CourseId}, Type: {assignment.Type}");

                // Find the existing assignment
                var existingAssignment = await _context.Assignments
                    .FirstOrDefaultAsync(a => a.Id == assignment.Id);

                if (existingAssignment == null)
                {
                    Console.WriteLine("Assignment not found!");
                    return false;
                }

                // Update all properties
                existingAssignment.CourseId = assignment.CourseId;
                existingAssignment.Title = assignment.Title;
                existingAssignment.Description = assignment.Description;
                existingAssignment.MaxPoints = assignment.MaxPoints;
                existingAssignment.Weight = assignment.Weight;
                existingAssignment.Type = assignment.Type;
                existingAssignment.DueDate = assignment.DueDate;

                _context.Assignments.Update(existingAssignment);
                var result = await _context.SaveChangesAsync();

                Console.WriteLine($"Update result: {result} rows affected");
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION in UpdateAssignmentAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAssignmentAsync(int id)
        {
            try
            {
                var assignment = await _context.Assignments
                    .Include(a => a.Grades)  // Include grades to handle cascade delete if needed
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (assignment != null)
                {
                    _context.Assignments.Remove(assignment);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AssignmentExistsAsync(int id)
        {
            return await _context.Assignments.AnyAsync(a => a.Id == id);
        }
    }
}