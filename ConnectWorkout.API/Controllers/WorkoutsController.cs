using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ConnectWorkout.Core.DTOs;
using ConnectWorkout.Core.Enums;
using ConnectWorkout.Core.Interfaces;
using ConnectWorkout.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ConnectWorkout.API.Controllers
{
    /// <summary>
    /// Controller for managing workout plans (fichas de treino)
    /// Handles workout and exercise CRUD operations for coaches
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkoutsController : ControllerBase
    {
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IWorkoutDayRepository _workoutDayRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IUserRepository _userRepository;
        private readonly IExerciseDbService _exerciseDbService;
        private readonly ILogger<WorkoutsController> _logger;

        public WorkoutsController(
            IWorkoutRepository workoutRepository,
            IWorkoutDayRepository workoutDayRepository,
            IExerciseRepository exerciseRepository,
            IUserRepository userRepository,
            IExerciseDbService exerciseDbService,
            ILogger<WorkoutsController> logger)
        {
            _workoutRepository = workoutRepository;
            _workoutDayRepository = workoutDayRepository;
            _exerciseRepository = exerciseRepository;
            _userRepository = userRepository;
            _exerciseDbService = exerciseDbService;
            _logger = logger;
        }

        // ========================================
        // WORKOUT MANAGEMENT
        // ========================================

        /// <summary>
        /// Get all workouts for a specific student
        /// </summary>
        /// <param name="studentId">ID of the student</param>
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentWorkouts(int studentId)
        {
            try
            {
                _logger.LogInformation("Getting workouts for student: {StudentId}", studentId);

                // Verify student exists
                var student = await _userRepository.GetByIdAsync(studentId);
                if (student == null || student.UserType != UserType.Student)
                {
                    return NotFound(new { message = "Student not found" });
                }

                var workouts = await _workoutRepository.GetWorkoutsByStudentIdAsync(studentId);

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
                _logger.LogError(ex, "Error getting workouts for student {StudentId}", studentId);
                return StatusCode(500, new { message = "Error retrieving workouts" });
            }
        }

        /// <summary>
        /// Get complete workout details with all days and exercises
        /// </summary>
        /// <param name="workoutId">ID of the workout</param>
        [HttpGet("{workoutId}")]
        public async Task<IActionResult> GetWorkoutDetails(int workoutId)
        {
            try
            {
                _logger.LogInformation("Getting details for workout: {WorkoutId}", workoutId);

                var workout = await _workoutRepository.GetWorkoutWithDaysAndExercisesAsync(workoutId);
                if (workout == null)
                {
                    return NotFound(new { message = "Workout not found" });
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
                _logger.LogError(ex, "Error getting workout details for {WorkoutId}", workoutId);
                return StatusCode(500, new { message = "Error retrieving workout details" });
            }
        }

        /// <summary>
        /// Create a new workout plan for a student
        /// </summary>
        /// <param name="dto">Workout creation data</param>
        [HttpPost]
        public async Task<IActionResult> CreateWorkout([FromBody] CreateWorkoutDto dto)
        {
            try
            {
                _logger.LogInformation("Creating workout for student: {StudentId}", dto.StudentId);

                // Verify student exists
                var student = await _userRepository.GetByIdAsync(dto.StudentId);
                if (student == null || student.UserType != UserType.Student)
                {
                    return BadRequest(new { message = "Invalid student ID" });
                }

                var workout = new Workout
                {
                    StudentId = dto.StudentId,
                    Name = dto.Name,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _workoutRepository.AddAsync(workout);

                return CreatedAtAction(nameof(GetWorkoutDetails), new { workoutId = workout.Id }, new { id = workout.Id, name = workout.Name });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating workout");
                return StatusCode(500, new { message = "Error creating workout" });
            }
        }

        /// <summary>
        /// Update workout details
        /// </summary>
        /// <param name="workoutId">ID of the workout</param>
        /// <param name="dto">Update data</param>
        [HttpPut("{workoutId}")]
        public async Task<IActionResult> UpdateWorkout(int workoutId, [FromBody] UpdateWorkoutDto dto)
        {
            try
            {
                _logger.LogInformation("Updating workout: {WorkoutId}", workoutId);

                var workout = await _workoutRepository.GetByIdAsync(workoutId);
                if (workout == null)
                {
                    return NotFound(new { message = "Workout not found" });
                }

                if (!string.IsNullOrEmpty(dto.Name))
                {
                    workout.Name = dto.Name;
                }

                if (dto.IsActive.HasValue)
                {
                    workout.IsActive = dto.IsActive.Value;
                }

                await _workoutRepository.UpdateAsync(workout);

                return Ok(new { message = "Workout updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workout {WorkoutId}", workoutId);
                return StatusCode(500, new { message = "Error updating workout" });
            }
        }

        /// <summary>
        /// Delete a workout plan
        /// </summary>
        /// <param name="workoutId">ID of the workout</param>
        [HttpDelete("{workoutId}")]
        public async Task<IActionResult> DeleteWorkout(int workoutId)
        {
            try
            {
                _logger.LogInformation("Deleting workout: {WorkoutId}", workoutId);

                var workout = await _workoutRepository.GetByIdAsync(workoutId);
                if (workout == null)
                {
                    return NotFound(new { message = "Workout not found" });
                }

                await _workoutRepository.DeleteAsync(workout);

                return Ok(new { message = "Workout deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting workout {WorkoutId}", workoutId);
                return StatusCode(500, new { message = "Error deleting workout" });
            }
        }

        // ========================================
        // WORKOUT DAY MANAGEMENT
        // ========================================

        /// <summary>
        /// Add a day to a workout plan
        /// </summary>
        /// <param name="workoutId">ID of the workout</param>
        /// <param name="dto">Workout day data</param>
        [HttpPost("{workoutId}/days")]
        public async Task<IActionResult> AddWorkoutDay(int workoutId, [FromBody] AddWorkoutDayDto dto)
        {
            try
            {
                _logger.LogInformation("Adding day to workout: {WorkoutId}", workoutId);

                var workout = await _workoutRepository.GetByIdAsync(workoutId);
                if (workout == null)
                {
                    return NotFound(new { message = "Workout not found" });
                }

                // Check if day already exists
                var existingDay = await _workoutDayRepository.GetWorkoutDayByDayOfWeekAsync(workoutId, dto.DayOfWeek);
                if (existingDay != null)
                {
                    return BadRequest(new { message = "This day already exists in the workout" });
                }

                var workoutDay = new WorkoutDay
                {
                    WorkoutId = workoutId,
                    DayOfWeek = dto.DayOfWeek
                };

                await _workoutDayRepository.AddAsync(workoutDay);

                return CreatedAtAction(nameof(GetWorkoutDetails), new { workoutId }, new { id = workoutDay.Id, dayOfWeek = workoutDay.DayOfWeek });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding day to workout {WorkoutId}", workoutId);
                return StatusCode(500, new { message = "Error adding workout day" });
            }
        }

        /// <summary>
        /// Delete a workout day
        /// </summary>
        /// <param name="workoutId">ID of the workout</param>
        /// <param name="dayId">ID of the day</param>
        [HttpDelete("{workoutId}/days/{dayId}")]
        public async Task<IActionResult> DeleteWorkoutDay(int workoutId, int dayId)
        {
            try
            {
                _logger.LogInformation("Deleting day {DayId} from workout {WorkoutId}", dayId, workoutId);

                var workoutDay = await _workoutDayRepository.GetByIdAsync(dayId);
                if (workoutDay == null || workoutDay.WorkoutId != workoutId)
                {
                    return NotFound(new { message = "Workout day not found" });
                }

                await _workoutDayRepository.DeleteAsync(workoutDay);

                return Ok(new { message = "Workout day deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting workout day {DayId}", dayId);
                return StatusCode(500, new { message = "Error deleting workout day" });
            }
        }

        // ========================================
        // EXERCISE MANAGEMENT
        // ========================================

        /// <summary>
        /// Add an exercise to a workout day
        /// </summary>
        /// <param name="workoutId">ID of the workout</param>
        /// <param name="dayId">ID of the workout day</param>
        /// <param name="dto">Exercise data</param>
        [HttpPost("{workoutId}/days/{dayId}/exercises")]
        public async Task<IActionResult> AddExercise(int workoutId, int dayId, [FromBody] AddExerciseDto dto)
        {
            try
            {
                _logger.LogInformation("Adding exercise to day {DayId} of workout {WorkoutId}", dayId, workoutId);

                var workoutDay = await _workoutDayRepository.GetWorkoutDayWithExercisesAsync(dayId);
                if (workoutDay == null || workoutDay.WorkoutId != workoutId)
                {
                    return NotFound(new { message = "Workout day not found" });
                }

                // Get next order number
                int nextOrder = workoutDay.Exercises?.Any() == true
                    ? workoutDay.Exercises.Max(e => e.Order) + 1
                    : 0;

                var exercise = new Exercise
                {
                    WorkoutDayId = dayId,
                    ExerciseDbId = dto.ExerciseDbId,
                    Name = dto.Name,
                    BodyPart = dto.BodyPart,
                    Equipment = dto.Equipment,
                    GifUrl = dto.GifUrl,
                    Sets = dto.Sets,
                    Repetitions = dto.Repetitions,
                    Weight = dto.Weight,
                    RestSeconds = dto.RestSeconds,
                    Order = nextOrder,
                    Notes = dto.Notes ?? string.Empty
                };

                await _exerciseRepository.AddAsync(exercise);

                return CreatedAtAction(nameof(GetWorkoutDetails), new { workoutId }, new { id = exercise.Id, name = exercise.Name });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding exercise to workout day {DayId}", dayId);
                return StatusCode(500, new { message = "Error adding exercise" });
            }
        }

        /// <summary>
        /// Update exercise configuration
        /// </summary>
        /// <param name="workoutId">ID of the workout</param>
        /// <param name="dayId">ID of the workout day</param>
        /// <param name="exerciseId">ID of the exercise</param>
        /// <param name="dto">Update data</param>
        [HttpPut("{workoutId}/days/{dayId}/exercises/{exerciseId}")]
        public async Task<IActionResult> UpdateExercise(int workoutId, int dayId, int exerciseId, [FromBody] UpdateExerciseDto dto)
        {
            try
            {
                _logger.LogInformation("Updating exercise {ExerciseId}", exerciseId);

                var exercise = await _exerciseRepository.GetByIdAsync(exerciseId);
                if (exercise == null || exercise.WorkoutDayId != dayId)
                {
                    return NotFound(new { message = "Exercise not found" });
                }

                // Update fields if provided
                if (!string.IsNullOrEmpty(dto.Sets))
                    exercise.Sets = dto.Sets;

                if (!string.IsNullOrEmpty(dto.Repetitions))
                    exercise.Repetitions = dto.Repetitions;

                if (dto.Weight.HasValue)
                    exercise.Weight = dto.Weight;

                if (dto.RestSeconds.HasValue)
                    exercise.RestSeconds = dto.RestSeconds;

                if (dto.Notes != null)
                    exercise.Notes = dto.Notes ?? string.Empty;

                await _exerciseRepository.UpdateAsync(exercise);

                return Ok(new { message = "Exercise updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating exercise {ExerciseId}", exerciseId);
                return StatusCode(500, new { message = "Error updating exercise" });
            }
        }

        /// <summary>
        /// Delete an exercise from a workout day
        /// </summary>
        /// <param name="workoutId">ID of the workout</param>
        /// <param name="dayId">ID of the workout day</param>
        /// <param name="exerciseId">ID of the exercise</param>
        [HttpDelete("{workoutId}/days/{dayId}/exercises/{exerciseId}")]
        public async Task<IActionResult> DeleteExercise(int workoutId, int dayId, int exerciseId)
        {
            try
            {
                _logger.LogInformation("Deleting exercise {ExerciseId}", exerciseId);

                var exercise = await _exerciseRepository.GetByIdAsync(exerciseId);
                if (exercise == null || exercise.WorkoutDayId != dayId)
                {
                    return NotFound(new { message = "Exercise not found" });
                }

                await _exerciseRepository.DeleteAsync(exercise);

                return Ok(new { message = "Exercise deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting exercise {ExerciseId}", exerciseId);
                return StatusCode(500, new { message = "Error deleting exercise" });
            }
        }

        /// <summary>
        /// Reorder exercises within a workout day
        /// </summary>
        /// <param name="workoutId">ID of the workout</param>
        /// <param name="dayId">ID of the workout day</param>
        /// <param name="dto">List of exercise IDs in desired order</param>
        [HttpPut("{workoutId}/days/{dayId}/exercises/reorder")]
        public async Task<IActionResult> ReorderExercises(int workoutId, int dayId, [FromBody] ReorderExercisesDto dto)
        {
            try
            {
                _logger.LogInformation("Reordering exercises for day {DayId}", dayId);

                var workoutDay = await _workoutDayRepository.GetByIdAsync(dayId);
                if (workoutDay == null || workoutDay.WorkoutId != workoutId)
                {
                    return NotFound(new { message = "Workout day not found" });
                }

                await _exerciseRepository.ReorderExercisesAsync(dto.ExerciseIds);

                return Ok(new { message = "Exercises reordered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering exercises for day {DayId}", dayId);
                return StatusCode(500, new { message = "Error reordering exercises" });
            }
        }
    }
}
