using InstaBotPrototype.Models;
using System.Collections.Generic;

namespace InstaBotPrototype.Services.DB
{
    public interface IConfigService
    {
        int AddConfig(int userId);
        bool IsLoggedIn(string sessionID);
        void SaveInstagramToken(long id, string nickname, string tokenStr, string sessionId);
        bool IsUserVerifiedInInstagram(string sessionId);
        int? GetVerifyKey(string sessionId);
        int? GetUserIdBySession(string sessionId);
        IEnumerable<string> GetUserTopics(int userId);
        IEnumerable<string> GetUserTags(int userId);
        void AddTag(string item, int configId);
        void AddTopic(string item, int configId);
        int? GetLatestConfig(int userId);
        void DeleteTopic(string item, int configId);
        void DeleteTag(string item, int configId);
    }
}