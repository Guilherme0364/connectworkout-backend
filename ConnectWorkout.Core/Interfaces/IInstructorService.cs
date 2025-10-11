using System.Collections.Generic;
using System.Threading.Tasks;
using ConnectWorkout.Core.DTOs;

namespace ConnectWorkout.Core.Interfaces
{
    /// <summary>
    /// Interface para o serviço de instrutores
    /// </summary>
    public interface IInstructorService
    {
        /// <summary>
        /// Conecta um instrutor a um aluno
        /// </summary>
        Task<bool> ConnectWithStudentAsync(int instructorId, ConnectStudentDto connectDto);
        
        /// <summary>
        /// Remove a conexão entre um instrutor e um aluno
        /// </summary>
        Task<bool> RemoveStudentAsync(int instructorId, int studentId);
        
        /// <summary>
        /// Obtém todos os alunos de um instrutor
        /// </summary>
        Task<IEnumerable<StudentSummaryDto>> GetStudentsAsync(int instructorId);
        
        /// <summary>
        /// Obtém informações detalhadas de um aluno específico
        /// </summary>
        Task<StudentSummaryDto> GetStudentDetailsAsync(int instructorId, int studentId);
    }
}