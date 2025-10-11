using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConnectWorkout.Core.Interfaces;
using ConnectWorkout.Core.Models;
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
                .Where(si => si.InstructorId == instructorId)
                .Include(si => si.Student)
                .Select(si => si.Student)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetInstructorsByStudentIdAsync(int studentId)
        {
            return await _context.StudentInstructors
                .Where(si => si.StudentId == studentId)
                .Include(si => si.Instructor)
                .Select(si => si.Instructor)
                .ToListAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }
    }
}