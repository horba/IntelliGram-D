using InstaBotPrototype.Models;
using System;

namespace RepositoryLibrary
{
    class UsersRepository : IRepository<UserModel>
    {
        public void Add(UserModel model) => throw new NotImplementedException();
        public void Delete(int id) => throw new NotImplementedException();
        public UserModel Get(int id) => throw new NotImplementedException();
        public void Update(UserModel model) => throw new NotImplementedException();
    }
}