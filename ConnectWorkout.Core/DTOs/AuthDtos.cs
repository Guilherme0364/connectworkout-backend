using ConnectWorkout.Core.Enums;

namespace ConnectWorkout.Core.DTOs
{
    /// <summary>
    /// DTO para registro de um novo usuário
    /// </summary>
    public class RegisterUserDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int? Age { get; set; }
        public Gender? Gender { get; set; }
        public UserType UserType { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO para login de usuário
    /// </summary>
    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    /// <summary>
    /// DTO para resultado de operações de autenticação
    /// </summary>
    public class AuthResultDto
    {
        public bool Success { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string Message { get; set; }
        public UserDto User { get; set; }
    }

    /// <summary>
    /// DTO para informações de usuário
    /// </summary>
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int? Age { get; set; }
        public Gender? Gender { get; set; }
        public UserType UserType { get; set; }
        public string Description { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string BodyType { get; set; }
        public string HealthConditions { get; set; }
        public string Goal { get; set; }
        public string Observations { get; set; }
        public int TotalExercisesCount { get; set; }
    }
    
    /// <summary>
    /// DTO para atualização de informações de usuário
    /// </summary>
    public class UpdateUserDto
    {
    public string? Name { get; set; }
    public int? Age { get; set; }
    public Gender? Gender { get; set; }
    public string? Description { get; set; }
    public string CurrentPassword { get; set; } // Tornando opcional
    public string? NewPassword { get; set; } // Tornando opcional
    }
}