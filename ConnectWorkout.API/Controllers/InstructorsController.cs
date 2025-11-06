using System.Security.Claims;
using System.Threading.Tasks;
using ConnectWorkout.Core.DTOs;
using ConnectWorkout.Core.Enums;
using ConnectWorkout.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ConnectWorkout.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InstructorsController : ControllerBase
    {
        private readonly IInstructorService _instructorService;
        private readonly IUserService _userService;
        private readonly ILogger<InstructorsController> _logger;

        public InstructorsController(
            IInstructorService instructorService,
            IUserService userService,
            ILogger<InstructorsController> logger)
        {
            _instructorService = instructorService;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("students")]
        public async Task<IActionResult> GetStudents()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Usuário não autenticado." });
            }

            var instructorId = int.Parse(userIdClaim.Value);
            
            // Verificar se o usuário é um instrutor
            var user = await _userService.GetUserByIdAsync(instructorId);
            if (user == null || user.UserType != UserType.Instructor)
            {
                return Forbid();
            }

            var students = await _instructorService.GetStudentsAsync(instructorId);
            return Ok(students);
        }

        [HttpGet("students/{studentId}")]
        public async Task<IActionResult> GetStudentDetails(int studentId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Usuário não autenticado." });
            }

            var instructorId = int.Parse(userIdClaim.Value);
            
            // Verificar se o usuário é um instrutor
            var user = await _userService.GetUserByIdAsync(instructorId);
            if (user == null || user.UserType != UserType.Instructor)
            {
                return Forbid();
            }

            var student = await _instructorService.GetStudentDetailsAsync(instructorId, studentId);
            if (student == null)
            {
                return NotFound(new { message = "Aluno não encontrado ou não está vinculado a este instrutor." });
            }

            return Ok(student);
        }

        [HttpPost("connect")]
        public async Task<IActionResult> ConnectWithStudent(ConnectStudentDto connectDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Usuário não autenticado." });
            }

            var instructorId = int.Parse(userIdClaim.Value);
            
            // Verificar se o usuário é um instrutor
            var user = await _userService.GetUserByIdAsync(instructorId);
            if (user == null || user.UserType != UserType.Instructor)
            {
                return Forbid();
            }

            var result = await _instructorService.ConnectWithStudentAsync(instructorId, connectDto);
            
            if (!result)
            {
                return BadRequest(new { message = "Não foi possível conectar com o aluno." });
            }

            return Ok(new { message = "Conectado com sucesso ao aluno." });
        }

        [HttpDelete("students/{studentId}")]
        public async Task<IActionResult> RemoveStudent(int studentId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Usuário não autenticado." });
            }

            var instructorId = int.Parse(userIdClaim.Value);
            
            // Verificar se o usuário é um instrutor
            var user = await _userService.GetUserByIdAsync(instructorId);
            if (user == null || user.UserType != UserType.Instructor)
            {
                return Forbid();
            }

            var result = await _instructorService.RemoveStudentAsync(instructorId, studentId);

            if (!result)
            {
                return NotFound(new { message = "Aluno não encontrado ou não está vinculado a este instrutor." });
            }

            return Ok(new { message = "Aluno removido com sucesso." });
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics([FromQuery] string period = "month")
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Usuário não autenticado." });
            }

            var instructorId = int.Parse(userIdClaim.Value);

            // Verificar se o usuário é um instrutor
            var user = await _userService.GetUserByIdAsync(instructorId);
            if (user == null || user.UserType != UserType.Instructor)
            {
                return Forbid();
            }

            var statistics = await _instructorService.GetInstructorStatisticsAsync(instructorId, period);

            if (statistics == null)
            {
                return NotFound(new { message = "Não foi possível obter as estatísticas." });
            }

            return Ok(statistics);
        }

        [HttpGet("invitations")]
        public async Task<IActionResult> GetInvitations()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Usuário não autenticado." });
            }

            var instructorId = int.Parse(userIdClaim.Value);

            // Verificar se o usuário é um instrutor
            var user = await _userService.GetUserByIdAsync(instructorId);
            if (user == null || user.UserType != UserType.Instructor)
            {
                return Forbid();
            }

            var invitations = await _instructorService.GetInvitationsAsync(instructorId);

            return Ok(invitations);
        }

        /// <summary>
        /// Delete the authenticated instructor's account and all associated data
        /// </summary>
        [HttpDelete("account")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteAccount()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Usuário não autenticado." });
                }

                var instructorId = int.Parse(userIdClaim.Value);

                _logger.LogInformation("Instructor {InstructorId} requesting account deletion", instructorId);

                // Verificar se o usuário é um instrutor
                var user = await _userService.GetUserByIdAsync(instructorId);
                if (user == null || user.UserType != UserType.Instructor)
                {
                    return Forbid();
                }

                // Delete account
                await _userService.DeleteAccountAsync(instructorId);

                _logger.LogInformation("Account deleted successfully for instructor {InstructorId}", instructorId);

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error deleting instructor account");
                return StatusCode(500, new { message = "Error deleting account" });
            }
        }
    }
}