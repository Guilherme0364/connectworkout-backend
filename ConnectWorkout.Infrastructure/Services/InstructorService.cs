using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConnectWorkout.Core.DTOs;
using ConnectWorkout.Core.Enums;
using ConnectWorkout.Core.Interfaces;
using ConnectWorkout.Core.Models;
using Microsoft.Extensions.Logging;

namespace ConnectWorkout.Infrastructure.Services
{
    /// <summary>
    /// Serviço para operações específicas de instrutores
    /// </summary>
    public class InstructorService : IInstructorService
    {
        private readonly IUserRepository _userRepository;
        private readonly IStudentInstructorRepository _studentInstructorRepository;
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IExerciseStatusRepository _exerciseStatusRepository;
        private readonly ILogger<InstructorService> _logger;

        public InstructorService(
            IUserRepository userRepository,
            IStudentInstructorRepository studentInstructorRepository,
            IWorkoutRepository workoutRepository,
            IExerciseStatusRepository exerciseStatusRepository,
            ILogger<InstructorService> logger)
        {
            _userRepository = userRepository;
            _studentInstructorRepository = studentInstructorRepository;
            _workoutRepository = workoutRepository;
            _exerciseStatusRepository = exerciseStatusRepository;
            _logger = logger;
        }

        public async Task<bool> ConnectWithStudentAsync(int instructorId, ConnectStudentDto connectDto)
        {
            // Verificar se o instrutor existe
            var instructor = await _userRepository.GetByIdAsync(instructorId);
            if (instructor == null || instructor.UserType != UserType.Instructor)
            {
                _logger.LogWarning("Instructor with ID {InstructorId} not found", instructorId);
                return false;
            }

            // Validar que ao menos StudentId ou Email foi fornecido
            if (!connectDto.StudentId.HasValue && string.IsNullOrWhiteSpace(connectDto.Email))
            {
                _logger.LogWarning("Neither StudentId nor Email provided in ConnectStudentDto");
                return false;
            }

            // Buscar aluno por ID ou Email
            User student = null;

            if (connectDto.StudentId.HasValue)
            {
                student = await _userRepository.GetByIdAsync(connectDto.StudentId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(connectDto.Email))
            {
                student = await _userRepository.GetByEmailAsync(connectDto.Email);
            }

            // Verificar se o aluno existe e é realmente um estudante
            if (student == null || student.UserType != UserType.Student)
            {
                _logger.LogWarning("Student not found or user is not a student. StudentId: {StudentId}, Email: {Email}",
                    connectDto.StudentId, connectDto.Email);
                return false;
            }

            // Verificar se já existe conexão
            bool connectionExists = await _studentInstructorRepository.ConnectionExistsAsync(
                student.Id, instructorId);

            if (connectionExists)
            {
                _logger.LogInformation(
                    "Connection between instructor {InstructorId} and student {StudentId} already exists",
                    instructorId, student.Id);
                return true; // Já está conectado
            }

            // Criar nova conexão
            var connection = new StudentInstructor
            {
                StudentId = student.Id,
                InstructorId = instructorId,
                ConnectedAt = DateTime.UtcNow
            };

            await _studentInstructorRepository.AddAsync(connection);
            await _studentInstructorRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Connection created between instructor {InstructorId} and student {StudentId}",
                instructorId, student.Id);

            return true;
        }

        public async Task<bool> RemoveStudentAsync(int instructorId, int studentId)
        {
            // Verificar se existe conexão
            bool connectionExists = await _studentInstructorRepository.ConnectionExistsAsync(
                studentId, instructorId);
                
            if (!connectionExists)
            {
                _logger.LogWarning(
                    "Connection between instructor {InstructorId} and student {StudentId} not found",
                    instructorId, studentId);
                return false;
            }
            
            // Remover conexão
            await _studentInstructorRepository.RemoveConnectionAsync(studentId, instructorId);
            
            _logger.LogInformation(
                "Connection removed between instructor {InstructorId} and student {StudentId}",
                instructorId, studentId);
                
            return true;
        }

        public async Task<IEnumerable<StudentSummaryDto>> GetStudentsAsync(int instructorId)
        {
            // Verificar se o instrutor existe
            var instructor = await _userRepository.GetByIdAsync(instructorId);
            if (instructor == null || instructor.UserType != UserType.Instructor)
            {
                _logger.LogWarning("Instructor with ID {InstructorId} not found", instructorId);
                return Enumerable.Empty<StudentSummaryDto>();
            }
            
            // Obter todos os alunos do instrutor
            var students = await _userRepository.GetStudentsByInstructorIdAsync(instructorId);
            
            // Converter para DTOs com informações resumidas
            var studentDtos = new List<StudentSummaryDto>();
            
            foreach (var student in students)
            {
                // Obter o treino ativo do aluno (se houver)
                var activeWorkout = await _workoutRepository.GetActiveWorkoutForStudentAsync(student.Id);
                
                // Obter estatísticas do dia
                var today = DateTime.UtcNow.Date;
                var completedExercises = await _exerciseStatusRepository.CountStatusesByTypeAsync(
                    student.Id, StatusType.Completed, today);
                    
                // Calcular total de exercícios para hoje (simplificado)
                int totalExercises = 0;
                if (activeWorkout != null)
                {
                    var todayDayOfWeek = DateTime.UtcNow.DayOfWeek;
                    var todayWorkoutDay = activeWorkout.WorkoutDays
                        .FirstOrDefault(wd => wd.DayOfWeek == todayDayOfWeek);
                        
                    if (todayWorkoutDay != null)
                    {
                        totalExercises = todayWorkoutDay.Exercises.Count;
                    }
                }
                
                studentDtos.Add(new StudentSummaryDto
                {
                    Id = student.Id,
                    Name = student.Name,
                    Email = student.Email,
                    Age = student.Age,
                    ActiveWorkoutId = activeWorkout?.Id ?? 0,
                    ActiveWorkoutName = activeWorkout?.Name ?? "Nenhum treino ativo",
                    CompletedExercisesToday = completedExercises,
                    TotalExercisesToday = totalExercises
                });
            }
            
            return studentDtos;
        }

        public async Task<StudentSummaryDto> GetStudentDetailsAsync(int instructorId, int studentId)
        {
            // Verificar se existe conexão entre instrutor e aluno
            bool connectionExists = await _studentInstructorRepository.ConnectionExistsAsync(
                studentId, instructorId);
                
            if (!connectionExists)
            {
                _logger.LogWarning(
                    "Connection between instructor {InstructorId} and student {StudentId} not found",
                    instructorId, studentId);
                return null;
            }
            
            // Obter informações do aluno
            var student = await _userRepository.GetByIdAsync(studentId);
            if (student == null)
            {
                _logger.LogWarning("Student with ID {StudentId} not found", studentId);
                return null;
            }
            
            // Obter o treino ativo do aluno (se houver)
            var activeWorkout = await _workoutRepository.GetActiveWorkoutForStudentAsync(studentId);
            
            // Obter estatísticas do dia
            var today = DateTime.UtcNow.Date;
            var completedExercises = await _exerciseStatusRepository.CountStatusesByTypeAsync(
                studentId, StatusType.Completed, today);
                
            // Calcular total de exercícios para hoje (simplificado)
            int totalExercises = 0;
            if (activeWorkout != null)
            {
                var todayDayOfWeek = DateTime.UtcNow.DayOfWeek;
                var todayWorkoutDay = activeWorkout.WorkoutDays
                    .FirstOrDefault(wd => wd.DayOfWeek == todayDayOfWeek);
                    
                if (todayWorkoutDay != null)
                {
                    totalExercises = todayWorkoutDay.Exercises.Count;
                }
            }
            
            return new StudentSummaryDto
            {
                Id = student.Id,
                Name = student.Name,
                Email = student.Email,
                Age = student.Age,
                ActiveWorkoutId = activeWorkout?.Id ?? 0,
                ActiveWorkoutName = activeWorkout?.Name ?? "Nenhum treino ativo",
                CompletedExercisesToday = completedExercises,
                TotalExercisesToday = totalExercises
            };
        }
    }
}