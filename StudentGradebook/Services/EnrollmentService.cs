using Microsoft.EntityFrameworkCore;
using StudentGradebook.Data;
using StudentGradebook.Models;

namespace StudentGradebook.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Enrollment>> GetAllEnrollmentsAsync()
        {
            return await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .OrderBy(e => e.Course.CourseCode)
                .ThenBy(e => e.Student.LastName)
                .ToListAsync();
        }

        public async Task<Enrollment> GetEnrollmentByIdAsync(int id)
        {
            return await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Enrollment>> GetEnrollmentsByStudentIdAsync(int studentId)
        {
            return await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.StudentId == studentId)
                .OrderBy(e => e.Course.CourseCode)
                .ToListAsync();
        }

        public async Task<List<Enrollment>> GetEnrollmentsByCourseIdAsync(int courseId)
        {
            return await _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.CourseId == courseId)
                .OrderBy(e => e.Student.LastName)
                .ThenBy(e => e.Student.FirstName)
                .ToListAsync();
        }

        public async Task<bool> EnrollStudentAsync(Enrollment enrollment)
        {
            try
            {
                Console.WriteLine("=== ENROLLMENT SERVICE ===");
                Console.WriteLine($"Adding enrollment: Student={enrollment.StudentId}, Course={enrollment.CourseId}");

                // Basic validation
                if (enrollment.StudentId == 0 || enrollment.CourseId == 0)
                {
                    Console.WriteLine("Invalid StudentId or CourseId");
                    return false;
                }

                _context.Enrollments.Add(enrollment);
                Console.WriteLine("Enrollment added to context, saving changes...");

                var result = await _context.SaveChangesAsync();
                Console.WriteLine($"SaveChangesAsync result: {result}");

                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION in EnrollStudentAsync: {ex.Message}");
                Console.WriteLine($"INNER EXCEPTION: {ex.InnerException?.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateEnrollmentAsync(Enrollment enrollment)
        {
            try
            {
                Console.WriteLine("=== UPDATE ENROLLMENT SERVICE ===");
                Console.WriteLine($"Updating enrollment ID: {enrollment.Id}, Status: {enrollment.Status}");

                var existingEnrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.Id == enrollment.Id);

                if (existingEnrollment == null)
                {
                    Console.WriteLine("Enrollment not found in database!");
                    return false;
                }

                // Only update the fields that should be editable
                existingEnrollment.Status = enrollment.Status;

                Console.WriteLine("Saving changes...");
                var result = await _context.SaveChangesAsync();
                Console.WriteLine($"Save result: {result} changes");

                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION in UpdateEnrollmentAsync: {ex.Message}");
                Console.WriteLine($"INNER EXCEPTION: {ex.InnerException?.Message}");
                return false;
            }
        }

        public async Task<bool> DropEnrollmentAsync(int id)
        {
            try
            {
                var enrollment = await GetEnrollmentByIdAsync(id);
                if (enrollment != null)
                {
                    enrollment.Status = EnrollmentStatus.Dropped;
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

        public async Task<bool> IsStudentEnrolledAsync(int studentId, int courseId)
        {
            return await _context.Enrollments
                .AnyAsync(e => e.StudentId == studentId &&
                              e.CourseId == courseId &&
                              e.Status == EnrollmentStatus.Enrolled);
        }

        public async Task<List<Student>> GetAvailableStudentsForCourseAsync(int courseId)
        {
            var enrolledStudentIds = await _context.Enrollments
                .Where(e => e.CourseId == courseId && e.Status == EnrollmentStatus.Enrolled)
                .Select(e => e.StudentId)
                .ToListAsync();

            return await _context.Students
                .Where(s => s.IsActive && !enrolledStudentIds.Contains(s.Id))
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();
        }

        public async Task<List<Course>> GetAvailableCoursesForStudentAsync(int studentId)
        {
            var enrolledCourseIds = await _context.Enrollments
                .Where(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Enrolled)
                .Select(e => e.CourseId)
                .ToListAsync();

            return await _context.Courses
                .Where(c => c.IsActive && !enrolledCourseIds.Contains(c.Id))
                .OrderBy(c => c.CourseCode)
                .ToListAsync();
        }
    }
}