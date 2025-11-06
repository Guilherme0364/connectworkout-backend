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
        private readonly IRepository<StudentInstructor> _studentInstructorRawRepository;
        private readonly IRepository<Workout> _workoutRawRepository;
        private readonly IRepository<ExerciseStatus> _exerciseStatusRawRepository;
        private readonly ILogger<InstructorService> _logger;

        public InstructorService(
            IUserRepository userRepository,
            IStudentInstructorRepository studentInstructorRepository,
            IWorkoutRepository workoutRepository,
            IExerciseStatusRepository exerciseStatusRepository,
            IRepository<StudentInstructor> studentInstructorRawRepository,
            IRepository<Workout> workoutRawRepository,
            IRepository<ExerciseStatus> exerciseStatusRawRepository,
            ILogger<InstructorService> logger)
        {
            _userRepository = userRepository;
            _studentInstructorRepository = studentInstructorRepository;
            _workoutRepository = workoutRepository;
            _exerciseStatusRepository = exerciseStatusRepository;
            _studentInstructorRawRepository = studentInstructorRawRepository;
            _workoutRawRepository = workoutRawRepository;
            _exerciseStatusRawRepository = exerciseStatusRawRepository;
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

            // Verificar se já existe uma conexão aceita
            bool acceptedConnectionExists = await _studentInstructorRepository
                .AcceptedConnectionExistsAsync(student.Id, instructorId);

            if (acceptedConnectionExists)
            {
                _logger.LogInformation(
                    "Accepted connection between instructor {InstructorId} and student {StudentId} already exists",
                    instructorId, student.Id);
                return true; // Já está conectado
            }

            // Verificar se já existe um convite pendente
            bool pendingInvitationExists = await _studentInstructorRepository
                .PendingInvitationExistsAsync(student.Id, instructorId);

            if (pendingInvitationExists)
            {
                _logger.LogInformation(
                    "Pending invitation already exists for instructor {InstructorId} and student {StudentId}",
                    instructorId, student.Id);
                return false; // Não criar convite duplicado
            }

            // Criar novo convite (pendente)
            var invitation = new StudentInstructor
            {
                StudentId = student.Id,
                InstructorId = instructorId,
                Status = Core.Enums.InvitationStatus.Pending,
                InvitedAt = DateTime.UtcNow
                // ConnectedAt será definido quando o aluno aceitar o convite
            };

            await _studentInstructorRepository.AddAsync(invitation);
            await _studentInstructorRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Invitation sent from instructor {InstructorId} to student {StudentId}",
                instructorId, student.Id);

            return true;
        }

        public async Task<bool> RemoveStudentAsync(int instructorId, int studentId)
        {
            // Verificar se existe conexão aceita
            bool acceptedConnectionExists = await _studentInstructorRepository
                .AcceptedConnectionExistsAsync(studentId, instructorId);

            if (!acceptedConnectionExists)
            {
                _logger.LogWarning(
                    "Accepted connection between instructor {InstructorId} and student {StudentId} not found",
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

            // Obter todos os relacionamentos estudante-instrutor aceitos para obter ConnectedAt
            var studentRelations = await _studentInstructorRawRepository
                .FindAsync(si => si.InstructorId == instructorId && si.Status == Core.Enums.InvitationStatus.Accepted);
            var relationshipMap = studentRelations.ToDictionary(si => si.StudentId, si => si.ConnectedAt ?? si.InvitedAt);

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
                    TotalExercisesToday = totalExercises,
                    EnrolledAt = relationshipMap.ContainsKey(student.Id) ? relationshipMap[student.Id] : DateTime.MinValue
                });
            }

            return studentDtos;
        }

        public async Task<StudentSummaryDto> GetStudentDetailsAsync(int instructorId, int studentId)
        {
            // Verificar se existe conexão aceita entre instrutor e aluno
            bool acceptedConnectionExists = await _studentInstructorRepository
                .AcceptedConnectionExistsAsync(studentId, instructorId);

            if (!acceptedConnectionExists)
            {
                _logger.LogWarning(
                    "Accepted connection between instructor {InstructorId} and student {StudentId} not found",
                    instructorId, studentId);
                return null;
            }

            // Obter a data de conexão
            var connection = await _studentInstructorRawRepository
                .FindAsync(si => si.StudentId == studentId && si.InstructorId == instructorId);
            var enrolledAt = connection.FirstOrDefault()?.ConnectedAt ?? DateTime.MinValue;

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
                TotalExercisesToday = totalExercises,
                EnrolledAt = enrolledAt
            };
        }

        public async Task<InstructorStatisticsDto> GetInstructorStatisticsAsync(int instructorId, string period = "month")
        {
            // Verificar se o instrutor existe
            var instructor = await _userRepository.GetByIdAsync(instructorId);
            if (instructor == null || instructor.UserType != UserType.Instructor)
            {
                _logger.LogWarning("Instructor with ID {InstructorId} not found", instructorId);
                return null;
            }

            // Obter todos os alunos do instrutor
            var students = await _userRepository.GetStudentsByInstructorIdAsync(instructorId);
            var studentIds = students.Select(s => s.Id).ToList();

            // Definir períodos de tempo
            var now = DateTime.UtcNow;
            var today = now.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfPreviousWeek = startOfWeek.AddDays(-7);
            var startOfPreviousMonth = startOfMonth.AddMonths(-1);

            // ESTATÍSTICAS DE ALUNOS
            var allStudentRelations = await _studentInstructorRawRepository
                .FindAsync(si => si.InstructorId == instructorId);

            var totalStudents = studentIds.Count;
            var newStudentsThisWeek = allStudentRelations
                .Count(sr => sr.ConnectedAt >= startOfWeek);
            var newStudentsThisMonth = allStudentRelations
                .Count(sr => sr.ConnectedAt >= startOfMonth);
            var newStudentsPreviousMonth = allStudentRelations
                .Count(sr => sr.ConnectedAt >= startOfPreviousMonth && sr.ConnectedAt < startOfMonth);

            // ESTATÍSTICAS DE TREINOS
            var allWorkouts = await _workoutRawRepository
                .FindAsync(w => studentIds.Contains(w.StudentId));

            var workoutsCreatedTotal = allWorkouts.Count();
            var workoutsCreatedThisMonth = allWorkouts
                .Count(w => w.CreatedAt >= startOfMonth);
            var workoutsCreatedThisWeek = allWorkouts
                .Count(w => w.CreatedAt >= startOfWeek);
            var workoutsCreatedPreviousMonth = allWorkouts
                .Count(w => w.CreatedAt >= startOfPreviousMonth && w.CreatedAt < startOfMonth);

            // Alunos ativos (com treinos ativos)
            var activeStudents = allWorkouts
                .Where(w => w.IsActive)
                .Select(w => w.StudentId)
                .Distinct()
                .Count();

            // ESTATÍSTICAS DE CONCLUSÃO
            var allExerciseStatuses = await _exerciseStatusRawRepository
                .FindAsync(es => studentIds.Contains(es.StudentId));

            // Estatísticas de hoje
            var todayStatuses = allExerciseStatuses.Where(es => es.Date.Date == today).ToList();
            var todayCompleted = todayStatuses.Count(es => es.Status == StatusType.Completed);
            var todayTotal = todayStatuses.Count;
            var completionRateToday = todayTotal > 0 ? (double)todayCompleted / todayTotal * 100 : 0;

            // Estatísticas da semana
            var weekStatuses = allExerciseStatuses.Where(es => es.Date.Date >= startOfWeek).ToList();
            var weekCompleted = weekStatuses.Count(es => es.Status == StatusType.Completed);
            var weekTotal = weekStatuses.Count;
            var completionRateThisWeek = weekTotal > 0 ? (double)weekCompleted / weekTotal * 100 : 0;

            // Estatísticas do mês
            var monthStatuses = allExerciseStatuses.Where(es => es.Date.Date >= startOfMonth).ToList();
            var monthCompleted = monthStatuses.Count(es => es.Status == StatusType.Completed);
            var monthTotal = monthStatuses.Count;
            var completionRateThisMonth = monthTotal > 0 ? (double)monthCompleted / monthTotal * 100 : 0;

            // Estatísticas do mês anterior para tendência
            var previousMonthStatuses = allExerciseStatuses
                .Where(es => es.Date.Date >= startOfPreviousMonth && es.Date.Date < startOfMonth).ToList();
            var previousMonthCompleted = previousMonthStatuses.Count(es => es.Status == StatusType.Completed);
            var previousMonthTotal = previousMonthStatuses.Count;
            var completionRatePreviousMonth = previousMonthTotal > 0 ? (double)previousMonthCompleted / previousMonthTotal * 100 : 0;

            // Taxa de conclusão geral (média)
            var allStatuses = allExerciseStatuses.ToList();
            var allCompleted = allStatuses.Count(es => es.Status == StatusType.Completed);
            var allTotal = allStatuses.Count;
            var averageCompletionRate = allTotal > 0 ? (double)allCompleted / allTotal * 100 : 0;

            // CALCULAR TENDÊNCIAS
            var studentsTrend = new StatisticTrendDto
            {
                Value = newStudentsThisMonth - newStudentsPreviousMonth,
                IsPositive = newStudentsThisMonth >= newStudentsPreviousMonth
            };

            var completionRateTrend = new StatisticTrendDto
            {
                Value = Math.Round(completionRateThisMonth - completionRatePreviousMonth, 2),
                IsPositive = completionRateThisMonth >= completionRatePreviousMonth
            };

            var workoutsCreatedTrend = new StatisticTrendDto
            {
                Value = workoutsCreatedThisMonth - workoutsCreatedPreviousMonth,
                IsPositive = workoutsCreatedThisMonth >= workoutsCreatedPreviousMonth
            };

            return new InstructorStatisticsDto
            {
                WorkoutsCreatedThisMonth = workoutsCreatedThisMonth,
                WorkoutsCreatedThisWeek = workoutsCreatedThisWeek,
                WorkoutsCreatedTotal = workoutsCreatedTotal,
                TotalStudents = totalStudents,
                ActiveStudents = activeStudents,
                NewStudentsThisWeek = newStudentsThisWeek,
                NewStudentsThisMonth = newStudentsThisMonth,
                AverageCompletionRate = Math.Round(averageCompletionRate, 2),
                CompletionRateToday = Math.Round(completionRateToday, 2),
                CompletionRateThisWeek = Math.Round(completionRateThisWeek, 2),
                CompletionRateThisMonth = Math.Round(completionRateThisMonth, 2),
                TotalWorkoutsCompletedThisMonth = monthCompleted,
                TotalExercisesCompletedThisMonth = monthCompleted,
                StudentsTrend = studentsTrend,
                CompletionRateTrend = completionRateTrend,
                WorkoutsCreatedTrend = workoutsCreatedTrend
            };
        }

        public async Task<IEnumerable<InvitationDto>> GetInvitationsAsync(int instructorId)
        {
            try
            {
                // Buscar todos os relacionamentos do instrutor (todos os status)
                var invitations = await _studentInstructorRepository.FindAsync(si => si.InstructorId == instructorId);

                var invitationDtos = new List<InvitationDto>();

                foreach (var invitation in invitations)
                {
                    // Buscar informações do aluno
                    var student = await _userRepository.GetByIdAsync(invitation.StudentId);

                    if (student != null)
                    {
                        invitationDtos.Add(new InvitationDto
                        {
                            Id = invitation.Id,
                            Student = new StudentInfoDto
                            {
                                Id = student.Id,
                                Name = student.Name,
                                Email = student.Email,
                                Age = student.Age,
                                Gender = (int?)student.Gender
                            },
                            Status = (int)invitation.Status,
                            StatusName = invitation.Status.ToString(),
                            InvitedAt = invitation.InvitedAt,
                            RespondedAt = invitation.RespondedAt,
                            ConnectedAt = invitation.ConnectedAt
                        });
                    }
                }

                // Ordenar por data de convite (mais recente primeiro)
                return invitationDtos.OrderByDescending(i => i.InvitedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invitations for instructor {InstructorId}", instructorId);
                return new List<InvitationDto>();
            }
        }
    }
}