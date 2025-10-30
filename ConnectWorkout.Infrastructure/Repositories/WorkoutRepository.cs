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
    /// Implementação do repositório de fichas de treino
    /// </summary>
    public class WorkoutRepository : Repository<Workout>, IWorkoutRepository
    {
        public WorkoutRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Workout>> GetWorkoutsByStudentIdAsync(int studentId)
        {
            return await _dbSet
                .Include(w => w.WorkoutDays)
                    .ThenInclude(wd => wd.Exercises)
                .Where(w => w.StudentId == studentId)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();
        }

        public async Task<Workout> GetWorkoutWithDaysAndExercisesAsync(int workoutId)
        {
            return await _dbSet
                .Include(w => w.WorkoutDays)
                    .ThenInclude(wd => wd.Exercises)
                .FirstOrDefaultAsync(w => w.Id == workoutId);
        }

        public async Task<Workout> GetActiveWorkoutForStudentAsync(int studentId)
        {
            return await _dbSet
                .Include(w => w.WorkoutDays)
                    .ThenInclude(wd => wd.Exercises)
                .Where(w => w.StudentId == studentId && w.IsActive)
                .OrderByDescending(w => w.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}