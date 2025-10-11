using System.Collections.Generic;
using System.Threading.Tasks;
using ConnectWorkout.Core.DTOs;

namespace ConnectWorkout.Core.Interfaces
{
    /// <summary>
    /// Interface para o serviço de fichas de treino
    /// </summary>
    public interface IWorkoutService
    {
        /// <summary>
        /// Cria uma nova ficha de treino
        /// </summary>
        Task<WorkoutDetailDto> CreateWorkoutAsync(int instructorId, CreateWorkoutDto createDto);
        
        /// <summary>
        /// Adiciona um novo dia de treino a uma ficha
        /// </summary>
        Task<WorkoutDayDto> AddWorkoutDayAsync(int workoutId, AddWorkoutDayDto dayDto);
        
        /// <summary>
        /// Adiciona um exercício a um dia de treino
        /// </summary>
        Task<ExerciseDetailDto> AddExerciseAsync(int workoutDayId, AddExerciseDto exerciseDto);
        
        /// <summary>
        /// Ativa uma ficha de treino (e desativa outras do mesmo aluno)
        /// </summary>
        Task<bool> ActivateWorkoutAsync(int workoutId);
        
        /// <summary>
        /// Obtém todas as fichas de treino de um aluno
        /// </summary>
        Task<IEnumerable<WorkoutSummaryDto>> GetWorkoutsByStudentAsync(int studentId);
        
        /// <summary>
        /// Obtém uma ficha de treino com todos os detalhes
        /// </summary>
        Task<WorkoutDetailDto> GetWorkoutDetailsAsync(int workoutId);
        
        /// <summary>
        /// Obtém o treino do dia atual para um aluno
        /// </summary>
        Task<WorkoutDayDto> GetTodaysWorkoutAsync(int studentId);
    }
}