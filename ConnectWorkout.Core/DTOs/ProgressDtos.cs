using System;
using System.Collections.Generic;
using ConnectWorkout.Core.Enums;

namespace ConnectWorkout.Core.DTOs
{
    /// <summary>
    /// DTO para marcar um exercício como concluído ou pulado
    /// </summary>
    public class MarkExerciseDto
    {
        public int ExerciseId { get; set; }
        public StatusType Status { get; set; }
    }
    
    /// <summary>
    /// DTO para estatísticas diárias de treino
    /// </summary>
    public class DailyStatsDto
    {
        public DateTime Date { get; set; }
        public int CompletedExercises { get; set; }
        public int SkippedExercises { get; set; }
        public int TotalExercises { get; set; }
        public double CompletionRate { get; set; }
    }
    
    /// <summary>
    /// DTO para estatísticas semanais de treino
    /// </summary>
    public class WeeklyStatsDto
    {
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
        public List<DailyStatsDto> DailyStats { get; set; }
        public int TotalCompletedExercises { get; set; }
        public int TotalSkippedExercises { get; set; }
        public int TotalExercises { get; set; }
        public double WeeklyCompletionRate { get; set; }
    }
}