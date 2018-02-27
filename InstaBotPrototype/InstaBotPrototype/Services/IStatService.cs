namespace InstaBotPrototype.Services
{
    public interface IStatService
    {
        int CountTags(int userId);
        int CountTopics(int userId);
        int CountPhotos(int userId);
    }
}
