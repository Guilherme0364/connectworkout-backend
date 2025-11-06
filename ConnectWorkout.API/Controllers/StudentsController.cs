using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ConnectWorkout.Core.DTOs;
using ConnectWorkout.Core.Enums;
using ConnectWorkout.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ConnectWorkout.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication
    public class StudentsController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IStudentInstructorRepository _studentInstructorRepository;
        private readonly IStudentService _studentService;
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(
            IUserService userService,
            IWorkoutRepository workoutRepository,
            IStudentInstructorRepository studentInstructorRepository,
            IStudentService studentService,
            ILogger<StudentsController> logger)
        {
            _userService = userService;
            _workoutRepository = workoutRepository;
            _studentInstructorRepository = studentInstructorRepository;
            _studentService = studentService;
            _logger = logger;
        }

        /// <summary>
        /// Get student dashboard data with aggregated information
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                // Get user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var userId = int.Parse(userIdClaim.Value);

                _logger.LogInformation("Getting dashboard data for student: {UserId}", userId);

                // Get student info
                var student = await _userService.GetUserByIdAsync(userId);
                if (student == null || student.UserType != UserType.Student)
                {
                    return Forbid();
                }

                // Get most recent instructor (ordered by ConnectedAt DESC)
                // Only get accepted connections, not pending invitations
                var instructorRelations = await _studentInstructorRepository.FindAsync(si => si.StudentId == userId && si.Status == InvitationStatus.Accepted);
                var mostRecentRelation = instructorRelations.OrderByDescending(si => si.ConnectedAt).FirstOrDefault();

                InstructorSummaryDto currentTrainer = null;
                bool hasTrainer = false;

                if (mostRecentRelation != null)
                {
                    hasTrainer = true;
                    var instructor = await _userService.GetUserByIdAsync(mostRecentRelation.InstructorId);

                    if (instructor != null)
                    {
                        // Count total students for this instructor
                        var instructorStudents = await _studentInstructorRepository.FindAsync(si => si.InstructorId == instructor.Id);
                        var studentCount = instructorStudents.Count();

                        currentTrainer = new InstructorSummaryDto
                        {
                            Id = instructor.Id,
                            Name = instructor.Name,
                            Email = instructor.Email,
                            Description = instructor.Description,
                            StudentCount = studentCount
                        };
                    }
                }

                // Get workout count and active workout
                var workouts = await _workoutRepository.GetWorkoutsByStudentIdAsync(userId);
                var workoutCount = workouts.Count();

                // Get most recent active workout (ordered by CreatedAt DESC)
                var activeWorkout = workouts
                    .Where(w => w.IsActive)
                    .OrderByDescending(w => w.CreatedAt)
                    .FirstOrDefault();

                var activeWorkoutId = activeWorkout?.Id;

                // Get pending invitations count
                var pendingInvitationsCount = await _studentService.GetPendingInvitationsCountAsync(userId);

                // Build dashboard DTO
                var dashboard = new StudentDashboardDto
                {
                    StudentName = student.Name,
                    HasTrainer = hasTrainer,
                    CurrentTrainer = currentTrainer,
                    WorkoutCount = workoutCount,
                    ActiveWorkoutId = activeWorkoutId,
                    PendingRequestsCount = pendingInvitationsCount
                };

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student dashboard");
                return StatusCode(500, new { message = "Error retrieving dashboard data" });
            }
        }

        /// <summary>
        /// Get current authenticated student's profile
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                // Get user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var userId = int.Parse(userIdClaim.Value);

                _logger.LogInformation("Getting profile for student: {UserId}", userId);

                // Get student profile from service
                var profile = await _userService.GetStudentProfileAsync(userId);

                if (profile == null)
                {
                    return NotFound(new { message = "Student profile not found" });
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student profile");
                return StatusCode(500, new { message = "Error retrieving profile" });
            }
        }

        /// <summary>
        /// Get current student's assigned trainer
        /// </summary>
        [HttpGet("current-trainer")]
        public async Task<IActionResult> GetCurrentTrainer()
        {
            try
            {
                // Get user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var userId = int.Parse(userIdClaim.Value);

                _logger.LogInformation("Getting current trainer for student: {UserId}", userId);

                // Verify user is a student
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null || user.UserType != UserType.Student)
                {
                    return Forbid();
                }

                // Get trainer information from service
                var trainer = await _userService.GetStudentInstructorAsync(userId);

                if (trainer == null)
                {
                    return Ok(new
                    {
                        hasTrainer = false,
                        message = "No trainer assigned",
                        trainer = (object)null
                    });
                }

                return Ok(new
                {
                    hasTrainer = true,
                    trainer = trainer
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current trainer");
                return StatusCode(500, new { message = "Error retrieving trainer information" });
            }
        }

        /// <summary>
        /// Update student profile (Minha ficha)
        /// </summary>
        /// <param name="dto">Profile update data</param>
        /// <returns>Updated profile</returns>
        [HttpPut("profile")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateStudentProfileDto dto)
        {
            try
            {
                // Get authenticated user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var userId = int.Parse(userIdClaim.Value);

                _logger.LogInformation("Updating profile for student: {UserId}", userId);

                // Validate model
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new
                    {
                        message = "Validation failed",
                        errors = errors
                    });
                }

                // Update profile through service
                var result = await _userService.UpdateStudentProfileAsync(userId, dto);

                if (result == null)
                {
                    return BadRequest(new { message = "Failed to update profile. User may not be a student." });
                }

                _logger.LogInformation("Profile updated successfully for student: {UserId}", userId);

                return Ok(new
                {
                    success = true,
                    message = "Perfil atualizado com sucesso",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student profile: {UserId}",
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, new
                {
                    message = "Erro ao atualizar perfil",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get student's assigned workout plans
        /// </summary>
        [HttpGet("workouts")]
        public async Task<IActionResult> GetMyWorkouts()
        {
            try
            {
                // Get user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var userId = int.Parse(userIdClaim.Value);

                _logger.LogInformation("Getting workouts for student: {UserId}", userId);

                // Verify user is a student
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null || user.UserType != UserType.Student)
                {
                    return Forbid();
                }

                var workouts = await _workoutRepository.GetWorkoutsByStudentIdAsync(userId);

                var workoutSummaries = workouts.Select(w => new WorkoutSummaryDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    CreatedAt = w.CreatedAt,
                    IsActive = w.IsActive,
                    DaysCount = w.WorkoutDays?.Count ?? 0,
                    ExercisesCount = w.WorkoutDays?.Sum(wd => wd.Exercises?.Count ?? 0) ?? 0
                });

                return Ok(workoutSummaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student workouts");
                return StatusCode(500, new { message = "Error retrieving workouts" });
            }
        }

        /// <summary>
        /// Get active workout for student
        /// </summary>
        [HttpGet("workouts/active")]
        public async Task<IActionResult> GetActiveWorkout()
        {
            try
            {
                // Get user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var userId = int.Parse(userIdClaim.Value);

                _logger.LogInformation("Getting active workout for student: {UserId}", userId);

                // Verify user is a student
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null || user.UserType != UserType.Student)
                {
                    return Forbid();
                }

                var workout = await _workoutRepository.GetActiveWorkoutForStudentAsync(userId);

                if (workout == null)
                {
                    return NotFound(new { message = "No active workout found" });
                }

                // Map to DTO
                var workoutDto = new WorkoutDetailDto
                {
                    Id = workout.Id,
                    Name = workout.Name,
                    CreatedAt = workout.CreatedAt,
                    IsActive = workout.IsActive,
                    WorkoutDays = workout.WorkoutDays?.Select(wd => new WorkoutDayDto
                    {
                        Id = wd.Id,
                        DayOfWeek = wd.DayOfWeek,
                        Exercises = wd.Exercises?.OrderBy(e => e.Order).Select(e => new ExerciseDetailDto
                        {
                            Id = e.Id,
                            ExerciseDbId = e.ExerciseDbId,
                            Name = e.Name,
                            BodyPart = e.BodyPart,
                            Equipment = e.Equipment,
                            GifUrl = e.GifUrl,
                            Sets = e.Sets,
                            Repetitions = e.Repetitions,
                            Weight = e.Weight,
                            RestSeconds = e.RestSeconds,
                            Order = e.Order,
                            Notes = e.Notes
                        }).ToList()
                    }).ToList()
                };

                return Ok(workoutDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active workout");
                return StatusCode(500, new { message = "Error retrieving active workout" });
            }
        }

        /// <summary>
        /// Get pending invitations for the authenticated student
        /// </summary>
        [HttpGet("invitations/pending")]
        public async Task<IActionResult> GetPendingInvitations()
        {
            try
            {
                // Get user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var userId = int.Parse(userIdClaim.Value);

                _logger.LogInformation("Getting pending invitations for student: {UserId}", userId);

                // Verify user is a student
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null || user.UserType != UserType.Student)
                {
                    return Forbid();
                }

                var pendingInvitations = await _studentService.GetPendingInvitationsAsync(userId);

                return Ok(pendingInvitations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending invitations");
                return StatusCode(500, new { message = "Error retrieving pending invitations" });
            }
        }

        /// <summary>
        /// Accept a pending invitation from an instructor
        /// </summary>
        [HttpPost("invitations/{invitationId}/accept")]
        public async Task<IActionResult> AcceptInvitation(int invitationId)
        {
            try
            {
                // Get user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var userId = int.Parse(userIdClaim.Value);

                _logger.LogInformation("Student {UserId} accepting invitation {InvitationId}", userId, invitationId);

                // Verify user is a student
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null || user.UserType != UserType.Student)
                {
                    return Forbid();
                }

                var result = await _studentService.AcceptInvitationAsync(userId, invitationId);

                if (!result)
                {
                    return BadRequest(new { message = "Failed to accept invitation. Invitation may not exist or already responded to." });
                }

                return Ok(new
                {
                    success = true,
                    message = "Invitation accepted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting invitation");
                return StatusCode(500, new { message = "Error accepting invitation" });
            }
        }

        /// <summary>
        /// Reject a pending invitation from an instructor
        /// </summary>
        [HttpPost("invitations/{invitationId}/reject")]
        public async Task<IActionResult> RejectInvitation(int invitationId)
        {
            try
            {
                // Get user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var userId = int.Parse(userIdClaim.Value);

                _logger.LogInformation("Student {UserId} rejecting invitation {InvitationId}", userId, invitationId);

                // Verify user is a student
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null || user.UserType != UserType.Student)
                {
                    return Forbid();
                }

                var result = await _studentService.RejectInvitationAsync(userId, invitationId);

                if (!result)
                {
                    return BadRequest(new { message = "Failed to reject invitation. Invitation may not exist or already responded to." });
                }

                return Ok(new
                {
                    success = true,
                    message = "Invitation rejected successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting invitation");
                return StatusCode(500, new { message = "Error rejecting invitation" });
            }
        }

        /// <summary>
        /// Delete the authenticated student's account and all associated data
        /// </summary>
        [HttpDelete("account")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteAccount()
        {
            try
            {
                // Get user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var userId = int.Parse(userIdClaim.Value);

                _logger.LogInformation("Student {UserId} requesting account deletion", userId);

                // Verify user is a student
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null || user.UserType != UserType.Student)
                {
                    return Forbid();
                }

                // Delete account
                await _userService.DeleteAccountAsync(userId);

                _logger.LogInformation("Account deleted successfully for student {UserId}", userId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting student account");
                return StatusCode(500, new { message = "Error deleting account" });
            }
        }

        /// <summary>
        /// Get instructor/trainer details by ID
        /// </summary>
        [HttpGet("trainers/{trainerId}")]
        public async Task<IActionResult> GetTrainerDetails(int trainerId)
        {
            try
            {
                // Get user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var userId = int.Parse(userIdClaim.Value);

                _logger.LogInformation("Student {UserId} requesting trainer details for trainer {TrainerId}", userId, trainerId);

                // Verify user is a student
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null || user.UserType != UserType.Student)
                {
                    return Forbid();
                }

                // Get instructor details
                var instructor = await _userService.GetUserByIdAsync(trainerId);
                if (instructor == null || instructor.UserType != UserType.Instructor)
                {
                    return NotFound(new { message = "Instructor not found" });
                }

                // Count instructor's students
                var instructorRelations = await _studentInstructorRepository.FindAsync(si =>
                    si.InstructorId == trainerId &&
                    si.Status == InvitationStatus.Accepted);
                var studentCount = instructorRelations.Count();

                // Build trainer DTO
                var trainerDto = new
                {
                    id = instructor.Id,
                    name = instructor.Name,
                    email = instructor.Email,
                    age = instructor.Age,
                    gender = instructor.Gender,
                    description = instructor.Description ?? "",
                    studentCount = studentCount,
                    credentials = instructor.Certifications,
                    photoUrl = (string)null
                };

                return Ok(trainerDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trainer details");
                return StatusCode(500, new { message = "Error retrieving trainer details" });
            }
        }
    }
}
