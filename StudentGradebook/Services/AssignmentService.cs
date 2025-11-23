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
                .Include(a => a.Grades)
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
                _context.Assignments.Update(assignment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAssignmentAsync(int id)
        {
            try
            {
                var assignment = await GetAssignmentByIdAsync(id);
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