using System.Collections;
using System.Collections.Generic;

namespace InstaBotPrototype.Services.Instagram
{
    public interface IInstagramService
    {
        int Login(string username, string password);
        IEnumerable<string> GetLatestPosts();
    }
}