namespace ConnectWorkout.Domain.Repositories.Coach
{
    public interface ICoachReadOnlyRepository
    {
        public Task<bool> ExistsCoachWithActiveEmail(string email);
        public Task<Entities.Coach> GetCoachByEmailAndPassword(string email, string password);
    }
}
