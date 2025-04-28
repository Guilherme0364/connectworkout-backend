namespace ConnectWorkout.Domain
{
    public interface IUnityOfWork
    {
        public Task Commit();
    }
}
