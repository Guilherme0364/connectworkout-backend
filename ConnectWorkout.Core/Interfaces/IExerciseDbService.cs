using System.Collections.Generic;
using System.Threading.Tasks;
using ConnectWorkout.Core.DTOs;

namespace ConnectWorkout.Core.Interfaces
{
    /// <summary>
    /// Interface para o serviço que integra com a API ExerciseDB
    /// </summary>
    public interface IExerciseDbService
    {
        /// <summary>
        /// Obtém todos os exercícios disponíveis
        /// </summary>
        Task<List<ExerciseDbModel>> GetAllExercisesAsync();
        
        /// <summary>
        /// Obtém um exercício específico pelo ID
        /// </summary>
        Task<ExerciseDbModel> GetExerciseByIdAsync(string id);
        
        /// <summary>
        /// Busca exercícios pelo nome
        /// </summary>
        Task<List<ExerciseDbModel>> SearchExercisesByNameAsync(string name);
        
        /// <summary>
        /// Obtém exercícios por parte do corpo
        /// </summary>
        Task<List<ExerciseDbModel>> GetExercisesByBodyPartAsync(string bodyPart);
        
        /// <summary>
        /// Obtém exercícios por músculo alvo
        /// </summary>
        Task<List<ExerciseDbModel>> GetExercisesByTargetAsync(string target);
        
        /// <summary>
        /// Obtém exercícios por equipamento
        /// </summary>
        Task<List<ExerciseDbModel>> GetExercisesByEquipmentAsync(string equipment);
        
        /// <summary>
        /// Obtém a lista de partes do corpo disponíveis
        /// </summary>
        Task<List<string>> GetBodyPartsListAsync();
        
        /// <summary>
        /// Obtém a lista de músculos alvo disponíveis
        /// </summary>
        Task<List<string>> GetTargetsListAsync();
        
        /// <summary>
        /// Obtém a lista de equipamentos disponíveis
        /// </summary>
        Task<List<string>> GetEquipmentsListAsync();
    }
}