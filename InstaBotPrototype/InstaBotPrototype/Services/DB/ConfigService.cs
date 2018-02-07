using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Common;
using InstaBotPrototype.Models;

namespace InstaBotPrototype.Services.DB
{
    public class ConfigService : IConfigService
    {
        private string connectionString;
        private DbProviderFactory factory = DbProviderFactories.GetFactory(ConfigurationManager.ConnectionStrings[1].ProviderName);
        private readonly char[] trimChar = { ' ', '\n', '\t' };
        private const int fieldLength = 128;

        public ConfigService()
        {
            connectionString = ConfigurationManager.ConnectionStrings[1].ConnectionString;
        }

        public ConfigService(string connectionStr)
        {
            connectionString = connectionStr;
        }

        #region IConfigService implementation

        public ConfigurationModel GetConfig()
        {
            return new ConfigurationModel
            {
                Tags = new TagModel[] { new TagModel { Tag = "tag1" }, new TagModel { Tag = "tag2" } },
                Topics = new TopicModel[] { new TopicModel { Topic = "topic1" }, new TopicModel { Topic = "topic2" } }
            };
        }

        public ConfigurationModel GetConfig(int? id)
        {
            if (id != null && id > 0)
            {
                var configModel = new ConfigurationModel();
                using (var connection =
                    new SqlConnection(connectionString))
                {
                    try
                    {
                        configModel.Tags = GetTagsByConfigId(id, connection);
                        configModel.Topics = GetTopicsByConfigId(id, connection);
                        return configModel;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            else
            {
                throw new Exception("Id must be a positive not null int number");
            }
            return null;
        }

        public void SaveConfig(ConfigurationModel model, String sessionId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string addConfigQuery =
                       "INSERT INTO Configuration (UserId) " +
                       "VALUES (@UserId); " +
                       "SELECT @ConfigID = SCOPE_IDENTITY();";
                    SqlCommand addConfigCmd = new SqlCommand(addConfigQuery, connection);
                    addConfigCmd.Parameters.Add("@UserId", SqlDbType.Int).Value = GetUserIdBySession(sessionId);
                    SqlParameter configID = new SqlParameter()
                    {
                        ParameterName = "@ConfigID",
                        SqlDbType = SqlDbType.Int,
                        Direction = ParameterDirection.Output
                    };
                    addConfigCmd.Parameters.Add(configID);
                    addConfigCmd.ExecuteNonQuery();

                    model.ConfigId = (int)configID.Value;

                    TrimTagsTopics(model);

                    AddTopicsToConfig(model.Topics, (int)model.ConfigId, connection);
                    AddTagsToConfig(model.Tags, (int)model.ConfigId, connection);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public bool IsLoggedIn(String sessionID)
        {
            return GetUserIdBySession(sessionID).HasValue;
        }
        public int? GetUserIdBySession(string sessionId)
        {
            int? userId = null;
            using (var dbConnection = factory.CreateConnection())
            {
                if (sessionId != null)
                {
                    dbConnection.ConnectionString = connectionString;
                    var param = factory.CreateParameter();
                    param.ParameterName = "@Id";
                    param.Value = sessionId;
                    var check = factory.CreateCommand();
                    check.CommandText = "SELECT UserId FROM dbo.Sessions WHERE SessionId = @Id";
                    check.Parameters.Add(param);
                    check.Connection = dbConnection;
                    dbConnection.Open();
                    var reader = check.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        userId = reader.GetInt32(0);
                    }
                    reader.Close();
                }
            }
            return userId;
        }
        public int? GetVerifyKey(string sessionId)
        {
            int? verifyKey = null;
            using (var dbConnection = factory.CreateConnection())
            {
                dbConnection.ConnectionString = connectionString;
                dbConnection.Open();
                var id = GetUserIdBySession(sessionId);
                var selectTelegram = factory.CreateCommand();
                selectTelegram.Connection = dbConnection;
                selectTelegram.CommandText = $"SELECT TelegramVerificationKey FROM dbo.TelegramVerification WHERE UserId = @id";
                selectTelegram.Parameters.Add(new SqlParameter
                {
                    ParameterName = "@id",
                    SqlDbType = SqlDbType.Int,
                    Value = id
                });
                var readerKey = selectTelegram.ExecuteReader();
                if (readerKey.HasRows)
                {
                    readerKey.Read();
                    verifyKey = readerKey.GetInt32(0);
                }
                readerKey.Close();
            }
            return verifyKey;
        }
        public void SaveInstagramToken(long id, string nickname, string tokenStr, string sessionId)
        {
            using (var dbConnection = factory.CreateConnection())
            {
                dbConnection.ConnectionString = connectionString;
                dbConnection.Open();
                var insertCmd = factory.CreateCommand();
                insertCmd.Connection = dbConnection;
                insertCmd.CommandText = "INSERT INTO InstagramIntegration VALUES (@UserId,@InstaId,@Nick,@Token)";
                var userIdParam = factory.CreateParameter();
                userIdParam.ParameterName = "@InstaId";
                userIdParam.Value = id;
                var nickParam = factory.CreateParameter();
                nickParam.ParameterName = "@Nick";
                nickParam.Value = nickname;
                var tokenParam = factory.CreateParameter();
                tokenParam.ParameterName = "@Token";
                tokenParam.Value = tokenStr;
                var userid = GetUserIdBySession(sessionId);
                var idParam = factory.CreateParameter();
                idParam.ParameterName = "@UserId";
                idParam.Value = userid.Value;
                insertCmd.Parameters.AddRange(new[] { idParam, userIdParam, nickParam, tokenParam });
                insertCmd.ExecuteNonQuery();
            }
        }
        public bool IsUserVerifiedInInstagram(string sessionId)
        {
            using (var dbConnection = factory.CreateConnection())
            {
                var id = GetUserIdBySession(sessionId);
                dbConnection.ConnectionString = connectionString;
                dbConnection.Open();
                var insertCmd = factory.CreateCommand();
                insertCmd.Connection = dbConnection;
                insertCmd.CommandText = "SELECT COUNT (*) FROM InstagramIntegration WHERE UserId = @Id";
                var userIdParam = factory.CreateParameter();
                userIdParam.ParameterName = "@Id";
                userIdParam.Value = id;
                insertCmd.Parameters.AddRange(new[] { userIdParam });
                return Convert.ToInt32(insertCmd.ExecuteScalar()) == 1;
            }
        }
        #endregion

        #region Private helper methods

        private void TrimTagsTopics(ConfigurationModel model)
        {
            foreach (var tag in model.Tags)
            {
                tag.Tag = tag.Tag.Trim();
            }

            foreach (var topic in model.Topics)
            {
                topic.Topic = topic.Topic.Trim();
            }
        }


        private IEnumerable<TopicModel> GetTopicsByConfigId(int? configId, SqlConnection connection)
        {
            if (configId != null)
            {
                string getTopic =
                    "SELECT T.Topic " +
                    "FROM ConfigTopic CT  " +
                    "JOIN Topic T ON T.TopicID = CT.TopicID " +
                    "WHERE CT.ConfigID = @ConfigID;";

                var getTopicCmd = new SqlCommand(getTopic, connection);
                var configID = new SqlParameter
                {
                    ParameterName = "@ConfigID",
                    SqlDbType = SqlDbType.Int,
                    Value = configId
                };
                getTopicCmd.Parameters.Add(configID);

                var reader = getTopicCmd.ExecuteReader();
                var topics = new List<TopicModel>();
                try
                {
                    while (reader.Read())
                    {
                        topics.Add(new TopicModel { Topic = reader.GetString(0) });
                    }
                }
                finally
                {
                    reader.Close();
                }
                return topics;
            }
            else
            {
                return null;
            }
        }

        private IEnumerable<TagModel> GetTagsByConfigId(int? configId, SqlConnection connection)
        {
            if (configId != null)
            {
                string getTag =
                    "SELECT T.Tag " +
                    "FROM ConfigTag CT " +
                    "JOIN Tag T ON T.TagID = CT.TagID " +
                    "WHERE CT.ConfigID = @ConfigID;";

                var getTagCmd = new SqlCommand(getTag, connection);
                var configID = new SqlParameter
                {
                    ParameterName = "@ConfigID",
                    SqlDbType = SqlDbType.Int,
                    Value = configId
                };
                getTagCmd.Parameters.Add(configID);
                var reader = getTagCmd.ExecuteReader();

                var tags = new List<TagModel>();
                try
                {
                    while (reader.Read())
                    {
                        tags.Add(new TagModel { Tag = reader.GetString(0) });
                    }
                }
                finally
                {
                    reader.Close();
                }
                return tags;
            }
            else
            {
                return null;
            }
        }

        private int AddTag(string tag, SqlConnection connection)
        {
            var addTagCmd =
                    new SqlCommand("INSERT INTO Tag (Tag) VALUES (@Tag);SELECT @TagID = SCOPE_IDENTITY()", connection);
            addTagCmd.Parameters.Add(new SqlParameter() { ParameterName = "@Tag", SqlDbType = SqlDbType.NVarChar, Value = tag });
            var tagID = new SqlParameter
            {
                ParameterName = "@TagID",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };
            addTagCmd.Parameters.Add(tagID);
            addTagCmd.ExecuteNonQuery();
            return (int)tagID.Value;
        }

        private int AddTopic(string topic, SqlConnection connection)
        {
            var topicId = GetTopicId(topic, connection);
            if (!topicId.HasValue)
            {
                var addTopicCmd =
                    new SqlCommand("INSERT INTO Topic (Topic) VALUES (@Topic);SELECT @TopicID = SCOPE_IDENTITY()", connection);
                addTopicCmd.Parameters.Add(new SqlParameter() { ParameterName = "@Topic", SqlDbType = SqlDbType.NVarChar, Value = topic });
                var topicID = new SqlParameter
                {
                    ParameterName = "@TopicID",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                };
                addTopicCmd.Parameters.Add(topicID);
                addTopicCmd.ExecuteNonQuery();
                return (int)topicID.Value;
            }
            else
            {
                return topicId.Value;
            }
        }

        private int? GetTagId(string tag, SqlConnection connection)
        {
            using (var tagExistsCmd = new SqlCommand("SELECT COUNT(TagID) FROM Tag WHERE Tag = @Tag;", connection))
            {
                tagExistsCmd.Parameters.Add("@Tag", SqlDbType.NVarChar, fieldLength).Value = tag;
                bool tagExists = Convert.ToBoolean(tagExistsCmd.ExecuteScalar());

                if (tagExists)
                {
                    using (var getTagIdCmd = new SqlCommand("SELECT TagID FROM Tag WHERE Tag = @Tag", connection))
                    {
                        getTagIdCmd.Parameters.Add("@Tag", SqlDbType.NVarChar, fieldLength).Value = tag;
                        return Convert.ToInt32(getTagIdCmd.ExecuteScalar());
                    }
                }
                return null;
            }
        }

        private int? GetTopicId(string topic, SqlConnection connection)
        {
            using (var topicExistsCmd = new SqlCommand("SELECT COUNT(TopicID) FROM Topic WHERE Topic = @Topic", connection))
            {
                topicExistsCmd.Parameters.Add("@Topic", SqlDbType.NVarChar, fieldLength).Value = topic;
                bool topicExists = Convert.ToBoolean(topicExistsCmd.ExecuteScalar());
                if (topicExists)
                {
                    using (var getTopicIdCmd = new SqlCommand("SELECT TopicID FROM Topic WHERE Topic = @Topic", connection))
                    {
                        getTopicIdCmd.Parameters.Add("@Topic", SqlDbType.NVarChar, fieldLength).Value = topic;
                        return Convert.ToInt32(getTopicIdCmd.ExecuteScalar());
                    }
                }
                return null;
            }
        }

        private void AssignConfigTag(int tagId, int configId, SqlConnection connection)
        {
            using (var configTagExistsCmd = new SqlCommand("SELECT COUNT(ConfigID) FROM ConfigTag WHERE ConfigID = @ConfigID AND TagID = @TagID", connection))
            {
                configTagExistsCmd.Parameters.Add(new SqlParameter { ParameterName = "@ConfigID", SqlDbType = SqlDbType.Int, Value = configId });
                configTagExistsCmd.Parameters.Add(new SqlParameter { ParameterName = "@TagID", SqlDbType = SqlDbType.Int, Value = tagId });
                // Not exists
                if (!Convert.ToBoolean(configTagExistsCmd.ExecuteScalar()))
                {
                    using (var addConfigTagCmd = new SqlCommand("INSERT INTO ConfigTag (ConfigID, TagID) VALUES(@ConfigID, @TagID)", connection))
                    {
                        addConfigTagCmd.Parameters.Add("@ConfigID", SqlDbType.Int).Value = configId;
                        addConfigTagCmd.Parameters.Add("@TagID", SqlDbType.Int).Value = tagId;
                        addConfigTagCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private void AssignConfigTopic(int topicId, int? configId, SqlConnection connection)
        {
            using (var configTopicExistsCmd = new SqlCommand("SELECT COUNT(ConfigID) FROM ConfigTopic WHERE ConfigID = @ConfigID AND TopicID = @TopicID", connection))
            {
                configTopicExistsCmd.Parameters.Add(new SqlParameter { ParameterName = "@ConfigID", SqlDbType = SqlDbType.Int, Value = configId });
                configTopicExistsCmd.Parameters.Add(new SqlParameter { ParameterName = "@TopicID", SqlDbType = SqlDbType.Int, Value = topicId });

                // Not exists
                if (!Convert.ToBoolean(configTopicExistsCmd.ExecuteScalar()))
                {
                    using (var addConfigTopicCmd = new SqlCommand("INSERT INTO ConfigTopic (ConfigID, TopicID) VALUES(@ConfigID, @TopicID)", connection))
                    {
                        addConfigTopicCmd.Parameters.Add("@ConfigID", SqlDbType.Int).Value = configId;
                        addConfigTopicCmd.Parameters.Add("@TopicID", SqlDbType.Int).Value = topicId;
                        addConfigTopicCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private void AddTagsToConfig(IEnumerable<TagModel> tags, int configId, SqlConnection connection)
        {
            foreach (var tag in tags)
            {
                var tagId = GetTagId(tag.Tag, connection);
                if (!tagId.HasValue)
                {
                    tagId = AddTag(tag.Tag, connection);
                }
                AssignConfigTag(tagId.Value, configId, connection);
            }
        }

        private void AddTopicsToConfig(IEnumerable<TopicModel> topics, int configId, SqlConnection connection)
        {
            foreach (var topic in topics)
            {
                var topicId = GetTagId(topic.Topic, connection);
                if (!topicId.HasValue)
                {
                    topicId = AddTopic(topic.Topic, connection);
                }
                AssignConfigTopic(topicId.Value, configId, connection);
            }
        }
        #endregion
    }
}