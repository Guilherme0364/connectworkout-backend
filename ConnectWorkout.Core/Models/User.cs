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