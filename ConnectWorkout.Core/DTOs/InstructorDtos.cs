using System;
using System.Collections.Generic;

namespace ConnectWorkout.Core.DTOs
{
    /// <summary>
    /// DTO para conectar um instrutor a um aluno
    /// </summary>
    public class ConnectStudentDto
    {
        /// <summary>
        /// ID do aluno (opcional se Email for fornecido)
        /// </summary>
        public int? StudentId { get; set; }

        /// <summary>
        /// Email do aluno (opcional se StudentId for fornecido)
        /// Ao menos um dos dois (StudentId ou Email) deve ser fornecido
        /// </summary>
        public string Email { get; set; }
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
        public DateTime EnrolledAt { get; set; }
    }

    /// <summary>
    /// DTO para tendências estatísticas
    /// </summary>
    public class StatisticTrendDto
    {
        public double Value { get; set; }
        public bool IsPositive { get; set; }
    }

    /// <summary>
    /// DTO para estatísticas do instrutor
    /// </summary>
    public class InstructorStatisticsDto
    {
        // Workout statistics
        public int WorkoutsCreatedThisMonth { get; set; }
        public int WorkoutsCreatedThisWeek { get; set; }
        public int WorkoutsCreatedTotal { get; set; }

        // Student statistics
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int NewStudentsThisWeek { get; set; }
        public int NewStudentsThisMonth { get; set; }

        // Completion statistics
        public double AverageCompletionRate { get; set; }
        public double CompletionRateToday { get; set; }
        public double CompletionRateThisWeek { get; set; }
        public double CompletionRateThisMonth { get; set; }

        // Engagement statistics
        public int TotalWorkoutsCompletedThisMonth { get; set; }
        public int TotalExercisesCompletedThisMonth { get; set; }

        // Trends
        public StatisticTrendDto StudentsTrend { get; set; }
        public StatisticTrendDto CompletionRateTrend { get; set; }
        public StatisticTrendDto WorkoutsCreatedTrend { get; set; }
    }

    /// <summary>
    /// DTO para informações de convite/conexão com aluno
    /// </summary>
    public class InvitationDto
    {
        /// <summary>
        /// ID do relacionamento
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Informações do aluno
        /// </summary>
        public StudentInfoDto Student { get; set; }

        /// <summary>
        /// Status do convite: 0 = Pending, 1 = Accepted, 2 = Rejected
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Nome do status (para facilitar no frontend)
        /// </summary>
        public string StatusName { get; set; }

        /// <summary>
        /// Data em que o convite foi enviado
        /// </summary>
        public DateTime InvitedAt { get; set; }

        /// <summary>
        /// Data em que o aluno respondeu (aceitou ou rejeitou)
        /// </summary>
        public DateTime? RespondedAt { get; set; }

        /// <summary>
        /// Data em que a conexão foi estabelecida
        /// </summary>
        public DateTime? ConnectedAt { get; set; }
    }

    /// <summary>
    /// DTO para informações básicas do aluno no convite
    /// </summary>
    public class StudentInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int? Age { get; set; }
        public int? Gender { get; set; }
    }
}