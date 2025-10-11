using System.Collections.Generic;
using System.Threading.Tasks;
using ConnectWorkout.Core.Models;

namespace ConnectWorkout.Core.Interfaces
{
    /// <summary>
    /// Interface para operações de repositório específicas de fichas de treino
    /// </summary>
    public interface IWorkoutRepository : IRepository<Workout>
    {
        /// <summary>
        /// Obtém todas as fichas de treino de um aluno específico
        /// </summary>
        Task<IEnumerable<Workout>> GetWorkoutsByStudentIdAsync(int studentId);
        
        /// <summary>
        /// Obtém uma ficha de treino com seus dias e exercícios
        /// </summary>
        Task<Workout> GetWorkoutWithDaysAndExercisesAsync(int workoutId);
        
        /// <summary>
        /// Obtém a ficha de treino ativa de um aluno
        /// </summary>
        Task<Workout> GetActiveWorkoutForStudentAsync(int studentId);
    }
}