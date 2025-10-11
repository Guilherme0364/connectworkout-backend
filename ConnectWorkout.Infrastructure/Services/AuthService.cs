using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ConnectWorkout.Core.DTOs;
using ConnectWorkout.Core.Interfaces;
using ConnectWorkout.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ConnectWorkout.Infrastructure.Services
{
    /// <summary>
    /// Serviço responsável pela autenticação de usuários
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<AuthResultDto> RegisterUserAsync(RegisterUserDto registerDto)
        {
            // Verificar se o email já existe
            if (await _userRepository.EmailExistsAsync(registerDto.Email))
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Este email já está em uso."
                };
            }

            // Criar hash da senha
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            // Criar novo usuário
            var user = new User
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                Age = registerDto.Age,
                Gender = registerDto.Gender,
                UserType = registerDto.UserType,
                Description = registerDto.Description ?? string.Empty,
                // Inicializar coleções vazias
                InstructorsRelations = new List<StudentInstructor>(),
                StudentsRelations = new List<StudentInstructor>(),
                Workouts = new List<Workout>(),
                ExerciseStatuses = new List<ExerciseStatus>()
            };

            // Salvar usuário no banco de dados
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            // Gerar token JWT
            var token = GenerateJwtToken(user);

            // Mapear usuário para DTO
            var userDto = MapToUserDto(user);

            // Retornar resultado
            return new AuthResultDto
            {
                Success = true,
                AccessToken = token,
                RefreshToken = GenerateRefreshToken(),
                Message = "Usuário registrado com sucesso!",
                User = userDto
            };
        }

        public async Task<AuthResultDto> LoginAsync(LoginDto loginDto)
        {
            // Buscar usuário pelo email
            var user = await _userRepository.GetByEmailAsync(loginDto.Email);

            // Verificar se o usuário existe
            if (user == null)
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Email ou senha incorretos."
                };
            }

            // Verificar se a senha está correta
            bool validPassword = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
            if (!validPassword)
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Email ou senha incorretos."
                };
            }

            // Gerar token JWT
            var token = GenerateJwtToken(user);

            // Mapear usuário para DTO
            var userDto = MapToUserDto(user);

            // Retornar resultado
            return new AuthResultDto
            {
                Success = true,
                AccessToken = token,
                RefreshToken = GenerateRefreshToken(),
                Message = "Login realizado com sucesso!",
                User = userDto
            };
        }

        public Task<AuthResultDto> RefreshTokenAsync(string refreshToken)
        {
            // Implementação simples - em um cenário real, você validaria o refresh token
            // e recuperaria o usuário associado a ele
            return Task.FromResult(new AuthResultDto
            {
                Success = false,
                Message = "Refresh token inválido ou expirado."
            });
        }

        // Método para gerar token JWT
        private string GenerateJwtToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenHandler = new JwtSecurityTokenHandler();
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Role, user.UserType.ToString()),
                    new Claim("UserType", user.UserType.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:DurationInMinutes"])),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // Método simples para gerar refresh token
        private string GenerateRefreshToken()
        {
            // Em um cenário real, você usaria um método mais seguro
            return Guid.NewGuid().ToString();
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