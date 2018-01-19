using System;
using System.Collections.Generic;
using InstaBotPrototype.Models;

namespace InstaBotPrototype.Services
{
    public interface IRepository
    {
        void AddSession(int userId, Guid sessionId);
        int AddTag(string tag);
        void AddTagsToConfigId(IEnumerable<TagModel> tags, int configId);
        int AddTopic(string topic);
        void AddUser(string login, string email, string password);
        void Dispose();
        int? GetTagId(string tag);
        IEnumerable<TagModel> GetTagsByConfigId(int configId);
        int? GetTelegramVerificationKey(int userId);
        int? GetTopicId(string topic);
        IEnumerable<TopicModel> GetTopicsByConfigId(int configId);
        int? GetUserId(string login, string password);
    }
}