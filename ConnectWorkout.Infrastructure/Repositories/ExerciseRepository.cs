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
    /// Implementação do repositório de exercícios
    /// </summary>
    public class ExerciseRepository : Repository<Exercise>, IExerciseRepository
    {
        public ExerciseRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Exercise>> GetExercisesByWorkoutDayIdAsync(int workoutDayId)
        {
            return await _dbSet
                .Where(e => e.WorkoutDayId == workoutDayId)
                .OrderBy(e => e.Order)
                .ToListAsync();
        }

        public async Task<Exercise> GetExerciseWithStatusesAsync(int exerciseId)
        {
            return await _dbSet
                .Include(e => e.Statuses)
                .FirstOrDefaultAsync(e => e.Id == exerciseId);
        }

        public async Task<IEnumerable<Exercise>> GetExercisesForStudentTodayAsync(int studentId)
        {
            // Obter o dia da semana atual
            DayOfWeek today = DateTime.Now.DayOfWeek;

            // Buscar os exercícios do treino ativo do aluno para o dia de hoje
            return await _context.Exercises
                .Include(e => e.WorkoutDay)
                    .ThenInclude(wd => wd.Workout)
                .Where(e =>
                    e.WorkoutDay.Workout.StudentId == studentId &&
                    e.WorkoutDay.Workout.IsActive &&
                    e.WorkoutDay.DayOfWeek == today)
                .ToListAsync();
        }

        public async Task ReorderExercisesAsync(List<int> exerciseIds)
        {
            for (int i = 0; i < exerciseIds.Count; i++)
            {
                var exercise = await _dbSet.FindAsync(exerciseIds[i]);
                if (exercise != null)
                {
                    exercise.Order = i;
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}