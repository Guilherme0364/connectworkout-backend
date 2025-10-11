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
    /// Implementação do repositório de status de exercícios
    /// </summary>
    public class ExerciseStatusRepository : Repository<ExerciseStatus>, IExerciseStatusRepository
    {
        public ExerciseStatusRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ExerciseStatus> GetStatusForExerciseAndDateAsync(int exerciseId, int studentId, DateTime date)
        {
            return await _dbSet
                .FirstOrDefaultAsync(es => 
                    es.ExerciseId == exerciseId && 
                    es.StudentId == studentId && 
                    es.Date.Date == date.Date);
        }

        public async Task<IEnumerable<ExerciseStatus>> GetStatusesByStudentAndDateAsync(int studentId, DateTime date)
        {
            return await _dbSet
                .Include(es => es.Exercise)
                .Where(es => 
                    es.StudentId == studentId && 
                    es.Date.Date == date.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<ExerciseStatus>> GetStatusesByDateRangeAsync(int studentId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(es => es.Exercise)
                .Where(es => 
                    es.StudentId == studentId && 
                    es.Date.Date >= startDate.Date && 
                    es.Date.Date <= endDate.Date)
                .OrderBy(es => es.Date)
                .ToListAsync();
        }

        public async Task<bool> HasStatusForExerciseAsync(int exerciseId, int studentId, DateTime date)
        {
            return await _dbSet.AnyAsync(es => 
                es.ExerciseId == exerciseId && 
                es.StudentId == studentId && 
                es.Date.Date == date.Date);
        }

        public async Task<int> CountStatusesByTypeAsync(int studentId, StatusType status, DateTime date)
        {
            return await _dbSet.CountAsync(es => 
                es.StudentId == studentId && 
                es.Status == status && 
                es.Date.Date == date.Date);
        }
    }
}