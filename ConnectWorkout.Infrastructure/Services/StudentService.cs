using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConnectWorkout.Core.DTOs;
using ConnectWorkout.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ConnectWorkout.Infrastructure.Services
{
    /// <summary>
    /// Serviço para operações relacionadas a alunos
    /// </summary>
    public class StudentService : IStudentService
    {
        private readonly IStudentInstructorRepository _studentInstructorRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<StudentService> _logger;

        public StudentService(
            IStudentInstructorRepository studentInstructorRepository,
            IUserRepository userRepository,
            ILogger<StudentService> logger)
        {
            _studentInstructorRepository = studentInstructorRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<InstructorSummaryDto>> GetInstructorsAsync(int studentId)
        {
            // Obter todos os instrutores do aluno (convites aceitos)
            var instructors = await _userRepository.GetInstructorsByStudentIdAsync(studentId);

            return instructors.Select(instructor => new InstructorSummaryDto
            {
                Id = instructor.Id,
                Name = instructor.Name,
                Email = instructor.Email,
                Description = instructor.Description ?? string.Empty,
                StudentCount = instructor.StudentsRelations?.Count(sr => sr.Status == Core.Enums.InvitationStatus.Accepted) ?? 0
            });
        }

        public async Task<InstructorSummaryDto> GetInstructorDetailsAsync(int studentId, int instructorId)
        {
            // Verificar se existe conexão aceita entre aluno e instrutor
            var connection = await _studentInstructorRepository.GetConnectionAsync(studentId, instructorId);

            if (connection == null || connection.Status != Core.Enums.InvitationStatus.Accepted)
            {
                _logger.LogWarning(
                    "No accepted connection between student {StudentId} and instructor {InstructorId}",
                    studentId, instructorId);
                return null;
            }

            var instructor = await _userRepository.GetByIdAsync(instructorId);

            if (instructor == null)
            {
                _logger.LogWarning("Instructor with ID {InstructorId} not found", instructorId);
                return null;
            }

            return new InstructorSummaryDto
            {
                Id = instructor.Id,
                Name = instructor.Name,
                Email = instructor.Email,
                Description = instructor.Description ?? string.Empty,
                StudentCount = instructor.StudentsRelations?.Count(sr => sr.Status == Core.Enums.InvitationStatus.Accepted) ?? 0
            };
        }

        public async Task<IEnumerable<PendingInvitationDto>> GetPendingInvitationsAsync(int studentId)
        {
            var pendingInvitations = await _studentInstructorRepository.GetPendingInvitationsForStudentAsync(studentId);

            return pendingInvitations.Select(invitation => new PendingInvitationDto
            {
                InvitationId = invitation.Id,
                InstructorId = invitation.InstructorId,
                InstructorName = invitation.Instructor?.Name ?? "Desconhecido",
                InstructorEmail = invitation.Instructor?.Email ?? string.Empty,
                InstructorDescription = invitation.Instructor?.Description ?? string.Empty,
                InstructorStudentCount = invitation.Instructor?.StudentsRelations
                    ?.Count(sr => sr.Status == Core.Enums.InvitationStatus.Accepted) ?? 0,
                InvitedAt = invitation.InvitedAt
            });
        }

        public async Task<bool> AcceptInvitationAsync(int studentId, int invitationId)
        {
            _logger.LogInformation(
                "Student {StudentId} attempting to accept invitation {InvitationId}",
                studentId, invitationId);

            var result = await _studentInstructorRepository.AcceptInvitationAsync(invitationId, studentId);

            if (result)
            {
                _logger.LogInformation(
                    "Student {StudentId} successfully accepted invitation {InvitationId}",
                    studentId, invitationId);
            }
            else
            {
                _logger.LogWarning(
                    "Student {StudentId} failed to accept invitation {InvitationId}",
                    studentId, invitationId);
            }

            return result;
        }

        public async Task<bool> RejectInvitationAsync(int studentId, int invitationId)
        {
            _logger.LogInformation(
                "Student {StudentId} attempting to reject invitation {InvitationId}",
                studentId, invitationId);

            var result = await _studentInstructorRepository.RejectInvitationAsync(invitationId, studentId);

            if (result)
            {
                _logger.LogInformation(
                    "Student {StudentId} successfully rejected invitation {InvitationId}",
                    studentId, invitationId);
            }
            else
            {
                _logger.LogWarning(
                    "Student {StudentId} failed to reject invitation {InvitationId}",
                    studentId, invitationId);
            }

            return result;
        }

        public async Task<int> GetPendingInvitationsCountAsync(int studentId)
        {
            var pendingInvitations = await _studentInstructorRepository.GetPendingInvitationsForStudentAsync(studentId);
            return pendingInvitations.Count();
        }
    }
}
