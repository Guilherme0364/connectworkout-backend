using System;
using System.Collections.Generic;
using ConnectWorkout.Core.Enums;

namespace ConnectWorkout.Core.Models
{
    /// <summary>
    /// Representa um usuário do sistema (Instrutor ou Aluno)
    /// </summary>
    public class User
    {
        /// <summary>
        /// Identificador único do usuário
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Nome completo do usuário
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Email do usuário (usado para login)
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// Hash da senha do usuário (nunca armazenamos a senha em texto puro)
        /// </summary>
        public string PasswordHash { get; set; }
        
        /// <summary>
        /// Idade do usuário (opcional)
        /// </summary>
        public int? Age { get; set; }
        
        /// <summary>
        /// Gênero do usuário (opcional)
        /// </summary>
        public Gender? Gender { get; set; }
        
        /// <summary>
        /// Tipo de usuário: Instrutor ou Aluno
        /// </summary>
        public UserType UserType { get; set; }
        
        /// <summary>
        /// Descrição ou biografia do usuário (opcional)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Altura do usuário em centímetros (opcional)
        /// </summary>
        public decimal? Height { get; set; }

        /// <summary>
        /// Peso do usuário em quilogramas (opcional)
        /// </summary>
        public decimal? Weight { get; set; }

        /// <summary>
        /// Tipo de corpo do usuário (opcional)
        /// </summary>
        public string BodyType { get; set; }

        /// <summary>
        /// Condições de saúde do usuário (opcional)
        /// </summary>
        public string HealthConditions { get; set; }

        /// <summary>
        /// Objetivo do aluno com os treinos (opcional)
        /// </summary>
        public string Goal { get; set; }

        /// <summary>
        /// Observações gerais sobre o aluno (opcional)
        /// </summary>
        public string Observations { get; set; }

        /// <summary>
        /// Número de telefone do usuário (opcional)
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Certificações do instrutor (JSON ou string separada por vírgulas) (opcional)
        /// </summary>
        public string? Certifications { get; set; }

        /// <summary>
        /// Especializações do instrutor (JSON ou string separada por vírgulas) (opcional)
        /// </summary>
        public string? Specializations { get; set; }

        /// <summary>
        /// Biografia detalhada do usuário (opcional, separada de Description)
        /// </summary>
        public string? Bio { get; set; }

        /// <summary>
        /// Anos de experiência do instrutor (opcional)
        /// </summary>
        public int? YearsOfExperience { get; set; }

        /// <summary>
        /// Links de redes sociais em formato JSON (opcional)
        /// Ex: {"instagram":"@user","facebook":"profile","website":"url"}
        /// </summary>
        public string? SocialLinksJson { get; set; }

        /// <summary>
        /// Data de criação do registro
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Data da última atualização do registro
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        // Propriedades de navegação para relacionamentos
        
        /// <summary>
        /// Para alunos: relacionamentos com seus instrutores
        /// </summary>
        public virtual ICollection<StudentInstructor> InstructorsRelations { get; set; }
        
        /// <summary>
        /// Para instrutores: relacionamentos com seus alunos
        /// </summary>
        public virtual ICollection<StudentInstructor> StudentsRelations { get; set; }
        
        /// <summary>
        /// Para alunos: suas fichas de treino
        /// </summary>
        public virtual ICollection<Workout> Workouts { get; set; }
        
        /// <summary>
        /// Para alunos: status dos exercícios realizados
        /// </summary>
        public virtual ICollection<ExerciseStatus> ExerciseStatuses { get; set; }
    }
}