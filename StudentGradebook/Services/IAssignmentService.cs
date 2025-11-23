using StudentGradebook.Models;

namespace StudentGradebook.Services
{
    public interface IAssignmentService
    {
        Task<List<Assignment>> GetAllAssignmentsAsync();
        Task<Assignment> GetAssignmentByIdAsync(int id);
        Task<List<Assignment>> GetAssignmentsByCourseIdAsync(int courseId);
        Task<bool> AddAssignmentAsync(Assignment assignment);
        Task<bool> UpdateAssignmentAsync(Assignment assignment);
        Task<bool> DeleteAssignmentAsync(int id);
        Task<bool> AssignmentExistsAsync(int id);
    }
}