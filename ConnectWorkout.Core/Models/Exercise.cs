using System.Collections.Generic;

namespace ConnectWorkout.Core.Models
{
    /// <summary>
    /// Representa um exercício dentro de um dia de treino
    /// </summary>
    public class Exercise
    {
        /// <summary>
        /// Identificador único do exercício
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// ID do dia de treino ao qual este exercício pertence
        /// </summary>
        public int WorkoutDayId { get; set; }
        
        /// <summary>
        /// ID do exercício na API ExerciseDB externa
        /// </summary>
        public string ExerciseDbId { get; set; }
        
        /// <summary>
        /// Número de séries a serem realizadas (ex: "3" ou "3-4")
        /// </summary>
        public string Sets { get; set; }
        
        /// <summary>
        /// Número de repetições por série (ex: "12" ou "8-12")
        /// </summary>
        public string Repetitions { get; set; }
        
        /// <summary>
        /// Instruções adicionais do instrutor para este exercício
        /// </summary>
        public string Notes { get; set; }
        
        // Propriedades de navegação
        
        /// <summary>
        /// Referência ao dia de treino
        /// </summary>
        public virtual WorkoutDay WorkoutDay { get; set; }
        
        /// <summary>
        /// Status de conclusão deste exercício em diferentes datas
        /// </summary>
        public virtual ICollection<ExerciseStatus> Statuses { get; set; }
    }
}