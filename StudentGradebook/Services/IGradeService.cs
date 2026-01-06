using StudentGradebook.Models;

namespace StudentGradebook.Services
{
    public interface IGradeService
    {
        Task<List<Grade>> GetAllGradesAsync();
        Task<Grade> GetGradeByIdAsync(int id);
        Task<List<Grade>> GetGradesByAssignmentIdAsync(int assignmentId);
        Task<List<Grade>> GetGradesByStudentIdAsync(int studentId);
        Task<bool> AddGradeAsync(Grade grade);
        Task<bool> UpdateGradeAsync(Grade grade);
        Task<bool> DeleteGradeAsync(int id);
        Task<bool> GradeExistsAsync(int assignmentId, int studentId);
        Task<bool> AddOrUpdateGradeAsync(Grade grade);
        Task<Dictionary<int, decimal>> GetStudentGradesForCourseAsync(int studentId, int courseId);
    }
}