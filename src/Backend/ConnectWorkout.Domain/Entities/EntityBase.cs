namespace ConnectWorkout.Domain.Entities
{
    public class EntityBase
    {
        public Guid Id { get; set; }
        public bool Active { get; set; } = true;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
