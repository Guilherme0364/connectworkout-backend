using System;
using ConnectWorkout.Core.Enums;

namespace ConnectWorkout.Core.Models
{
    /// <summary>
    /// Representa a relação entre um aluno e um instrutor
    /// </summary>
    public class StudentInstructor
    {
        /// <summary>
        /// Identificador único da relação
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// ID do aluno
        /// </summary>
        public int StudentId { get; set; }
        
        /// <summary>
        /// ID do instrutor
        /// </summary>
        public int InstructorId { get; set; }

        /// <summary>
        /// Status do convite (Pendente, Aceito, Rejeitado)
        /// </summary>
        public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

        /// <summary>
        /// Data em que o convite foi enviado
        /// </summary>
        public DateTime InvitedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data em que o aluno respondeu ao convite (aceitou ou rejeitou)
        /// </summary>
        public DateTime? RespondedAt { get; set; }

        /// <summary>
        /// Data em que a relação foi estabelecida (quando aceita)
        /// </summary>
        public DateTime? ConnectedAt { get; set; }
        
        // Propriedades de navegação
        
        /// <summary>
        /// Referência ao aluno
        /// </summary>
        public virtual User Student { get; set; }
        
        /// <summary>
        /// Referência ao instrutor
        /// </summary>
        public virtual User Instructor { get; set; }
    }
}