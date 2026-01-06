using StudentGradebook.Models;

namespace StudentGradebook.Services
{
    public interface IEnrollmentService
    {
        Task<List<Enrollment>> GetAllEnrollmentsAsync();
        Task<Enrollment> GetEnrollmentByIdAsync(int id);
        Task<List<Enrollment>> GetEnrollmentsByStudentIdAsync(int studentId);
        Task<List<Enrollment>> GetEnrollmentsByCourseIdAsync(int courseId);
        Task<bool> EnrollStudentAsync(Enrollment enrollment);
        Task<bool> UpdateEnrollmentAsync(Enrollment enrollment);
        Task<bool> DropEnrollmentAsync(int id);
        Task<bool> IsStudentEnrolledAsync(int studentId, int courseId);
        Task<List<Student>> GetAvailableStudentsForCourseAsync(int courseId);
        Task<List<Course>> GetAvailableCoursesForStudentAsync(int studentId);
    }
}