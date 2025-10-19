using System.Collections.Generic;
using System.Threading.Tasks;
using ConnectWorkout.Core.Models;

namespace ConnectWorkout.Core.Interfaces
{
    /// <summary>
    /// Interface para operações de repositório específicas de exercícios
    /// </summary>
    public interface IExerciseRepository : IRepository<Exercise>
    {
        /// <summary>
        /// Obtém todos os exercícios de um dia de treino específico
        /// </summary>
        Task<IEnumerable<Exercise>> GetExercisesByWorkoutDayIdAsync(int workoutDayId);
        
        /// <summary>
        /// Obtém um exercício com seus status de conclusão
        /// </summary>
        Task<Exercise> GetExerciseWithStatusesAsync(int exerciseId);
        
        /// <summary>
        /// Obtém os exercícios programados para um aluno no dia de hoje
        /// </summary>
        Task<IEnumerable<Exercise>> GetExercisesForStudentTodayAsync(int studentId);

        /// <summary>
        /// Reordena os exercícios de um dia de treino
        /// </summary>
        Task ReorderExercisesAsync(List<int> exerciseIds);
    }
}