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
        /// Nome do exercício (armazenado do ExerciseDB)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Grupo muscular trabalhado (ex: "chest", "back")
        /// </summary>
        public string BodyPart { get; set; }

        /// <summary>
        /// Equipamento necessário (ex: "barbell", "dumbbell")
        /// </summary>
        public string Equipment { get; set; }

        /// <summary>
        /// URL do GIF demonstrativo do ExerciseDB
        /// </summary>
        public string GifUrl { get; set; }

        /// <summary>
        /// Número de séries a serem realizadas (ex: "3" ou "3-4")
        /// </summary>
        public string Sets { get; set; }

        /// <summary>
        /// Número de repetições por série (ex: "12" ou "8-12" ou "até falha")
        /// </summary>
        public string Repetitions { get; set; }

        /// <summary>
        /// Peso a ser utilizado em kg (opcional)
        /// </summary>
        public decimal? Weight { get; set; }

        /// <summary>
        /// Tempo de descanso entre séries em segundos (opcional)
        /// </summary>
        public int? RestSeconds { get; set; }

        /// <summary>
        /// Ordem do exercício dentro do treino do dia
        /// </summary>
        public int Order { get; set; }

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