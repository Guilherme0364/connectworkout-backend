namespace ConnectWorkout.Domain.Repositories.Member
{
    public interface IMemberReadOnlyRepository
    {
        public Task<bool> ExistsActiveMemberWithEmail(string email);
        public Task<Entities.Member> GetMemberByEmailAndPassword(string email, string password);
    }
}
