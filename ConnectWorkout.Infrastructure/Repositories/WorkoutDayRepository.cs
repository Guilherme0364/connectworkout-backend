using System;
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
    /// Implementação do repositório de dias de treino
    /// </summary>
    public class WorkoutDayRepository : Repository<WorkoutDay>, IWorkoutDayRepository
    {
        public WorkoutDayRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<WorkoutDay>> GetWorkoutDaysByWorkoutIdAsync(int workoutId)
        {
            return await _dbSet
                .Where(wd => wd.WorkoutId == workoutId)
                .ToListAsync();
        }

        public async Task<WorkoutDay> GetWorkoutDayWithExercisesAsync(int workoutDayId)
        {
            return await _dbSet
                .Include(wd => wd.Exercises)
                .FirstOrDefaultAsync(wd => wd.Id == workoutDayId);
        }

        public async Task<WorkoutDay> GetWorkoutDayByDayOfWeekAsync(int workoutId, DayOfWeek dayOfWeek)
        {
            return await _dbSet
                .Include(wd => wd.Exercises)
                .FirstOrDefaultAsync(wd => 
                    wd.WorkoutId == workoutId && 
                    wd.DayOfWeek == dayOfWeek);
        }
    }
}