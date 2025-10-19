using System;
using System.Collections.Generic;
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
        public string ExerciseDbId { get; set; }
        public string Name { get; set; }
        public string BodyPart { get; set; }
        public string Equipment { get; set; }
        public string GifUrl { get; set; }
        public string Sets { get; set; }
        public string Repetitions { get; set; }
        public decimal? Weight { get; set; }
        public int? RestSeconds { get; set; }
        public string Notes { get; set; }
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