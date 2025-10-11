using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConnectWorkout.Core.Models;

namespace ConnectWorkout.Core.Interfaces
{
    /// <summary>
    /// Interface para operações de repositório específicas de dias de treino
    /// </summary>
    public interface IWorkoutDayRepository : IRepository<WorkoutDay>
    {
        /// <summary>
        /// Obtém todos os dias de treino de uma ficha específica
        /// </summary>
        Task<IEnumerable<WorkoutDay>> GetWorkoutDaysByWorkoutIdAsync(int workoutId);
        
        /// <summary>
        /// Obtém um dia de treino com seus exercícios
        /// </summary>
        Task<WorkoutDay> GetWorkoutDayWithExercisesAsync(int workoutDayId);
        
        /// <summary>
        /// Obtém um dia de treino específico por dia da semana
        /// </summary>
        Task<WorkoutDay> GetWorkoutDayByDayOfWeekAsync(int workoutId, DayOfWeek dayOfWeek);
    }
}