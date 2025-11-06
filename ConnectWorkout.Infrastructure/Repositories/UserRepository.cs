using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConnectWorkout.Core.Interfaces;
using ConnectWorkout.Core.Models;
using ConnectWorkout.Core.Enums;
using ConnectWorkout.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConnectWorkout.Infrastructure.Repositories
{
    /// <summary>
    /// Implementação do repositório de usuários
    /// </summary>
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<IEnumerable<User>> GetStudentsByInstructorIdAsync(int instructorId)
        {
            return await _context.StudentInstructors
                .Where(si => si.InstructorId == instructorId && si.Status == InvitationStatus.Accepted)
                .Include(si => si.Student)
                .Select(si => si.Student)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetInstructorsByStudentIdAsync(int studentId)
        {
            return await _context.StudentInstructors
                .Where(si => si.StudentId == studentId && si.Status == InvitationStatus.Accepted)
                .Include(si => si.Instructor)
                .Select(si => si.Instructor)
                .ToListAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task DeleteUserAsync(int userId)
        {
            // 1. Delete all StudentInstructor relationships where user is either student or instructor
            var studentRelations = await _context.StudentInstructors
                .Where(si => si.StudentId == userId)
                .ToListAsync();

            var instructorRelations = await _context.StudentInstructors
                .Where(si => si.InstructorId == userId)
                .ToListAsync();

            _context.StudentInstructors.RemoveRange(studentRelations);
            _context.StudentInstructors.RemoveRange(instructorRelations);

            // 2. Delete all ExerciseStatuses for this user (student)
            var exerciseStatuses = await _context.ExerciseStatuses
                .Where(es => es.StudentId == userId)
                .ToListAsync();

            _context.ExerciseStatuses.RemoveRange(exerciseStatuses);

            // 3. Delete all Workouts for this user (student) - cascade will handle WorkoutDays and Exercises
            var workouts = await _context.Workouts
                .Where(w => w.StudentId == userId)
                .ToListAsync();

            _context.Workouts.RemoveRange(workouts);

            // 4. Finally, delete the user
            var user = await _dbSet.FindAsync(userId);
            if (user != null)
            {
                _dbSet.Remove(user);
            }

            await SaveChangesAsync();
        }
    }
}