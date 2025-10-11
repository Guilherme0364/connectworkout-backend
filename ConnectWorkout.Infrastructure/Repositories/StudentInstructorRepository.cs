using System.Threading.Tasks;
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

        public async Task<bool> ConnectionExistsAsync(int studentId, int instructorId)
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
    }
}