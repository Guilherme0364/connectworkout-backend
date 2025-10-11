using System.Collections.Generic;
using System.Threading.Tasks;
using ConnectWorkout.Core.DTOs;

namespace ConnectWorkout.Core.Interfaces
{
    /// <summary>
    /// Interface para o serviço de alunos
    /// </summary>
    public interface IStudentService
    {
        /// <summary>
        /// Obtém todos os instrutores de um aluno
        /// </summary>
        Task<IEnumerable<InstructorSummaryDto>> GetInstructorsAsync(int studentId);
        
        /// <summary>
        /// Obtém informações detalhadas de um instrutor específico
        /// </summary>
        Task<InstructorSummaryDto> GetInstructorDetailsAsync(int studentId, int instructorId);
    }
}