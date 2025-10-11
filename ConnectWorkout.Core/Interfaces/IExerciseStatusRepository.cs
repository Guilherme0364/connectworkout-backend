using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConnectWorkout.Core.Enums;
using ConnectWorkout.Core.Models;

namespace ConnectWorkout.Core.Interfaces
{
    /// <summary>
    /// Interface para operações de repositório específicas de status de exercícios
    /// </summary>
    public interface IExerciseStatusRepository : IRepository<ExerciseStatus>
    {
        /// <summary>
        /// Obtém o status de um exercício para um aluno em uma data específica
        /// </summary>
        Task<ExerciseStatus> GetStatusForExerciseAndDateAsync(int exerciseId, int studentId, DateTime date);
        
        /// <summary>
        /// Obtém todos os status de exercícios de um aluno em uma data específica
        /// </summary>
        Task<IEnumerable<ExerciseStatus>> GetStatusesByStudentAndDateAsync(int studentId, DateTime date);
        
        /// <summary>
        /// Obtém todos os status de exercícios de um aluno em um intervalo de datas
        /// </summary>
        Task<IEnumerable<ExerciseStatus>> GetStatusesByDateRangeAsync(int studentId, DateTime startDate, DateTime endDate);
        
        /// <summary>
        /// Verifica se um exercício já tem um status registrado para uma data específica
        /// </summary>
        Task<bool> HasStatusForExerciseAsync(int exerciseId, int studentId, DateTime date);
        
        /// <summary>
        /// Conta quantos exercícios têm um status específico em uma data
        /// </summary>
        Task<int> CountStatusesByTypeAsync(int studentId, StatusType status, DateTime date);
    }
}