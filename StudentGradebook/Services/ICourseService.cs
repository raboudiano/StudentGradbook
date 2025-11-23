using StudentGradebook.Models;

namespace StudentGradebook.Services
{
    public interface ICourseService
    {
        Task<List<Course>> GetAllCoursesAsync();
        Task<Course> GetCourseByIdAsync(int id);
        Task<Course> GetCourseByCodeAsync(string courseCode);
        Task<bool> AddCourseAsync(Course course);
        Task<bool> UpdateCourseAsync(Course course);
        Task<bool> DeleteCourseAsync(int id);
        Task<bool> CourseCodeExistsAsync(string courseCode);
        Task<List<Course>> GetActiveCoursesAsync();
    }
}