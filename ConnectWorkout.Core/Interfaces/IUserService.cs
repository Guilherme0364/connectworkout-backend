using System.Collections.Generic;
using System.Threading.Tasks;
using ConnectWorkout.Core.DTOs;

namespace ConnectWorkout.Core.Interfaces
{
    /// <summary>
    /// Interface para o serviço de usuários
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Obtém informações de um usuário pelo ID
        /// </summary>
        Task<UserDto> GetUserByIdAsync(int userId);

        /// <summary>
        /// Atualiza informações de um usuário
        /// </summary>
        Task<UserDto> UpdateUserAsync(int userId, UpdateUserDto updateDto);

        /// <summary>
        /// Busca usuários por nome ou email
        /// </summary>
        Task<IEnumerable<UserDto>> SearchUsersAsync(string query, int page = 1, int pageSize = 10);

        /// <summary>
        /// Obtém o perfil de um estudante
        /// </summary>
        Task<UserDto> GetStudentProfileAsync(int userId);

        /// <summary>
        /// Obtém o instrutor atribuído a um estudante
        /// </summary>
        Task<InstructorSummaryDto> GetStudentInstructorAsync(int studentId);

        /// <summary>
        /// Atualiza o perfil de um estudante (Minha ficha)
        /// </summary>
        Task<UserDto> UpdateStudentProfileAsync(int userId, UpdateStudentProfileDto updateDto);

        /// <summary>
        /// Deleta permanentemente a conta de um usuário e todos os dados relacionados
        /// </summary>
        Task DeleteAccountAsync(int userId);
    }
}