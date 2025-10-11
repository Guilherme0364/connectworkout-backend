using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConnectWorkout.Core.DTOs;
using ConnectWorkout.Core.Interfaces;
using ConnectWorkout.Core.Models;
using Microsoft.Extensions.Logging;

namespace ConnectWorkout.Infrastructure.Services
{
    /// <summary>
    /// Serviço para operações relacionadas a usuários
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<UserDto> GetUserByIdAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", userId);
                return null;
            }
            
            return MapToUserDto(user);
        }

        public async Task<UserDto> UpdateUserAsync(int userId, UpdateUserDto updateDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found for update", userId);
                return null;
            }
            
            // Atualizar informações básicas
            if (!string.IsNullOrEmpty(updateDto.Name))
                user.Name = updateDto.Name;
                
            if (updateDto.Age.HasValue)
                user.Age = updateDto.Age;
                
            if (updateDto.Gender.HasValue)
                user.Gender = updateDto.Gender;
                
            if (!string.IsNullOrEmpty(updateDto.Description))
                user.Description = updateDto.Description;
            
            // Atualizar senha se fornecida
            if (!string.IsNullOrEmpty(updateDto.CurrentPassword) && !string.IsNullOrEmpty(updateDto.NewPassword))
            {
                // Verificar senha atual
                bool validPassword = BCrypt.Net.BCrypt.Verify(updateDto.CurrentPassword, user.PasswordHash);
                
                if (!validPassword)
                {
                    _logger.LogWarning("Invalid current password for user {UserId}", userId);
                    throw new InvalidOperationException("Senha atual incorreta.");
                }
                
                // Atualizar para nova senha
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateDto.NewPassword);
            }
            
            // Salvar alterações
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();
            
            _logger.LogInformation("User {UserId} updated successfully", userId);
            
            return MapToUserDto(user);
        }

        public async Task<IEnumerable<UserDto>> SearchUsersAsync(string query, int page = 1, int pageSize = 10)
        {
            // Buscar usuários que correspondam à consulta no nome ou email
            var users = await _userRepository.FindAsync(u => 
                u.Name.Contains(query) || 
                u.Email.Contains(query));
            
            // Aplicar paginação
            var pagedUsers = users
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
            
            // Converter para DTOs
            return pagedUsers.Select(MapToUserDto);
        }
        
        // Método para mapear User para UserDto
        private UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Age = user.Age,
                Gender = user.Gender,
                UserType = user.UserType,
                Description = user.Description
            };
        }
    }
}