using System.Threading.Tasks;
using ConnectWorkout.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ConnectWorkout.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExercisesController : ControllerBase
    {
        private readonly IExerciseDbService _exerciseDbService;
        private readonly ILogger<ExercisesController> _logger;

        public ExercisesController(IExerciseDbService exerciseDbService, ILogger<ExercisesController> logger)
        {
            _exerciseDbService = exerciseDbService;
            _logger = logger;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchExercises(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return BadRequest(new { message = "O parâmetro de busca não pode ser vazio." });
            }

            var exercises = await _exerciseDbService.SearchExercisesByNameAsync(name);
            return Ok(exercises);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetExerciseById(string id)
        {
            try
            {
                var exercise = await _exerciseDbService.GetExerciseByIdAsync(id);
                if (exercise == null)
                {
                    return NotFound(new { message = "Exercício não encontrado." });
                }

                return Ok(exercise);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar exercício pelo ID {Id}", id);
                return StatusCode(500, new { message = "Erro ao buscar exercício." });
            }
        }

        [HttpGet("bodyparts")]
        public async Task<IActionResult> GetBodyParts()
        {
            try
            {
                var bodyParts = await _exerciseDbService.GetBodyPartsListAsync();
                return Ok(bodyParts);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar partes do corpo");
                return StatusCode(500, new { message = "Erro ao buscar partes do corpo." });
            }
        }

        [HttpGet("bodypart/{bodyPart}")]
        public async Task<IActionResult> GetExercisesByBodyPart(string bodyPart)
        {
            try
            {
                var exercises = await _exerciseDbService.GetExercisesByBodyPartAsync(bodyPart);
                return Ok(exercises);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar exercícios para a parte do corpo {BodyPart}", bodyPart);
                return StatusCode(500, new { message = "Erro ao buscar exercícios." });
            }
        }

        [HttpGet("targets")]
        public async Task<IActionResult> GetTargets()
        {
            try
            {
                var targets = await _exerciseDbService.GetTargetsListAsync();
                return Ok(targets);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar músculos alvo");
                return StatusCode(500, new { message = "Erro ao buscar músculos alvo." });
            }
        }

        [HttpGet("target/{target}")]
        public async Task<IActionResult> GetExercisesByTarget(string target)
        {
            try
            {
                var exercises = await _exerciseDbService.GetExercisesByTargetAsync(target);
                return Ok(exercises);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar exercícios para o músculo alvo {Target}", target);
                return StatusCode(500, new { message = "Erro ao buscar exercícios." });
            }
        }

        [HttpGet("equipments")]
        public async Task<IActionResult> GetEquipments()
        {
            try
            {
                var equipments = await _exerciseDbService.GetEquipmentsListAsync();
                return Ok(equipments);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar equipamentos");
                return StatusCode(500, new { message = "Erro ao buscar equipamentos." });
            }
        }

        [HttpGet("equipment/{equipment}")]
        public async Task<IActionResult> GetExercisesByEquipment(string equipment)
        {
            try
            {
                var exercises = await _exerciseDbService.GetExercisesByEquipmentAsync(equipment);
                return Ok(exercises);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar exercícios para o equipamento {Equipment}", equipment);
                return StatusCode(500, new { message = "Erro ao buscar exercícios." });
            }
        }
    }
}