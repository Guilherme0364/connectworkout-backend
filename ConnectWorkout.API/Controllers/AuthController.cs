using System.Threading.Tasks;
using ConnectWorkout.Core.DTOs;
using ConnectWorkout.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ConnectWorkout.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
        {
            _logger.LogInformation("Register attempt for user with email: {Email}", registerDto.Email);
            
            var result = await _authService.RegisterUserAsync(registerDto);
            
            if (!result.Success)
            {
                _logger.LogWarning("Register failed for {Email}: {Message}", 
                    registerDto.Email, result.Message);
                return BadRequest(result);
            }
            
            _logger.LogInformation("User registered successfully: {Email}", registerDto.Email);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            _logger.LogInformation("Login attempt for user: {Email}", loginDto.Email);
            
            var result = await _authService.LoginAsync(loginDto);
            
            if (!result.Success)
            {
                _logger.LogWarning("Login failed for {Email}", loginDto.Email);
                return BadRequest(result);
            }
            
            _logger.LogInformation("User logged in successfully: {Email}", loginDto.Email);
            return Ok(result);
        }
    }
}