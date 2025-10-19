using System.ComponentModel.DataAnnotations;
using ConnectWorkout.Core.Enums;

namespace ConnectWorkout.Core.DTOs
{
    /// <summary>
    /// DTO para atualização do perfil do aluno (Minha ficha)
    /// </summary>
    public class UpdateStudentProfileDto
    {
        /// <summary>
        /// Nome do aluno
        /// </summary>
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
        public string Name { get; set; }

        /// <summary>
        /// Gênero do aluno (Male = 1, Female = 2, Other = 3)
        /// </summary>
        [Required(ErrorMessage = "Gênero é obrigatório")]
        public Gender Gender { get; set; }

        /// <summary>
        /// Idade do aluno
        /// </summary>
        [Range(10, 120, ErrorMessage = "Idade deve estar entre 10 e 120 anos")]
        public int? Age { get; set; }

        /// <summary>
        /// Altura em centímetros
        /// </summary>
        [Range(50, 250, ErrorMessage = "Altura deve estar entre 50 e 250 cm")]
        public decimal? Height { get; set; }

        /// <summary>
        /// Peso em quilogramas
        /// </summary>
        [Range(20, 300, ErrorMessage = "Peso deve estar entre 20 e 300 kg")]
        public decimal? Weight { get; set; }

        /// <summary>
        /// Tipo de corpo (Corpo)
        /// </summary>
        [StringLength(100, ErrorMessage = "Tipo de corpo deve ter no máximo 100 caracteres")]
        public string BodyType { get; set; }

        /// <summary>
        /// Condições de saúde (Saúde)
        /// </summary>
        [StringLength(500, ErrorMessage = "Condições de saúde devem ter no máximo 500 caracteres")]
        public string HealthConditions { get; set; }

        /// <summary>
        /// Objetivo do aluno (Objetivo)
        /// </summary>
        [StringLength(500, ErrorMessage = "Objetivo deve ter no máximo 500 caracteres")]
        public string Goal { get; set; }

        /// <summary>
        /// Observações gerais (Observações)
        /// </summary>
        [StringLength(1000, ErrorMessage = "Observações devem ter no máximo 1000 caracteres")]
        public string Observations { get; set; }
    }
}
