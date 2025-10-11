using System;
using System.Collections.Generic;

namespace ConnectWorkout.Core.Models
{
    /// <summary>
    /// Representa um dia de treino dentro de uma ficha
    /// </summary>
    public class WorkoutDay
    {
        /// <summary>
        /// Identificador único do dia de treino
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// ID da ficha de treino à qual este dia pertence
        /// </summary>
        public int WorkoutId { get; set; }
        
        /// <summary>
        /// Dia da semana para este treino
        /// </summary>
        public DayOfWeek DayOfWeek { get; set; }
        
        // Propriedades de navegação
        
        /// <summary>
        /// Referência à ficha de treino
        /// </summary>
        public virtual Workout Workout { get; set; }
        
        /// <summary>
        /// Exercícios que compõem este dia de treino
        /// </summary>
        public virtual ICollection<Exercise> Exercises { get; set; }
    }
}