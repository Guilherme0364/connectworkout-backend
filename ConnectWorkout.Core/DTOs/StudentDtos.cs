using System.Collections.Generic;

namespace ConnectWorkout.Core.DTOs
{
    /// <summary>
    /// DTO para exibir informações resumidas de um instrutor
    /// </summary>
    public class InstructorSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public int StudentCount { get; set; }
    }

    /// <summary>
    /// DTO para exibir informações do dashboard do aluno
    /// </summary>
    public class StudentDashboardDto
    {
        public string StudentName { get; set; }
        public bool HasTrainer { get; set; }
        public InstructorSummaryDto CurrentTrainer { get; set; }
        public int WorkoutCount { get; set; }
        public int? ActiveWorkoutId { get; set; }
        public int PendingRequestsCount { get; set; }
    }
}