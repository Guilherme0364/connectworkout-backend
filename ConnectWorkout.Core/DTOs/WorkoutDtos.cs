using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ConnectWorkout.Core.Enums;

namespace ConnectWorkout.Core.DTOs
{
    /// <summary>
    /// DTO para criar uma nova ficha de treino
    /// </summary>
    public class CreateWorkoutDto
    {
        public string Name { get; set; }
        public int StudentId { get; set; }
    }

    /// <summary>
    /// DTO para criar uma ficha de treino completa com dias e exercícios
    /// </summary>
    public class CreateWorkoutBulkDto
    {
        public string Name { get; set; }
        public int StudentId { get; set; }
        public List<CreateWorkoutDayBulkDto> WorkoutDays { get; set; }
    }

    /// <summary>
    /// DTO para criar um dia de treino com exercícios na criação em massa
    /// </summary>
    public class CreateWorkoutDayBulkDto
    {
        public DayOfWeek DayOfWeek { get; set; }
        public List<AddExerciseDto> Exercises { get; set; }
    }
    
    /// <summary>
    /// DTO para atualizar uma ficha de treino
    /// </summary>
    public class UpdateWorkoutDto
    {
        public string Name { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// DTO para adicionar um dia de treino
    /// </summary>
    public class AddWorkoutDayDto
    {
        public DayOfWeek DayOfWeek { get; set; }
    }
    
    /// <summary>
    /// DTO para adicionar um exercício
    /// </summary>
    public class AddExerciseDto
    {
        [Required(ErrorMessage = "ExerciseDbId is required")]
        public string ExerciseDbId { get; set; }

        [Required(ErrorMessage = "Exercise name is required")]
        [StringLength(200, ErrorMessage = "Exercise name cannot exceed 200 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "BodyPart is required (e.g., 'chest', 'back', 'legs')")]
        [StringLength(100, ErrorMessage = "BodyPart cannot exceed 100 characters")]
        public string BodyPart { get; set; }

        [Required(ErrorMessage = "Equipment is required (e.g., 'barbell', 'dumbbell', 'body weight')")]
        [StringLength(100, ErrorMessage = "Equipment cannot exceed 100 characters")]
        public string Equipment { get; set; }

        [Required(ErrorMessage = "GifUrl is required")]
        [Url(ErrorMessage = "GifUrl must be a valid URL")]
        public string GifUrl { get; set; }

        [Required(ErrorMessage = "Sets is required (e.g., '3', '4')")]
        [StringLength(20, ErrorMessage = "Sets cannot exceed 20 characters")]
        public string Sets { get; set; }

        [Required(ErrorMessage = "Repetitions is required (e.g., '10', '12', '8-12')")]
        [StringLength(20, ErrorMessage = "Repetitions cannot exceed 20 characters")]
        public string Repetitions { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Weight must be a positive number")]
        public decimal? Weight { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "RestSeconds must be a positive number")]
        public int? RestSeconds { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }
    
    /// <summary>
    /// DTO para atualizar um exercício
    /// </summary>
    public class UpdateExerciseDto
    {
        public string Sets { get; set; }
        public string Repetitions { get; set; }
        public decimal? Weight { get; set; }
        public int? RestSeconds { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// DTO para reordenar exercícios
    /// </summary>
    public class ReorderExercisesDto
    {
        public List<int> ExerciseIds { get; set; }
    }

    /// <summary>
    /// DTO para exibir um exercício com detalhes da API ExerciseDB
    /// </summary>
    public class ExerciseDetailDto
    {
        public int Id { get; set; }
        public string ExerciseDbId { get; set; }
        public string Name { get; set; }
        public string BodyPart { get; set; }
        public string Target { get; set; }
        public string Equipment { get; set; }
        public string GifUrl { get; set; }
        public string Sets { get; set; }
        public string Repetitions { get; set; }
        public decimal? Weight { get; set; }
        public int? RestSeconds { get; set; }
        public int Order { get; set; }
        public string Notes { get; set; }
        public StatusType? Status { get; set; }
    }
    
    /// <summary>
    /// DTO para exibir um dia de treino com seus exercícios
    /// </summary>
    public class WorkoutDayDto
    {
        public int Id { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public List<ExerciseDetailDto> Exercises { get; set; }
    }
    
    /// <summary>
    /// DTO para exibir uma ficha de treino completa
    /// </summary>
    public class WorkoutDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public List<WorkoutDayDto> WorkoutDays { get; set; }
    }
    
    /// <summary>
    /// DTO para exibir informações resumidas de uma ficha de treino
    /// </summary>
    public class WorkoutSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public int DaysCount { get; set; }
        public int ExercisesCount { get; set; }
    }
}