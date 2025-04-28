namespace ConnectWorkout.Domain.Entities
{
    public class Member : EntityBase
    {        
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public int Weight { get; set; }

    }
}
