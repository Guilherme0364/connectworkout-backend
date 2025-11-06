using System.Collections.Generic;
using System.Threading.Tasks;
using ConnectWorkout.Core.Models;

namespace ConnectWorkout.Core.Interfaces
{
    /// <summary>
    /// Interface para operações de repositório específicas de usuários
    /// </summary>
    public interface IUserRepository : IRepository<User>
    {
        /// <summary>
        /// Obtém um usuário pelo seu email
        /// </summary>
        Task<User> GetByEmailAsync(string email);
        
        /// <summary>
        /// Obtém todos os alunos de um instrutor específico
        /// </summary>
        Task<IEnumerable<User>> GetStudentsByInstructorIdAsync(int instructorId);
        
        /// <summary>
        /// Obtém todos os instrutores de um aluno específico
        /// </summary>
        Task<IEnumerable<User>> GetInstructorsByStudentIdAsync(int studentId);
        
        /// <summary>
        /// Verifica se um email já está em uso
        /// </summary>
        Task<bool> EmailExistsAsync(string email);

        /// <summary>
        /// Deleta permanentemente um usuário e todos os dados relacionados
        /// </summary>
        Task DeleteUserAsync(int userId);
    }
}