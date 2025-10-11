using System;
using System.Threading.Tasks;
using ConnectWorkout.Core.DTOs;

namespace ConnectWorkout.Core.Interfaces
{
    /// <summary>
    /// Interface para o serviço de progresso de treino
    /// </summary>
    public interface IProgressService
    {
        /// <summary>
        /// Marca um exercício como concluído ou pulado
        /// </summary>
        Task<bool> MarkExerciseAsync(int studentId, MarkExerciseDto markDto);
        
        /// <summary>
        /// Obtém estatísticas de treino de um dia específico
        /// </summary>
        Task<DailyStatsDto> GetDailyStatsAsync(int studentId, DateTime date);
        
        /// <summary>
        /// Obtém estatísticas de treino da semana atual
        /// </summary>
        Task<WeeklyStatsDto> GetCurrentWeekStatsAsync(int studentId);
        
        /// <summary>
        /// Obtém estatísticas de treino de uma semana específica
        /// </summary>
        Task<WeeklyStatsDto> GetWeeklyStatsAsync(int studentId, DateTime weekStartDate);
    }
}