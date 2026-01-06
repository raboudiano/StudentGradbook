using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentGradebook.Models;
using StudentGradebook.Services;
using System.ComponentModel.DataAnnotations;

namespace StudentGradebook.Controllers
{
    [Authorize]
    public class AssignmentsController : Controller
    {
        private readonly IAssignmentService _assignmentService;
        private readonly ICourseService _courseService;
        private readonly IGradeService _gradeService;
        private readonly IEnrollmentService _enrollmentService;

        public AssignmentsController(IAssignmentService assignmentService,
                                ICourseService courseService,
                                IGradeService gradeService,
                                IEnrollmentService enrollmentService)
        {
            _assignmentService = assignmentService;
            _courseService = courseService;
            _gradeService = gradeService;
            _enrollmentService = enrollmentService;
        }

        // ViewModel for creating/editing assignments
        public class AssignmentViewModel
        {
            public int Id { get; set; }

           
            public int CourseId { get; set; }

            [Required]
            [StringLength(100)]
            public string Title { get; set; }

            public string Description { get; set; }

            [Range(0, 1000)]
            public decimal MaxPoints { get; set; }

            [Range(0, 100)]
            public decimal Weight { get; set; }

            [Required]
            public string Type { get; set; }

            [DataType(DataType.Date)]
            public DateTime DueDate { get; set; }
        }

        // ViewModel for entering grades
        public class EnterGradesViewModel
        {
            public Assignment Assignment { get; set; }
            public List<Student> Students { get; set; }
            public Dictionary<int, Grade> ExistingGrades { get; set; }
        }

        // GET: Assignments
        public async Task<IActionResult> Index()
        {
            var assignments = await _assignmentService.GetAllAssignmentsAsync();
            return View(assignments);
        }

        // GET: Assignments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var assignment = await _assignmentService.GetAssignmentByIdAsync(id);
            if (assignment == null)
            {
                return NotFound();
            }

            var grades = await _gradeService.GetGradesByAssignmentIdAsync(id);
            ViewBag.Grades = grades;

            return View(assignment);
        }

        // GET: Assignments/Create
        public async Task<IActionResult> Create()
        {
            await PopulateCoursesViewData();
            return View();
        }

        // POST: Assignments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssignmentViewModel viewModel)
        {
            Console.WriteLine("=== ASSIGNMENT CREATE DEBUG ===");
            Console.WriteLine($"ModelState IsValid: {ModelState.IsValid}");
            Console.WriteLine($"CourseId: {viewModel.CourseId}");

            if (ModelState.IsValid)
            {
                Console.WriteLine("Attempting to save assignment...");

                // Convert ViewModel to Entity
                var assignment = new Assignment
                {
                    CourseId = viewModel.CourseId,
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    MaxPoints = viewModel.MaxPoints,
                    Weight = viewModel.Weight,
                    Type = viewModel.Type,
                    DueDate = viewModel.DueDate
                };

                var result = await _assignmentService.AddAssignmentAsync(assignment);
                if (result)
                {
                    TempData["SuccessMessage"] = "Assignment created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Error creating assignment. Please try again.");
            }
            else
            {
                Console.WriteLine("=== MODEL STATE ERRORS ===");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors)
                    {
                        Console.WriteLine($"{key}: {error.ErrorMessage}");
                    }
                }
            }

            await PopulateCoursesViewData();
            return View(viewModel);
        }

        // GET: Assignments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var assignment = await _assignmentService.GetAssignmentByIdAsync(id);
            if (assignment == null)
            {
                return NotFound();
            }

            // Convert Entity to ViewModel
            var viewModel = new AssignmentViewModel
            {
                Id = assignment.Id,
                CourseId = assignment.CourseId,
                Title = assignment.Title,
                Description = assignment.Description,
                MaxPoints = assignment.MaxPoints,
                Weight = assignment.Weight,
                Type = assignment.Type,
                DueDate = assignment.DueDate
            };

            await PopulateCoursesViewData();
            return View(viewModel);
        }

        
        // GET: Assignments/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var assignment = await _assignmentService.GetAssignmentByIdAsync(id);
            if (assignment == null)
            {
                return NotFound();
            }
            return View(assignment);
        }

        // POST: Assignments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _assignmentService.DeleteAssignmentAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Assignment deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Error deleting assignment. Please try again.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Assignments/EnterGrades/5
        public async Task<IActionResult> EnterGrades(int id)
        {
            var assignment = await _assignmentService.GetAssignmentByIdAsync(id);
            if (assignment == null)
            {
                return NotFound();
            }

            // Get enrolled students for this course using EnrollmentService
            var enrollments = await _enrollmentService.GetEnrollmentsByCourseIdAsync(assignment.CourseId);
            var students = enrollments
                .Where(e => e.Status == EnrollmentStatus.Enrolled)
                .Select(e => e.Student)
                .ToList();

            // Get existing grades
            var existingGrades = await _gradeService.GetGradesByAssignmentIdAsync(id);
            var gradeDictionary = existingGrades.ToDictionary(g => g.StudentId, g => g);

            var viewModel = new EnterGradesViewModel
            {
                Assignment = assignment,
                Students = students,
                ExistingGrades = gradeDictionary
            };

            return View(viewModel);
        }

        // POST: Assignments/EnterGrades/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnterGrades(int id, Dictionary<int, decimal> grades, Dictionary<int, string> notes)
        {
            var assignment = await _assignmentService.GetAssignmentByIdAsync(id);
            if (assignment == null)
            {
                return NotFound();
            }

            bool allSaved = true;

            if (grades != null)
            {
                foreach (var (studentId, pointsEarned) in grades)
                {
                    var grade = new Grade
                    {
                        AssignmentId = id,
                        StudentId = studentId,
                        PointsEarned = pointsEarned,
                        Notes = notes != null && notes.ContainsKey(studentId) ? notes[studentId] : null,
                        GradedDate = DateTime.Now
                    };

                    var result = await _gradeService.AddOrUpdateGradeAsync(grade);
                    if (!result)
                    {
                        allSaved = false;
                    }
                }
            }

            if (allSaved)
            {
                TempData["SuccessMessage"] = "Grades saved successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Some grades may not have been saved. Please check and try again.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }


        private async Task PopulateCoursesViewData()
        {
            var courses = await _courseService.GetActiveCoursesAsync();
            Console.WriteLine($"PopulateCoursesViewData - Courses count: {courses.Count()}");
            foreach (var course in courses)
            {
                Console.WriteLine($"Available course: {course.Id} - {course.Name}");
            }
            ViewBag.CourseId = new SelectList(courses, "Id", "Name");
        }
    }
}