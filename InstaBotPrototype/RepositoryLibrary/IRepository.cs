namespace RepositoryLibrary
{
    public interface IRepository<TKey, TValue>
    {
        void Add(TValue model);
        TValue Get(TKey id);
        void Update(TValue model);
        void Delete(TKey id);
    }
}