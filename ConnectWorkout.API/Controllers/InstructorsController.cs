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
    }
}