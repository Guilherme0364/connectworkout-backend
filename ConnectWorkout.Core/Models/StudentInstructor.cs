using System;

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
        /// Data em que a relação foi estabelecida
        /// </summary>
        public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
        
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