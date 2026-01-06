using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentGradebook.Models;
using StudentGradebook.Services;

namespace StudentGradebook.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly ICourseService _courseService;
        private readonly IAssignmentService _assignmentService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly IGradeService _gradeService;

        public DashboardController(IStudentService studentService,
                                 ICourseService courseService,
                                 IAssignmentService assignmentService,
                                 IEnrollmentService enrollmentService,
                                 IGradeService gradeService)
        {
            _studentService = studentService;
            _courseService = courseService;
            _assignmentService = assignmentService;
            _enrollmentService = enrollmentService;
            _gradeService = gradeService;
        }

        // GET: Dashboard
        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel
            {
                TotalStudents = await _studentService.GetAllStudentsAsync(),
                TotalCourses = await _courseService.GetAllCoursesAsync(),
                TotalAssignments = await _assignmentService.GetAllAssignmentsAsync(),
                RecentEnrollments = await _enrollmentService.GetAllEnrollmentsAsync()
            };

            // Get statistics
            viewModel.ActiveStudentsCount = viewModel.TotalStudents.Count(s => s.IsActive);
            viewModel.ActiveCoursesCount = viewModel.TotalCourses.Count(c => c.IsActive);
            viewModel.TotalEnrollmentsCount = viewModel.RecentEnrollments.Count;

            // Get recent activity (last 10 enrollments)
            viewModel.RecentEnrollments = viewModel.RecentEnrollments
                .OrderByDescending(e => e.EnrollmentDate)
                .Take(10)
                .ToList();

            return View(viewModel);
        }

        // GET: Dashboard/StudentGrades/5
        public async Task<IActionResult> StudentGrades(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            var grades = await _gradeService.GetGradesByStudentIdAsync(id);
            var enrollments = await _enrollmentService.GetEnrollmentsByStudentIdAsync(id);

            var viewModel = new StudentGradesViewModel
            {
                Student = student,
                Grades = grades,
                Enrollments = enrollments
            };

            return View(viewModel);
        }

        // GET: Dashboard/CourseGrades/5
        public async Task<IActionResult> CourseGrades(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var assignments = await _assignmentService.GetAssignmentsByCourseIdAsync(id);
            var enrollments = await _enrollmentService.GetEnrollmentsByCourseIdAsync(id);

            var viewModel = new CourseGradesViewModel
            {
                Course = course,
                Assignments = assignments,
                Enrollments = enrollments
            };

            // Calculate grades for each student
            viewModel.StudentGrades = new List<StudentGradeSummary>();
            foreach (var enrollment in enrollments.Where(e => e.Status == EnrollmentStatus.Enrolled))
            {
                var studentGrades = await _gradeService.GetStudentGradesForCourseAsync(enrollment.StudentId, id);
                var summary = new StudentGradeSummary
                {
                    Student = enrollment.Student,
                    AssignmentGrades = studentGrades,
                    TotalPoints = studentGrades.Values.Sum(),
                    AverageGrade = studentGrades.Any() ? studentGrades.Values.Average() : 0
                };
                viewModel.StudentGrades.Add(summary);
            }

            return View(viewModel);
        }

        // ViewModels for Dashboard - MOVED INSIDE THE CONTROLLER CLASS
        public class DashboardViewModel
        {
            public List<Student> TotalStudents { get; set; }
            public List<Course> TotalCourses { get; set; }
            public List<Assignment> TotalAssignments { get; set; }
            public List<Enrollment> RecentEnrollments { get; set; }
            public int ActiveStudentsCount { get; set; }
            public int ActiveCoursesCount { get; set; }
            public int TotalEnrollmentsCount { get; set; }
        }

        public class StudentGradesViewModel
        {
            public Student Student { get; set; }
            public List<Grade> Grades { get; set; }
            public List<Enrollment> Enrollments { get; set; }
        }

        public class CourseGradesViewModel
        {
            public Course Course { get; set; }
            public List<Assignment> Assignments { get; set; }
            public List<Enrollment> Enrollments { get; set; }
            public List<StudentGradeSummary> StudentGrades { get; set; }
        }

        public class StudentGradeSummary
        {
            public Student Student { get; set; }
            public Dictionary<int, decimal> AssignmentGrades { get; set; }
            public decimal TotalPoints { get; set; }
            public decimal AverageGrade { get; set; }
        }
    }
}