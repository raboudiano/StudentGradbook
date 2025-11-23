using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentGradebook.Models;
using StudentGradebook.Services;

namespace StudentGradebook.Controllers
{
    [Authorize]
    public class EnrollmentsController : Controller
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly IStudentService _studentService;
        private readonly ICourseService _courseService;

        public EnrollmentsController(IEnrollmentService enrollmentService,
                                   IStudentService studentService,
                                   ICourseService courseService)
        {
            _enrollmentService = enrollmentService;
            _studentService = studentService;
            _courseService = courseService;
        }

        // GET: Enrollments
        public async Task<IActionResult> Index()
        {
            var enrollments = await _enrollmentService.GetAllEnrollmentsAsync();
            return View(enrollments);
        }

        // GET: Enrollments/Create  <-- ADD THIS METHOD
        public async Task<IActionResult> Create()
        {
            await PopulateViewData();
            return View();
        }

        // POST: Enrollments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Enrollment enrollment)
        {
            Console.WriteLine("=== CREATE ENROLLMENT ===");
            Console.WriteLine($"ModelState IsValid: {ModelState.IsValid}");
            Console.WriteLine($"StudentId: {enrollment.StudentId}");
            Console.WriteLine($"CourseId: {enrollment.CourseId}");
            Console.WriteLine($"EnrollmentDate: {enrollment.EnrollmentDate}");
            Console.WriteLine($"Status: {enrollment.Status}");

            if (ModelState.IsValid)
            {
                Console.WriteLine("Model is valid, checking enrollment...");

                // Check if student is already enrolled
                if (await _enrollmentService.IsStudentEnrolledAsync(enrollment.StudentId, enrollment.CourseId))
                {
                    ModelState.AddModelError("", "Student is already enrolled in this course.");
                    await PopulateViewData();
                    return View(enrollment);
                }

                var result = await _enrollmentService.EnrollStudentAsync(enrollment);
                if (result)
                {
                    TempData["SuccessMessage"] = "Student enrolled successfully!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Error enrolling student. Please try again.");
            }
            else
            {
                Console.WriteLine("Model is INVALID:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($" - {error.ErrorMessage}");
                }
            }

            await PopulateViewData();
            return View(enrollment);
        }

        // GET: Enrollments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            Console.WriteLine($"=== EDIT GET - ID: {id} ===");

            var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(id);
            if (enrollment == null)
            {
                Console.WriteLine("Enrollment not found!");
                return NotFound();
            }

            Console.WriteLine($"Found enrollment: Student={enrollment.StudentId}, Course={enrollment.CourseId}, Status={enrollment.Status}");

            // We don't need to populate dropdowns for edit since we're using hidden fields
            // and showing the student/course as read-only text
            return View(enrollment);
        }

        // POST: Enrollments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Enrollment enrollment)
        {
            Console.WriteLine($"=== EDIT POST ===");
            Console.WriteLine($"ID from route: {id}, ID from model: {enrollment.Id}");
            Console.WriteLine($"StudentId: {enrollment.StudentId}, CourseId: {enrollment.CourseId}, Status: {enrollment.Status}");

            if (id != enrollment.Id)
            {
                Console.WriteLine("ID mismatch!");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                Console.WriteLine("Model is valid, updating enrollment...");

                var result = await _enrollmentService.UpdateEnrollmentAsync(enrollment);
                if (result)
                {
                    Console.WriteLine("Update successful!");
                    TempData["SuccessMessage"] = "Enrollment updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                Console.WriteLine("Update failed!");
                ModelState.AddModelError("", "Error updating enrollment. Please try again.");
            }
            else
            {
                Console.WriteLine("Model is INVALID:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($" - {error.ErrorMessage}");
                }
            }

            // Reload the enrollment with student/course data for the view
            var existingEnrollment = await _enrollmentService.GetEnrollmentByIdAsync(id);
            if (existingEnrollment != null)
            {
                enrollment.Student = existingEnrollment.Student;
                enrollment.Course = existingEnrollment.Course;
            }

            return View(enrollment);
        }

        // POST: Enrollments/Drop/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Drop(int id)
        {
            var result = await _enrollmentService.DropEnrollmentAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Student dropped from course successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Error dropping student from course. Please try again.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Enrollments/ByStudent/5
        public async Task<IActionResult> ByStudent(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            var enrollments = await _enrollmentService.GetEnrollmentsByStudentIdAsync(id);
            ViewBag.Student = student;
            return View(enrollments);
        }

        // GET: Enrollments/ByCourse/5
        public async Task<IActionResult> ByCourse(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var enrollments = await _enrollmentService.GetEnrollmentsByCourseIdAsync(id);
            ViewBag.Course = course;
            return View(enrollments);
        }

        private async Task PopulateViewData()
        {
            var students = await _studentService.GetAllStudentsAsync();
            var courses = await _courseService.GetActiveCoursesAsync();

            // Only show active students and courses
            var activeStudents = students.Where(s => s.IsActive).ToList();
            var activeCourses = courses.Where(c => c.IsActive).ToList();

            ViewBag.StudentId = new SelectList(activeStudents, "Id", "FullName");
            ViewBag.CourseId = new SelectList(activeCourses, "Id", "Name");
        }

        // You can remove these test methods if you want since the main Create works now
        public async Task<IActionResult> CheckData()
        {
            var students = await _studentService.GetAllStudentsAsync();
            var courses = await _courseService.GetAllCoursesAsync();

            ViewBag.StudentCount = students.Count;
            ViewBag.CourseCount = courses.Count;
            ViewBag.Students = students;
            ViewBag.Courses = courses;

            return View();
        }

        // GET: Enrollments/TestCreate
        public IActionResult TestCreate()
        {
            return View();
        }

        // POST: Enrollments/TestCreate  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> testCreate(int studentId, int courseId, EnrollmentStatus status)
        {
            try
            {
                Console.WriteLine("=== TEST CREATE ===");
                Console.WriteLine($"StudentId: {studentId}, CourseId: {courseId}, Status: {status}");

                var enrollment = new Enrollment
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    Status = status,
                    EnrollmentDate = DateTime.Now
                };

                var result = await _enrollmentService.EnrollStudentAsync(enrollment);

                if (result)
                {
                    TempData["SuccessMessage"] = "Test enrollment created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to create test enrollment";
                    return View();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TEST CREATE EXCEPTION: {ex}");
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return View();
            }
        }
    }
}