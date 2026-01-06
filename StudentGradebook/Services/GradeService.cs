using Microsoft.EntityFrameworkCore;
using StudentGradebook.Data;
using StudentGradebook.Models;

namespace StudentGradebook.Services
{
    public class GradeService : IGradeService
    {
        private readonly ApplicationDbContext _context;

        public GradeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Grade>> GetAllGradesAsync()
        {
            return await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Assignment)
                .ThenInclude(a => a.Course)
                .OrderBy(g => g.Assignment.Course.CourseCode)
                .ThenBy(g => g.Student.LastName)
                .ToListAsync();
        }

        public async Task<Grade> GetGradeByIdAsync(int id)
        {
            return await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Assignment)
                .ThenInclude(a => a.Course)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<List<Grade>> GetGradesByAssignmentIdAsync(int assignmentId)
        {
            return await _context.Grades
                .Include(g => g.Student)
                .Where(g => g.AssignmentId == assignmentId)
                .OrderBy(g => g.Student.LastName)
                .ThenBy(g => g.Student.FirstName)
                .ToListAsync();
        }

        public async Task<List<Grade>> GetGradesByStudentIdAsync(int studentId)
        {
            return await _context.Grades
                .Include(g => g.Assignment)
                .ThenInclude(a => a.Course)
                .Where(g => g.StudentId == studentId)
                .OrderBy(g => g.Assignment.Course.CourseCode)
                .ThenBy(g => g.Assignment.DueDate)
                .ToListAsync();
        }

        public async Task<bool> AddGradeAsync(Grade grade)
        {
            try
            {
                _context.Grades.Add(grade);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateGradeAsync(Grade grade)
        {
            try
            {
                _context.Grades.Update(grade);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteGradeAsync(int id)
        {
            try
            {
                var grade = await GetGradeByIdAsync(id);
                if (grade != null)
                {
                    _context.Grades.Remove(grade);
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

        public async Task<bool> GradeExistsAsync(int assignmentId, int studentId)
        {
            return await _context.Grades
                .AnyAsync(g => g.AssignmentId == assignmentId && g.StudentId == studentId);
        }

        public async Task<bool> AddOrUpdateGradeAsync(Grade grade)
        {
            try
            {
                var existingGrade = await _context.Grades
                    .FirstOrDefaultAsync(g => g.AssignmentId == grade.AssignmentId && g.StudentId == grade.StudentId);

                if (existingGrade != null)
                {
                    // Update existing grade
                    existingGrade.PointsEarned = grade.PointsEarned;
                    existingGrade.Notes = grade.Notes;
                    existingGrade.GradedDate = DateTime.Now;
                }
                else
                {
                    // Add new grade
                    grade.GradedDate = DateTime.Now;
                    _context.Grades.Add(grade);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Dictionary<int, decimal>> GetStudentGradesForCourseAsync(int studentId, int courseId)
        {
            return await _context.Grades
                .Include(g => g.Assignment)
                .Where(g => g.StudentId == studentId && g.Assignment.CourseId == courseId)
                .ToDictionaryAsync(g => g.AssignmentId, g => g.PointsEarned);
        }
    }
}