namespace ConnectWorkout.Domain.Repositories.Coach
{
    public interface ICoachWriteOnlyRepository
    {
        public Task Add(Entities.Coach coach);
    }
}
