using System;
using ConnectWorkout.Core.Enums;

namespace ConnectWorkout.Core.Models
{
    /// <summary>
    /// Representa o status de conclusão de um exercício em uma data específica
    /// </summary>
    public class ExerciseStatus
    {
        /// <summary>
        /// Identificador único do status
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// ID do exercício
        /// </summary>
        public int ExerciseId { get; set; }
        
        /// <summary>
        /// ID do aluno que realizou (ou pulou) o exercício
        /// </summary>
        public int StudentId { get; set; }
        
        /// <summary>
        /// Data em que o exercício foi realizado ou pulado
        /// </summary>
        public DateTime Date { get; set; } = DateTime.UtcNow.Date;
        
        /// <summary>
        /// Status: Concluído ou Pulado
        /// </summary>
        public StatusType Status { get; set; }
        
        // Propriedades de navegação
        
        /// <summary>
        /// Referência ao exercício
        /// </summary>
        public virtual Exercise Exercise { get; set; }
        
        /// <summary>
        /// Referência ao aluno
        /// </summary>
        public virtual User Student { get; set; }
    }
}