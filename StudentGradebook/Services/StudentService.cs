using Microsoft.EntityFrameworkCore;
using StudentGradebook.Data;
using StudentGradebook.Models;

namespace StudentGradebook.Services
{
    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _context;

        public StudentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Student>> GetAllStudentsAsync()
        {
            return await _context.Students
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();
        }

        public async Task<Student> GetStudentByIdAsync(int id)
        {
            return await _context.Students
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Student> GetStudentByStudentIdAsync(string studentId)
        {
            return await _context.Students
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
        }

        public async Task<bool> AddStudentAsync(Student student)
        {
            try
            {
                _context.Students.Add(student);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateStudentAsync(Student student)
        {
            try
            {
                _context.Students.Update(student);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteStudentAsync(int id)
        {
            try
            {
                var student = await GetStudentByIdAsync(id);
                if (student != null)
                {
                    // Soft delete by setting IsActive to false
                    student.IsActive = false;
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

        public async Task<bool> StudentIdExistsAsync(string studentId)
        {
            return await _context.Students
                .AnyAsync(s => s.StudentId == studentId);
        }
    }
}