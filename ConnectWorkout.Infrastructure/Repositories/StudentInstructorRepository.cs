using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConnectWorkout.Core.Enums;
using ConnectWorkout.Core.Interfaces;
using ConnectWorkout.Core.Models;
using ConnectWorkout.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConnectWorkout.Infrastructure.Repositories
{
    /// <summary>
    /// Implementação do repositório de relações aluno-instrutor
    /// </summary>
    public class StudentInstructorRepository : Repository<StudentInstructor>, IStudentInstructorRepository
    {
        public StudentInstructorRepository(ApplicationDbContext context) : base(context)
        {
        }

        [System.Obsolete("Use AcceptedConnectionExistsAsync, PendingInvitationExistsAsync, or AnyRelationshipExistsAsync instead")]
        public async Task<bool> ConnectionExistsAsync(int studentId, int instructorId)
        {
            // Kept for backward compatibility, but marked as obsolete
            return await AnyRelationshipExistsAsync(studentId, instructorId);
        }

        public async Task<bool> AcceptedConnectionExistsAsync(int studentId, int instructorId)
        {
            return await _dbSet.AnyAsync(si =>
                si.StudentId == studentId &&
                si.InstructorId == instructorId &&
                si.Status == InvitationStatus.Accepted);
        }

        public async Task<bool> PendingInvitationExistsAsync(int studentId, int instructorId)
        {
            return await _dbSet.AnyAsync(si =>
                si.StudentId == studentId &&
                si.InstructorId == instructorId &&
                si.Status == InvitationStatus.Pending);
        }

        public async Task<bool> AnyRelationshipExistsAsync(int studentId, int instructorId)
        {
            return await _dbSet.AnyAsync(si =>
                si.StudentId == studentId &&
                si.InstructorId == instructorId);
        }

        public async Task<StudentInstructor> GetConnectionAsync(int studentId, int instructorId)
        {
            return await _dbSet.FirstOrDefaultAsync(si => 
                si.StudentId == studentId && 
                si.InstructorId == instructorId);
        }

        public async Task RemoveConnectionAsync(int studentId, int instructorId)
        {
            var connection = await GetConnectionAsync(studentId, instructorId);

            if (connection != null)
            {
                _dbSet.Remove(connection);
                await SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<StudentInstructor>> GetPendingInvitationsForStudentAsync(int studentId)
        {
            return await _dbSet
                .Include(si => si.Instructor)
                    .ThenInclude(i => i.StudentsRelations)
                .Where(si => si.StudentId == studentId && si.Status == InvitationStatus.Pending)
                .OrderByDescending(si => si.InvitedAt)
                .ToListAsync();
        }

        public async Task<StudentInstructor> GetInvitationByIdAsync(int invitationId)
        {
            return await _dbSet
                .Include(si => si.Instructor)
                .Include(si => si.Student)
                .FirstOrDefaultAsync(si => si.Id == invitationId);
        }

        public async Task<bool> AcceptInvitationAsync(int invitationId, int studentId)
        {
            var invitation = await _dbSet.FirstOrDefaultAsync(si =>
                si.Id == invitationId &&
                si.StudentId == studentId &&
                si.Status == InvitationStatus.Pending);

            if (invitation == null)
            {
                return false;
            }

            invitation.Status = InvitationStatus.Accepted;
            invitation.RespondedAt = DateTime.UtcNow;
            invitation.ConnectedAt = DateTime.UtcNow;

            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectInvitationAsync(int invitationId, int studentId)
        {
            var invitation = await _dbSet.FirstOrDefaultAsync(si =>
                si.Id == invitationId &&
                si.StudentId == studentId &&
                si.Status == InvitationStatus.Pending);

            if (invitation == null)
            {
                return false;
            }

            invitation.Status = InvitationStatus.Rejected;
            invitation.RespondedAt = DateTime.UtcNow;

            await SaveChangesAsync();
            return true;
        }
    }
}