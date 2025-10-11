using System.Security.Claims;
using System.Threading.Tasks;
using ConnectWorkout.Core.DTOs;
using ConnectWorkout.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ConnectWorkout.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            // Obtém o ID do usuário a partir do token JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Usuário não autenticado." });
            }

            var userId = int.Parse(userIdClaim.Value);
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { message = "Usuário não encontrado." });
            }

            return Ok(user);
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(UpdateUserDto updateDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Usuário não autenticado." });
            }

            var userId = int.Parse(userIdClaim.Value);

            try
            {
                var updatedUser = await _userService.UpdateUserAsync(userId, updateDto);
                if (updatedUser == null)
                {
                    return NotFound(new { message = "Usuário não encontrado." });
                }

                return Ok(updatedUser);
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> SearchUsers([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var users = await _userService.SearchUsersAsync(query, page, pageSize);
            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Usuário não encontrado." });
            }

            return Ok(user);
        }
    }
}