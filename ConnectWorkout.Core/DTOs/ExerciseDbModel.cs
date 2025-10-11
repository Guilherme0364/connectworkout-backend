using System.Collections.Generic;

namespace ConnectWorkout.Core.DTOs
{
    /// <summary>
    /// Representa um exercício retornado pela API ExerciseDB
    /// </summary>
    public class ExerciseDbModel
    {
        /// <summary>
        /// Identificador único na API ExerciseDB
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Nome do exercício
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Parte do corpo trabalhada (ex: "back", "chest", "legs")
        /// </summary>
        public string BodyPart { get; set; }
        
        /// <summary>
        /// Equipamento necessário (ex: "barbell", "dumbbell", "body weight")
        /// </summary>
        public string Equipment { get; set; }
        
        /// <summary>
        /// URL da imagem GIF demonstrando o exercício
        /// </summary>
        public string GifUrl { get; set; }
        
        /// <summary>
        /// Músculo alvo principal
        /// </summary>
        public string Target { get; set; }
        
        /// <summary>
        /// Músculos secundários trabalhados
        /// </summary>
        public List<string> SecondaryMuscles { get; set; }
        
        /// <summary>
        /// Instruções de execução passo a passo
        /// </summary>
        public List<string> Instructions { get; set; }
    }
}