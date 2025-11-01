using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
        private readonly IStudentInstructorRepository _studentInstructorRepository;
        private readonly IWorkoutRepository _workoutRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            IStudentInstructorRepository studentInstructorRepository,
            IWorkoutRepository workoutRepository,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _studentInstructorRepository = studentInstructorRepository;
            _workoutRepository = workoutRepository;
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

            // Atualizar novos campos de perfil
            if (!string.IsNullOrEmpty(updateDto.Phone))
                user.Phone = updateDto.Phone;

            if (!string.IsNullOrEmpty(updateDto.Certifications))
                user.Certifications = updateDto.Certifications;

            if (!string.IsNullOrEmpty(updateDto.Specializations))
                user.Specializations = updateDto.Specializations;

            if (!string.IsNullOrEmpty(updateDto.Bio))
                user.Bio = updateDto.Bio;

            if (updateDto.YearsOfExperience.HasValue)
                user.YearsOfExperience = updateDto.YearsOfExperience;

            // Serializar SocialLinks para JSON
            if (updateDto.SocialLinks != null)
            {
                user.SocialLinksJson = JsonSerializer.Serialize(updateDto.SocialLinks);
            }

            // Atualizar data de última modificação
            user.UpdatedAt = DateTime.UtcNow;

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
        
        public async Task<UserDto> GetStudentProfileAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("Student with ID {UserId} not found", userId);
                return null;
            }

            // Verificar se é realmente um estudante
            if (user.UserType != Core.Enums.UserType.Student)
            {
                _logger.LogWarning("User with ID {UserId} is not a student", userId);
                return null;
            }

            // Calculate total exercises count across all workouts
            var workouts = await _workoutRepository.GetWorkoutsByStudentIdAsync(userId);
            var totalExercisesCount = workouts
                .SelectMany(w => w.WorkoutDays ?? Enumerable.Empty<WorkoutDay>())
                .SelectMany(wd => wd.Exercises ?? Enumerable.Empty<Exercise>())
                .Count();

            return MapToUserDto(user, totalExercisesCount);
        }

        public async Task<InstructorSummaryDto> GetStudentInstructorAsync(int studentId)
        {
            try
            {
                // Buscar a relação ativa entre estudante e instrutor
                var relationships = await _studentInstructorRepository.FindAsync(
                    si => si.StudentId == studentId);

                var activeRelationship = relationships.FirstOrDefault();

                if (activeRelationship == null)
                {
                    _logger.LogInformation("No instructor found for student {StudentId}", studentId);
                    return null;
                }

                // Buscar informações do instrutor
                var instructor = await _userRepository.GetByIdAsync(activeRelationship.InstructorId);

                if (instructor == null)
                {
                    _logger.LogWarning("Instructor with ID {InstructorId} not found", activeRelationship.InstructorId);
                    return null;
                }

                return new InstructorSummaryDto
                {
                    Id = instructor.Id,
                    Name = instructor.Name,
                    Email = instructor.Email,
                    Description = instructor.Description
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting instructor for student {StudentId}", studentId);
                throw;
            }
        }

        public async Task<UserDto> UpdateStudentProfileAsync(int userId, UpdateStudentProfileDto updateDto)
        {
            try
            {
                // Get user from database
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found for profile update", userId);
                    return null;
                }

                // Verify user is a student
                if (user.UserType != Core.Enums.UserType.Student)
                {
                    _logger.LogWarning("User {UserId} is not a student, cannot update student profile", userId);
                    return null;
                }

                // Update user fields
                user.Name = updateDto.Name;
                user.Description = string.Empty; // Students don't have a description field in the profile update DTO
                user.Gender = updateDto.Gender;
                user.Age = updateDto.Age;
                user.Height = updateDto.Height;
                user.Weight = updateDto.Weight;
                user.BodyType = updateDto.BodyType ?? string.Empty;
                user.HealthConditions = updateDto.HealthConditions ?? string.Empty;
                user.Goal = updateDto.Goal ?? string.Empty;
                user.Observations = updateDto.Observations ?? string.Empty;

                // Save to database
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation("Student profile updated successfully for user {UserId}", userId);

                // Return updated profile
                return MapToUserDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student profile for user {UserId}", userId);
                throw;
            }
        }

        // Método para mapear User para UserDto
        private UserDto MapToUserDto(User user, int totalExercisesCount = 0)
        {
            // Deserializar SocialLinks do JSON
            SocialLinksDto socialLinks = null;
            if (!string.IsNullOrEmpty(user.SocialLinksJson))
            {
                try
                {
                    socialLinks = JsonSerializer.Deserialize<SocialLinksDto>(user.SocialLinksJson);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize SocialLinksJson for user {UserId}", user.Id);
                }
            }

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Age = user.Age,
                Gender = user.Gender,
                UserType = user.UserType,
                Description = user.Description,
                Height = user.Height,
                Weight = user.Weight,
                BodyType = user.BodyType,
                HealthConditions = user.HealthConditions,
                Goal = user.Goal,
                Observations = user.Observations,
                TotalExercisesCount = totalExercisesCount,

                // Novos campos
                Phone = user.Phone,
                Certifications = user.Certifications,
                Specializations = user.Specializations,
                Bio = user.Bio,
                YearsOfExperience = user.YearsOfExperience,
                SocialLinks = socialLinks,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}