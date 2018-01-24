namespace RepositoryLibrary
{
    interface IRepository<T> where T : new()
    {
        void Add(T model);
        T Get(int id);
        void Update(T model);
        void Delete(int id);
    }
}