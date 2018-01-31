using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using InstaBotPrototype.Services.DB;
using System.Data.Common;

namespace InstaBotPrototype.Models
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
                InstaUsername = "defaultName",
                InstaPassword = "defaultPass",
                TelegramUsername = "defaultTelegram",
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
                        connection.Open();
                        string getConfigQuery =
                            "SELECT InstaUsername, InstaPassword, TelegramUsername " +
                            "FROM Configuration WHERE ConfigID = @ConfigID";

                        using (var command = new SqlCommand(getConfigQuery, connection))
                        {
                            var configID = new SqlParameter
                            {
                                ParameterName = "@ConfigID",
                                SqlDbType = SqlDbType.Int,
                                Value = id
                            };
                            command.Parameters.Add(configID);
                            var reader = command.ExecuteReader();

                            // Get main info
                            bool configExists = reader.HasRows;
                            if (configExists)
                            {
                                try
                                {
                                    reader.Read();
                                    configModel.InstaUsername = reader.GetString(0);
                                    configModel.InstaPassword = reader.GetString(1);
                                    configModel.TelegramUsername = reader.GetString(2);
                                }
                                finally
                                {
                                    reader.Close();
                                }

                                // Get tags
                                configModel.Tags = GetTagsByConfigId(id, connection);
                                // Get topics
                                configModel.Topics = GetTopicsByConfigId(id, connection);
                                return configModel;
                            }
                        }
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

        public void SaveConfig(ConfigurationModel config, String sessionId)
        {
            using (var connection =
                new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    bool isInstagramUsernameUnique = IsInstagramUsernameUnique(config.InstaUsername, connection);

                    if (config.ConfigId == null && isInstagramUsernameUnique)
                    {
                        AddConfig(config, sessionId ,connection);
                    }
                    else if (config.ConfigId != null && config.ConfigId > 0 && !isInstagramUsernameUnique)
                    {
                        UpdateConfig(config, connection);
                    }
                    else
                    {
                        throw new Exception("Incorrect instaUsername or user id");
                    }

                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public bool IsLoggedIn(String sessionID)
        {
            return GetUserIdBySession(sessionID).HasValue;   
        }
        public int? GetUserIdBySession(string sessionId) {
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
                    if (reader.HasRows) {
                        reader.Read();
                        userId = reader.GetInt32(0);
                    }
                    reader.Close();
                }
            }
            return userId;
        }
        #endregion

        #region Private helper methods

        private void AddConfig(ConfigurationModel model,String sessionId, SqlConnection connection)
        {
            try
            {
                string addConfigQuery =
                    "INSERT INTO Configuration (InstaUsername, InstaPassword, TelegramUsername,UserId) " +
                    "VALUES (@InstaUsername, @InstaPassword,  @TelegramUsername,@UserId); " +
                    "SELECT @ConfigID = SCOPE_IDENTITY();";

                SqlCommand addConfigCmd = new SqlCommand(addConfigQuery, connection);
                addConfigCmd.Parameters.Add("@InstaUsername", SqlDbType.NVarChar, fieldLength).Value = model.InstaUsername;
                addConfigCmd.Parameters.Add("@InstaPassword", SqlDbType.NVarChar, fieldLength).Value = model.InstaPassword;
                addConfigCmd.Parameters.Add("@TelegramUsername", SqlDbType.NVarChar, fieldLength).Value = model.TelegramUsername;
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

                AddTopicsToConfigId(model.Topics, (int)model.ConfigId, connection);
                AddTagsToConfigId(model.Tags, (int)model.ConfigId, connection);
                

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void UpdateConfig(ConfigurationModel model, SqlConnection connection)
        {
            string updateQuery = "UPDATE Configuration " +
                "SET InstaPassword = @InstaPassword, " +
                "TelegramUsername = @TelegramUsername " +
                "WHERE ConfigID = @ConfigID";
            var updateCmd = new SqlCommand(updateQuery, connection);
            updateCmd.Parameters.Add(new SqlParameter() { ParameterName = "@InstaPassword", SqlDbType = SqlDbType.NVarChar, SqlValue = model.InstaPassword });
            updateCmd.Parameters.Add(new SqlParameter() { ParameterName = "@TelegramUsername", SqlDbType = SqlDbType.NVarChar, SqlValue = model.TelegramUsername });
            updateCmd.Parameters.Add(new SqlParameter() { ParameterName = "@ConfigID", SqlDbType = SqlDbType.Int, SqlValue = model.ConfigId });
            updateCmd.ExecuteScalar();

            TrimTagsTopics(model);

            string deleteTags =
                "DELETE FROM ConfigTag WHERE ConfigID = @ConfigID";
            var deleteTagsCmd = new SqlCommand(deleteTags, connection);
            deleteTagsCmd.Parameters.Add(new SqlParameter() { ParameterName = "@ConfigID", SqlDbType = SqlDbType.Int, SqlValue = model.ConfigId });
            deleteTagsCmd.ExecuteNonQuery();

            AddTagsToConfigId(model.Tags, (int)model.ConfigId, connection);

            string deleteTopics =
                "DELETE FROM ConfigTopic WHERE ConfigID = @ConfigID";
            var deleteTopicsCmd = new SqlCommand(deleteTopics, connection);
            deleteTopicsCmd.Parameters.Add(new SqlParameter() { ParameterName = "@ConfigID", SqlDbType = SqlDbType.Int, SqlValue = model.ConfigId });
            deleteTopicsCmd.ExecuteNonQuery();

            AddTopicsToConfigId(model.Topics, (int)model.ConfigId, connection);
        }

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

        private bool IsInstagramUsernameUnique(string instaUsername, SqlConnection connection)
        {
            if (instaUsername != null)
            {
                string checkUniqueInstagram =
                "SELECT COUNT(ConfigID) FROM Configuration WHERE InstaUsername = @InstaUsername";

                using (var checkUniqueInstagramCmd = new SqlCommand(checkUniqueInstagram, connection))
                {
                    var instaParameter = new SqlParameter
                    {
                        ParameterName = "@InstaUsername",
                        SqlDbType = SqlDbType.NVarChar,
                        Value = instaUsername
                    };
                    checkUniqueInstagramCmd.Parameters.Add(instaParameter);

                    return !Convert.ToBoolean(checkUniqueInstagramCmd.ExecuteScalar());
                }
            }
            else
            {
                return false;
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

        private int? GetTagId(string tag, SqlConnection connection)
        {
            using (var tagExistsCmd = new SqlCommand("SELECT COUNT(TagID) FROM Tag WHERE Tag = @Tag;", connection))
            {
                tagExistsCmd.Parameters.Add("@Tag", SqlDbType.NVarChar, fieldLength).Value = tag;
                bool tagExists = Convert.ToBoolean(tagExistsCmd.ExecuteScalar());

                if (tagExists)
                {
                    using (var getTagIdCmd = new SqlCommand("SELECT @TagID = TagID FROM Tag WHERE Tag = @Tag", connection))
                    {
                        var tagId = new SqlParameter
                        {
                            ParameterName = "@TagID",
                            SqlDbType = SqlDbType.Int,
                            Direction = ParameterDirection.Output
                        };
                        getTagIdCmd.Parameters.Add(tagId);
                        getTagIdCmd.Parameters.Add("@Tag", SqlDbType.NVarChar, fieldLength).Value = tag;
                        return (int?)tagId.Value;
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
                    using (var getTopicIdCmd = new SqlCommand("SELECT @TopicID = TopicID FROM Topic WHERE Topic = @Topic", connection))
                    {
                        var topicId = new SqlParameter
                        {
                            ParameterName = "@TopicID",
                            SqlDbType = SqlDbType.Int,
                            Direction = ParameterDirection.Output
                        };
                        getTopicIdCmd.Parameters.Add(topicId);
                        getTopicIdCmd.Parameters.Add("@Topic", SqlDbType.NVarChar, fieldLength).Value = topic;
                        return (int?)topicId.Value;
                    }
                }
                return null;
            }
        }

        private bool TopicExists(string topic, SqlConnection connection, out int topicId)
        {
            var topicCommand = new SqlCommand(
                "SELECT COUNT(TopicID) FROM Topic WHERE Topic = @Topic;" +
                "SELECT @TopicID = TopicID FROM Topic WHERE Topic = @Topic", connection);
            var topicID = new SqlParameter
            {
                ParameterName = "@TopicID",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };
            topicCommand.Parameters.Add(topicID);
            topicCommand.Parameters.Add("@Topic", SqlDbType.NVarChar, fieldLength).Value = topic.Trim(trimChar);

            bool result = Convert.ToBoolean(topicCommand.ExecuteScalar());
            topicId = result ? (int)topicID.Value : -1;

            return result;
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

        private void AddTagsToConfigId(IEnumerable<TagModel> tags, int configId, SqlConnection connection)
        {
            int? tagId;
            foreach (var tag in tags)
            {
                tagId = GetTagId(tag.Tag, connection);
                if (tagId == null)
                {
                    tagId = AddTag(tag.Tag, connection);
                }
                AssignConfigTag((int)tagId, configId, connection);
            }
        }

        private void AddTopicsToConfigId(IEnumerable<TopicModel> topics, int configId, SqlConnection connection)
        {
            int? topicId;
            foreach (var topic in topics)
            {
                topicId = GetTopicId(topic.Topic, connection);
                if (topicId == null)
                {
                    topicId = AddTopic(topic.Topic, connection);
                }
                AssignConfigTopic((int)topicId, (int)configId, connection);
            }
        }

        #endregion
    }
}