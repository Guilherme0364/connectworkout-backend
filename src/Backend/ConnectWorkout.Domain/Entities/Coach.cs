namespace ConnectWorkout.Domain.Entities
{
    public class Coach : EntityBase
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password {  get; set; } = string.Empty;
    }
}
