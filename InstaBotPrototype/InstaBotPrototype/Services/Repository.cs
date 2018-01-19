using InstaBotPrototype.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace InstaBotPrototype.Services
{
    public class Repository : IRepository
    {
        DbProviderFactory factory = DbProviderFactories.GetFactory(ConfigurationManager.ConnectionStrings[1].ProviderName);
        DbConnection connection;

        public Repository()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[1].ConnectionString;
            connection = factory.CreateConnection();
            connection.ConnectionString = connectionString;
            connection.Open();
        }

        public IEnumerable<TopicModel> GetTopicsByConfigId(int configId)
        {
            var text = "SELECT T.Topic FROM ConfigTopic CT  JOIN Topic T ON T.TopicID = CT.TopicID WHERE CT.ConfigID = @configId;";
            var p1 = CreateParameter("@configId", configId);

            var topics = new List<TopicModel>();

            using (var select = CreateCommand(text, p1))
            using (var reader = select.ExecuteReader())
            {
                while (reader.Read())
                {
                    topics.Add(new TopicModel { Topic = reader.GetString(0) });
                }
            }

            return topics;
        }

        public IEnumerable<TagModel> GetTagsByConfigId(int configId)
        {
            var text = "SELECT T.Tag FROM ConfigTag CT JOIN Tag T ON T.TagID = CT.TagID WHERE CT.ConfigID = @configID;";
            var p1 = CreateParameter("@configId", configId);

            var tags = new List<TagModel>();

            using (var select = CreateCommand(text, p1))
            using (var reader = select.ExecuteReader())
            {
                while (reader.Read())
                {
                    tags.Add(new TagModel { Tag = reader.GetString(0) });
                }
            }

            return tags;
        }

        public int AddTag(string tag)
        {
            var text = "INSERT INTO Tag (Tag) VALUES (@Tag);SELECT @TagID = SCOPE_IDENTITY()";
            var p1 = CreateParameter("@Tag", tag);
            var p2 = CreateParameter("@TagID", null, ParameterDirection.Output);

            using (var command = CreateCommand(text, p1, p2))
            {
                command.ExecuteNonQuery();
            }

            return (int)p2.Value;
        }

        public int AddTopic(string topic)
        {
            var text = "INSERT INTO Topic (Topic) VALUES (@Topic);SELECT @TopicID = SCOPE_IDENTITY()";
            var p1 = CreateParameter("@Topic", topic);
            var p2 = CreateParameter("@TopicID", null, ParameterDirection.Output);

            using (var command = CreateCommand(text, p1, p2))
            {
                command.ExecuteNonQuery();
            }

            return (int)p2.Value;
        }

        public int? GetTagId(string tag)
        {
            var text = "SELECT @TagID = TagID FROM Tag WHERE Tag = @Tag";
            var p1 = CreateParameter("@TagID", null, ParameterDirection.Output);
            var p2 = CreateParameter("@Tag", tag);

            using (var select = CreateCommand(text, p1, p2))
            {
                select.ExecuteNonQuery();
            }

            return (int)p2.Value;
        }

        public int? GetTopicId(string topic)
        {
            var text = "SELECT @TopicID = TopicID FROM Topic WHERE Topic = @Topic";
            var p1 = CreateParameter("@TopicID", null, ParameterDirection.Output);
            var p2 = CreateParameter("@Topic", topic);

            using (var select = CreateCommand(text, p1, p2))
            {
                select.ExecuteNonQuery();
            }

            return (int)p2.Value;
        }

        void AssignConfigTag(int tagId, int configId)
        {
            var text = "INSERT INTO ConfigTag (ConfigID, TagID) VALUES(@ConfigID, @TagID)";
            var p1 = CreateParameter("@ConfigID", configId);
            var p2 = CreateParameter("@TagID", tagId);

            using (var select = CreateCommand(text, p1, p2))
            {
                select.ExecuteNonQuery();
            }
        }

        void AssignConfigTopic(int topicId, int configId)
        {
            var text = "INSERT INTO ConfigTopic (ConfigID, TopicID) VALUES(@ConfigID, @TopicID)";
            var p1 = CreateParameter("@ConfigID", configId);
            var p2 = CreateParameter("@TopicID", topicId);

            using (var select = CreateCommand(text, p1, p2))
            {
                select.ExecuteNonQuery();
            }
        }

        public void AddTagsToConfigId(IEnumerable<TagModel> tags, int configId)
        {
            int? tagId;
            foreach (var tag in tags)
            {
                tagId = GetTagId(tag.Tag);
                if (tagId == null)
                {
                    tagId = AddTag(tag.Tag);
                }
                AssignConfigTag(tagId.Value, configId);
            }
        }

        private void AddTopicsToConfigId(IEnumerable<TopicModel> topics, int configId)
        {
            int? topicId;
            foreach (var topic in topics)
            {
                topicId = GetTopicId(topic.Topic);
                if (topicId == null)
                {
                    topicId = AddTopic(topic.Topic);
                }
                AssignConfigTopic(topicId.Value, configId);
            }
        }

        public int? GetUserId(string login, string password)
        {
            var text = $"SELECT Id FROM dbo.Users WHERE Login = @login and Password = @password";
            var p1 = CreateParameter("@login", login);
            var p2 = CreateParameter("@password", password);

            int? id = null;

            using (var select = CreateCommand(text, p1, p2))
            using (var reader = select.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    reader.Read();
                    id = reader.GetInt32(0);
                }
            }

            return id;
        }

        public int? GetTelegramVerificationKey(int userId)
        {
            var text = $"SELECT TelegramVerificationKey FROM dbo.TelegramVerification WHERE UserId = @id";
            var p1 = CreateParameter("@id", userId);

            int? key = null;

            using (var select = CreateCommand(text, p1))
            using (var reader = select.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    reader.Read();
                    key = reader.GetInt32(0);
                }
            }

            return key;
        }

        public void AddSession(int userId, Guid sessionId)
        {
            var text = $"INSERT INTO dbo.Sessions (UserId, SessionId) VALUES (@id, @sessionId)"; ;
            var p1 = CreateParameter("@id", userId);
            var p2 = CreateParameter("@sessionId", sessionId);

            using (var insert = CreateCommand(text, p1, p2))
            {
                insert.ExecuteNonQuery();
            }
        }

        public int? AddUser(string login, string email, string password)
        {
            var text = $"INSERT INTO dbo.Users (Login, Email, Password, RegisterDate) VALUES (@login, @email, @password, SYSDATETIME())";
            var p1 = CreateParameter("@login", login);
            var p2 = CreateParameter("@email", email);
            var p3 = CreateParameter("@password", password);

            using (var insert = CreateCommand(text, p1, p2))
            {
                insert.ExecuteNonQuery();
            }

            return GetUserId(login, password);
        }

        private DbCommand CreateCommand(string text, params DbParameter[] args)
        {
            var command = factory.CreateCommand();

            command.Connection = connection;
            command.CommandText = text;
            command.Parameters.AddRange(args);

            return command;
        }

        private DbParameter CreateParameter(string name, object value, ParameterDirection direction = ParameterDirection.Input)
        {
            var parameter = factory.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            parameter.Direction = direction;
            return parameter;
        }

        private bool isDisposed = false;

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            connection.Close();
            GC.SuppressFinalize(this);
        }

        ~Repository()
        {
            Dispose();
        }
    }
}