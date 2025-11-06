using System;
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

    /// <summary>
    /// DTO para exibir convites pendentes de instrutores
    /// </summary>
    public class PendingInvitationDto
    {
        public int InvitationId { get; set; }
        public int InstructorId { get; set; }
        public string InstructorName { get; set; }
        public string InstructorEmail { get; set; }
        public string InstructorDescription { get; set; }
        public int InstructorStudentCount { get; set; }
        public DateTime InvitedAt { get; set; }
    }
}