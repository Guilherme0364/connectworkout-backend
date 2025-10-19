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
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(
            IUserService userService,
            IWorkoutRepository workoutRepository,
            ILogger<StudentsController> logger)
        {
            _userService = userService;
            _workoutRepository = workoutRepository;
            _logger = logger;
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
    }
}
