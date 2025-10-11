using ConnectWorkout.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ConnectWorkout.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Conjuntos de entidades - representam tabelas no banco de dados
        public DbSet<User> Users { get; set; }
        public DbSet<StudentInstructor> StudentInstructors { get; set; }
        public DbSet<Workout> Workouts { get; set; }
        public DbSet<WorkoutDay> WorkoutDays { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<ExerciseStatus> ExerciseStatuses { get; set; }

        /// <summary>
        /// Configura o modelo de entidades e seus relacionamentos
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração para User
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                
                entity.HasKey(u => u.Id);
                
                entity.Property(u => u.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(u => u.Email)
                    .IsUnique();
                
                entity.Property(u => u.PasswordHash)
                    .IsRequired();
                
                entity.Property(u => u.UserType)
                    .IsRequired();
            });

            // Configuração para StudentInstructor
            modelBuilder.Entity<StudentInstructor>(entity =>
            {
                entity.ToTable("StudentInstructors");
                
                entity.HasKey(si => si.Id);
                
                // Configurando relação com Student
                entity.HasOne(si => si.Student)
                    .WithMany(u => u.InstructorsRelations)
                    .HasForeignKey(si => si.StudentId)
                    .OnDelete(DeleteBehavior.Restrict); // Evita exclusão em cascata
                
                // Configurando relação com Instructor
                entity.HasOne(si => si.Instructor)
                    .WithMany(u => u.StudentsRelations)
                    .HasForeignKey(si => si.InstructorId)
                    .OnDelete(DeleteBehavior.Restrict); // Evita exclusão em cascata
            });

            // Configuração para Workout
            modelBuilder.Entity<Workout>(entity =>
            {
                entity.ToTable("Workouts");
                
                entity.HasKey(w => w.Id);
                
                entity.Property(w => w.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.Property(w => w.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");
                
                entity.Property(w => w.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);
                
                // Configurando relação com User (Student)
                entity.HasOne(w => w.Student)
                    .WithMany(u => u.Workouts)
                    .HasForeignKey(w => w.StudentId)
                    .OnDelete(DeleteBehavior.Cascade); // Exclusão em cascata
            });

            // Configuração para WorkoutDay
            modelBuilder.Entity<WorkoutDay>(entity =>
            {
                entity.ToTable("WorkoutDays");
                
                entity.HasKey(wd => wd.Id);
                
                entity.Property(wd => wd.DayOfWeek)
                    .IsRequired();
                
                // Configurando relação com Workout
                entity.HasOne(wd => wd.Workout)
                    .WithMany(w => w.WorkoutDays)
                    .HasForeignKey(wd => wd.WorkoutId)
                    .OnDelete(DeleteBehavior.Cascade); // Exclusão em cascata
            });

            // Configuração para Exercise
            modelBuilder.Entity<Exercise>(entity =>
            {
                entity.ToTable("Exercises");
                
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.ExerciseDbId)
                    .IsRequired()
                    .HasMaxLength(50);
                
                entity.Property(e => e.Sets)
                    .IsRequired()
                    .HasMaxLength(20);
                
                entity.Property(e => e.Repetitions)
                    .IsRequired()
                    .HasMaxLength(20);
                
                // Configurando relação com WorkoutDay
                entity.HasOne(e => e.WorkoutDay)
                    .WithMany(wd => wd.Exercises)
                    .HasForeignKey(e => e.WorkoutDayId)
                    .OnDelete(DeleteBehavior.Cascade); // Exclusão em cascata
            });

            // Configuração para ExerciseStatus
            modelBuilder.Entity<ExerciseStatus>(entity =>
            {
                entity.ToTable("ExerciseStatuses");
                
                entity.HasKey(es => es.Id);
                
                entity.Property(es => es.Date)
                    .IsRequired()
                    .HasDefaultValueSql("CONVERT(date, GETDATE())");
                
                entity.Property(es => es.Status)
                    .IsRequired();
                
                // Configurando relação com Exercise
                entity.HasOne(es => es.Exercise)
                    .WithMany(e => e.Statuses)
                    .HasForeignKey(es => es.ExerciseId)
                    .OnDelete(DeleteBehavior.Cascade); // Exclusão em cascata
                
                // Configurando relação com User (Student)
                entity.HasOne(es => es.Student)
                    .WithMany(u => u.ExerciseStatuses)
                    .HasForeignKey(es => es.StudentId)
                    .OnDelete(DeleteBehavior.Restrict); // Evita exclusão em cascata
                
                // Índice composto para evitar duplicações
                entity.HasIndex(es => new { es.ExerciseId, es.StudentId, es.Date })
                    .IsUnique();
            });
        }
    }
}