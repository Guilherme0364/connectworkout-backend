namespace ConnectWorkout.Domain.Repositories.Member
{
    // Apenas métodos de escrita
    public interface IMemberWriteOnlyRepository
    {
        public Task Add(Entities.Member member);
    }
}
