using System.Collections.Generic;

namespace ConnectWorkout.Core.DTOs
{
    /// <summary>
    /// DTO para conectar um instrutor a um aluno
    /// </summary>
    public class ConnectStudentDto
    {
        public int StudentId { get; set; }
    }
    
    /// <summary>
    /// DTO para exibir informações resumidas de um aluno
    /// </summary>
    public class StudentSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int? Age { get; set; }
        public int ActiveWorkoutId { get; set; }
        public string ActiveWorkoutName { get; set; }
        public int CompletedExercisesToday { get; set; }
        public int TotalExercisesToday { get; set; }
    }
}