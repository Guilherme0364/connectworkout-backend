using System;
using System.Collections.Generic;

namespace ConnectWorkout.Core.Models
{
    /// <summary>
    /// Representa uma ficha de treino criada para um aluno
    /// </summary>
    public class Workout
    {
        /// <summary>
        /// Identificador único da ficha de treino
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// ID do aluno dono da ficha
        /// </summary>
        public int StudentId { get; set; }
        
        /// <summary>
        /// Nome da ficha de treino (ex: "Hipertrofia Fase 1")
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Data de criação da ficha
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Indica se esta é a ficha ativa do aluno
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        // Propriedades de navegação
        
        /// <summary>
        /// Referência ao aluno dono da ficha
        /// </summary>
        public virtual User Student { get; set; }
        
        /// <summary>
        /// Dias de treino que compõem esta ficha
        /// </summary>
        public virtual ICollection<WorkoutDay> WorkoutDays { get; set; }
    }
}