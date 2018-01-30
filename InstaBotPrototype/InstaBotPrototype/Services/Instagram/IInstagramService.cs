using System.Collections;
using System.Collections.Generic;
using static InstaBotPrototype.Services.Instagram.InstagramService;


namespace InstaBotPrototype.Services.Instagram
{
    public interface IInstagramService
    {
        int Login(string username, string password);
        IEnumerable<ImageData> GetLatestPosts(string username);
    }
}