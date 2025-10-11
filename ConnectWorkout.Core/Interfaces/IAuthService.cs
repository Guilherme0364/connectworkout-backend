using System.Threading.Tasks;
using ConnectWorkout.Core.DTOs;

namespace ConnectWorkout.Core.Interfaces
{
    /// <summary>
    /// Interface para o serviço de autenticação
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registra um novo usuário
        /// </summary>
        Task<AuthResultDto> RegisterUserAsync(RegisterUserDto registerDto);
        
        /// <summary>
        /// Autentica um usuário
        /// </summary>
        Task<AuthResultDto> LoginAsync(LoginDto loginDto);
        
        /// <summary>
        /// Renova um token expirado usando um refresh token
        /// </summary>
        Task<AuthResultDto> RefreshTokenAsync(string refreshToken);
    }
}