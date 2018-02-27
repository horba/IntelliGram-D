using System;
using System.Data;
using System.Collections.Generic;
using System.Data.Common;

namespace InstaBotPrototype.Services.DB
{
    public class ConfigService : IConfigService
    {
        private string connectionString = AppSettingsProvider.Config["connectionString"];
        private DbProviderFactory factory = DbProviderFactories.GetFactoryByProvider(AppSettingsProvider.Config["dataProvider"]);
        #region IConfigService implementation
        public int AddConfig(int userId)
        {
            using (var connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                string addConfigQuery =
                       @"INSERT INTO Configuration (UserId) 
                       VALUES (@UserId);
                       SELECT @ConfigID = SCOPE_IDENTITY();";
                var addConfigCmd = factory.CreateCommand();
                addConfigCmd.CommandText = addConfigQuery;
                addConfigCmd.Connection = connection;
                var param = factory.CreateParameter();
                param.ParameterName = "@UserId";
                param.Value = userId;
                param.DbType = DbType.Int32;
                addConfigCmd.Parameters.Add(param);
                var configID = factory.CreateParameter();
                configID.ParameterName = "@ConfigID";
                configID.Direction = ParameterDirection.Output;
                configID.DbType = DbType.Int32;
                addConfigCmd.Parameters.Add(configID);
                addConfigCmd.ExecuteNonQuery();
                return Convert.ToInt32(configID.Value);
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
                var telegramParam = factory.CreateParameter();
                telegramParam.ParameterName = "@id";
                telegramParam.Value = id;
                telegramParam.DbType = DbType.Int32;
                selectTelegram.Parameters.Add(telegramParam);
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
        public string GetInstagramNick(string sessionId)
        {
            using (var connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                var command = factory.CreateCommand();
                command.CommandText = "SELECT Nickname FROM dbo.InstagramIntegration WHERE UserId = @id";
                command.Connection = connection;
                var param = factory.CreateParameter();
                param.ParameterName = "@id";
                param.Value = GetUserIdBySession(sessionId);
                param.DbType = System.Data.DbType.Int32;
                command.Parameters.Add(param);
                var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    return reader.GetString(0);
                }
                else {
                    return null;
                }
            }
        }
        public IEnumerable<string> GetUserTopics(int userId)
        {
            var topics = new List<string>();
            using (var DbConnection = factory.CreateConnection())
            {
                DbConnection.ConnectionString = connectionString;
                DbConnection.Open();
                var selectTopics = factory.CreateCommand();
                selectTopics.CommandText = @"SELECT Topic FROM dbo.Configuration 
                                            JOIN dbo.ConfigTopic ON dbo.Configuration.ConfigID = dbo.ConfigTopic.ConfigID 
                                            JOIN Topic ON dbo.Topic.TopicID = dbo.ConfigTopic.TopicID
                                            WHERE dbo.ConfigTopic.ConfigID = (SELECT MAX(ConfigID) from dbo.Configuration WHERE dbo.Configuration.UserId = @id)";
                selectTopics.Connection = DbConnection;
                var parameter = factory.CreateParameter();
                parameter.ParameterName = "@id";
                parameter.Value = userId;
                selectTopics.Parameters.Add(parameter);
                var topicsReader = selectTopics.ExecuteReader();
                while (topicsReader.Read())
                {
                    topics.Add(topicsReader.GetString(0));
                }
                topicsReader.Close();
            }
            return topics;
        }
        public IEnumerable<string> GetUserTags(int userId)
        {
            var tags = new List<string>();
            using (var DbConnection = factory.CreateConnection())
            {
                DbConnection.ConnectionString = connectionString;
                DbConnection.Open();
                var selectTags = factory.CreateCommand();
                selectTags.CommandText = @"SELECT Tag FROM dbo.Configuration 
                                            JOIN dbo.ConfigTag ON dbo.Configuration.ConfigID = dbo.ConfigTag.ConfigID 
                                            JOIN Tag ON dbo.Tag.TagID = dbo.ConfigTag.TagID
                                            WHERE dbo.ConfigTag.ConfigID = (SELECT MAX(ConfigID) from dbo.Configuration WHERE dbo.Configuration.UserId = @id)";
                selectTags.Connection = DbConnection;
                var parameter = factory.CreateParameter();
                parameter.ParameterName = "@id";
                parameter.Value = userId;
                parameter.DbType = System.Data.DbType.Int32;
                selectTags.Parameters.Add(parameter);
                var tagsReader = selectTags.ExecuteReader();
                while (tagsReader.Read())
                {
                    tags.Add(tagsReader.GetString(0));
                }
                tagsReader.Close();
            }
            return tags;
        }
        public int? GetLatestConfig(int userId)
        {
            int? configId = null;
            using (var dbConnection = factory.CreateConnection())
            {
                dbConnection.ConnectionString = connectionString;
                dbConnection.Open();
                var selectTags = factory.CreateCommand();
                selectTags.CommandText = @"SELECT MAX(ConfigID) from dbo.Configuration WHERE dbo.Configuration.UserId = @id";
                selectTags.Connection = dbConnection;
                var parameter = factory.CreateParameter();
                parameter.ParameterName = "@id";
                parameter.Value = userId;
                parameter.DbType = System.Data.DbType.Int32;
                selectTags.Parameters.Add(parameter);
                var queryResult = selectTags.ExecuteScalar();
                try
                {
                    configId = Convert.ToInt32(queryResult);
                }
                catch {
                    configId = AddConfig(userId);
                }
            }
            return configId;
        }
        public void AddTag(string item, int configId)
        {
            using (var dbConnection = factory.CreateConnection())
            {
                dbConnection.ConnectionString = connectionString;
                dbConnection.Open();
                var tagID = GetTagId(item, dbConnection);
                if (!tagID.HasValue)
                {
                    tagID = InsertTag(item, dbConnection);
                }
                AssignConfigTag(tagID.Value, configId, dbConnection);
            }
        }
        public void AddTopic(string item, int configId)
        {
            using (var dbConnection = factory.CreateConnection())
            {
                dbConnection.ConnectionString = connectionString;
                dbConnection.Open();
                var topicID = GetTopicId(item, dbConnection);
                if (!topicID.HasValue)
                {
                    topicID = InsertTopic(item, dbConnection);
                }
                AssignConfigTopic(topicID.Value, configId, dbConnection);
            }
        }
        public void DeleteTag(string item, int configId)
        {
            using (var dbConnection = factory.CreateConnection())
            {
                dbConnection.ConnectionString = connectionString;
                dbConnection.Open();
                var topicId = GetTagId(item, dbConnection);
                using (var configTopicExistsCmd = factory.CreateCommand())
                {
                    configTopicExistsCmd.Connection = dbConnection;
                    configTopicExistsCmd.CommandText = "SELECT COUNT(ConfigID) FROM ConfigTag WHERE ConfigID = @ConfigID AND TagID = @TagID";
                    var configParam = factory.CreateParameter();
                    configParam.ParameterName = "@ConfigID";
                    configParam.Value = configId;
                    configParam.DbType = DbType.Int32;
                    configTopicExistsCmd.Parameters.Add(configParam);
                    var topicParam = factory.CreateParameter();
                    topicParam.ParameterName = "@TagID";
                    topicParam.Value = topicId;
                    topicParam.DbType = DbType.Int32;
                    configTopicExistsCmd.Parameters.Add(topicParam);
                    if (Convert.ToBoolean(configTopicExistsCmd.ExecuteScalar()))
                    {
                        using (var addConfigTopicCmd = factory.CreateCommand())
                        {
                            addConfigTopicCmd.CommandText = "DELETE FROM ConfigTag WHERE ConfigID = @ConfigID AND TagID = @TagID";
                            addConfigTopicCmd.Connection = dbConnection;
                            configParam = factory.CreateParameter();
                            configParam.ParameterName = "@ConfigID";
                            configParam.Value = configId;
                            configParam.DbType = DbType.Int32;
                            addConfigTopicCmd.Parameters.Add(configParam);
                            topicParam = factory.CreateParameter();
                            topicParam.ParameterName = "@TagID";
                            topicParam.Value = topicId;
                            topicParam.DbType = DbType.Int32;
                            addConfigTopicCmd.Parameters.Add(topicParam);
                            addConfigTopicCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
        public void DeleteTopic(string item, int configId)
        {
            using (var dbConnection = factory.CreateConnection())
            {
                dbConnection.ConnectionString = connectionString;
                dbConnection.Open();
                var topicId = GetTopicId(item, dbConnection);
                using (var configTopicExistsCmd = factory.CreateCommand())
                {
                    configTopicExistsCmd.Connection = dbConnection;
                    configTopicExistsCmd.CommandText = "SELECT COUNT(ConfigID) FROM ConfigTopic WHERE ConfigID = @ConfigID AND TopicID = @TopicID";
                    var configParam = factory.CreateParameter();
                    configParam.ParameterName = "@ConfigID";
                    configParam.Value = configId;
                    configParam.DbType = DbType.Int32;
                    configTopicExistsCmd.Parameters.Add(configParam);
                    var topicParam = factory.CreateParameter();
                    topicParam.ParameterName = "@TopicID";
                    topicParam.Value = topicId;
                    topicParam.DbType = DbType.Int32;
                    configTopicExistsCmd.Parameters.Add(topicParam);
                    if (Convert.ToBoolean(configTopicExistsCmd.ExecuteScalar()))
                    {
                        using (var addConfigTopicCmd = factory.CreateCommand())
                        {
                            addConfigTopicCmd.CommandText = "DELETE FROM ConfigTopic WHERE ConfigID = @ConfigID AND TopicID = @TopicID";
                            addConfigTopicCmd.Connection = dbConnection;
                            configParam = factory.CreateParameter();
                            configParam.ParameterName = "@ConfigID";
                            configParam.Value = configId;
                            configParam.DbType = DbType.Int32;
                            addConfigTopicCmd.Parameters.Add(configParam);
                            topicParam = factory.CreateParameter();
                            topicParam.ParameterName = "@TopicID";
                            topicParam.Value = topicId;
                            topicParam.DbType = DbType.Int32;
                            addConfigTopicCmd.Parameters.Add(topicParam);
                            addConfigTopicCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
        #endregion

        #region Private helper methods

        private int InsertTag(string tag, DbConnection connection)
        {
            var addTagCmd =
                    factory.CreateCommand();
            addTagCmd.CommandText = "INSERT INTO Tag (Tag) VALUES (@Tag);SELECT @TagID = SCOPE_IDENTITY()";
            addTagCmd.Connection = connection;
            var tagParam = factory.CreateParameter();
            tagParam.ParameterName = "@Tag";
            tagParam.Value = tag;
            addTagCmd.Parameters.Add(tagParam);
            var tagIDParam = factory.CreateParameter();
            tagIDParam.DbType = DbType.Int32;
            tagIDParam.Direction = ParameterDirection.Output;
            tagIDParam.ParameterName = "@TagID";
            addTagCmd.Parameters.Add(tagIDParam);
            addTagCmd.ExecuteNonQuery();
            return Convert.ToInt32(tagIDParam.Value);
        }
        private int InsertTopic(string topic, DbConnection connection)
        {
            var addTagCmd = factory.CreateCommand();
            addTagCmd.CommandText = "INSERT INTO Topic (Topic) VALUES (@Topic); SELECT @TopicID = SCOPE_IDENTITY()";
            addTagCmd.Connection = connection;
            var tagParam = factory.CreateParameter();
            tagParam.ParameterName = "@Topic";
            tagParam.Value = topic;
            addTagCmd.Parameters.Add(tagParam);
            var tagIDParam = factory.CreateParameter();
            tagIDParam.DbType = DbType.Int32;
            tagIDParam.Direction = ParameterDirection.Output;
            tagIDParam.ParameterName = "@TopicID";
            addTagCmd.Parameters.Add(tagIDParam);
            addTagCmd.ExecuteNonQuery();
            return Convert.ToInt32(tagIDParam.Value);
        }
        private int? GetTagId(string tag, DbConnection connection)
        {
            using (var tagExistsCmd = factory.CreateCommand())
            {
                tagExistsCmd.CommandText = "SELECT COUNT(TagID) FROM Tag WHERE Tag = @Tag;";
                tagExistsCmd.Connection = connection;
                var tagParam = factory.CreateParameter();
                tagParam.Value = tag;
                tagParam.ParameterName = "@Tag";
                tagExistsCmd.Parameters.Add(tagParam);
                bool tagExists = Convert.ToBoolean(tagExistsCmd.ExecuteScalar());

                if (tagExists)
                {
                    using (var getTagIdCmd = factory.CreateCommand())
                    {
                        getTagIdCmd.CommandText = "SELECT TagID FROM Tag WHERE Tag = @Tag";
                        getTagIdCmd.Connection = connection;
                        tagParam = factory.CreateParameter();
                        tagParam.Value = tag;
                        tagParam.ParameterName = "@Tag";
                        getTagIdCmd.Parameters.Add(tagParam);
                        return Convert.ToInt32(getTagIdCmd.ExecuteScalar());
                    }
                }
                return null;
            }
        }
        private int? GetTopicId(string topic, DbConnection connection)
        {
            using (var tagExistsCmd = factory.CreateCommand())
            {
                tagExistsCmd.CommandText = "SELECT COUNT(TopicID) FROM Topic WHERE Topic = @Topic;";
                tagExistsCmd.Connection = connection;
                var tagParam = factory.CreateParameter();
                tagParam.Value = topic;
                tagParam.ParameterName = "@Topic";
                tagExistsCmd.Parameters.Add(tagParam);
                bool tagExists = Convert.ToBoolean(tagExistsCmd.ExecuteScalar());

                if (tagExists)
                {
                    using (var getTagIdCmd = factory.CreateCommand())
                    {
                        getTagIdCmd.CommandText = "SELECT TopicID FROM Topic WHERE Topic = @Topic";
                        getTagIdCmd.Connection = connection;
                        tagParam = factory.CreateParameter();
                        tagParam.Value = topic;
                        tagParam.ParameterName = "@Topic";
                        getTagIdCmd.Parameters.Add(tagParam);
                        return Convert.ToInt32(getTagIdCmd.ExecuteScalar());
                    }
                }
                return null;
            }
        }
        private void AssignConfigTag(int tagId, int configId, DbConnection connection)
        {
            using (var configTagExistsCmd = factory.CreateCommand())
            {
                configTagExistsCmd.CommandText = "SELECT COUNT(ConfigID) FROM ConfigTag WHERE ConfigID = @ConfigID AND TagID = @TagID";
                configTagExistsCmd.Connection = connection;
                var configParam = factory.CreateParameter();
                configParam.ParameterName = "@ConfigID";
                configParam.Value = configId;
                configParam.DbType = DbType.Int32;
                configTagExistsCmd.Parameters.Add(configParam);
                var tagParam = factory.CreateParameter();
                tagParam.ParameterName = "@TagID";
                tagParam.Value = tagId;
                tagParam.DbType = DbType.Int32;
                configTagExistsCmd.Parameters.Add(tagParam);
                if (!Convert.ToBoolean(configTagExistsCmd.ExecuteScalar()))
                {
                    using (var addConfigTagCmd = factory.CreateCommand())
                    {
                        addConfigTagCmd.CommandText = "INSERT INTO ConfigTag (ConfigID, TagID) VALUES(@ConfigID, @TagID)";
                        addConfigTagCmd.Connection = connection;
                        configParam = factory.CreateParameter();
                        configParam.ParameterName = "@ConfigID";
                        configParam.Value = configId;
                        configParam.DbType = DbType.Int32;
                        tagParam = factory.CreateParameter();
                        tagParam.ParameterName = "@TagID";
                        tagParam.Value = tagId;
                        tagParam.DbType = DbType.Int32;
                        addConfigTagCmd.Parameters.Add(configParam);
                        addConfigTagCmd.Parameters.Add(tagParam);
                        addConfigTagCmd.ExecuteNonQuery();
                    }
                }
            }
        }
        private void AssignConfigTopic(int topicId, int configId, DbConnection connection)
        {
            using (var configTagExistsCmd = factory.CreateCommand())
            {
                configTagExistsCmd.CommandText = "SELECT COUNT(ConfigID) FROM ConfigTopic WHERE ConfigID = @ConfigID AND TopicID = @TopicID";
                configTagExistsCmd.Connection = connection;
                var configParam = factory.CreateParameter();
                configParam.ParameterName = "@ConfigID";
                configParam.Value = configId;
                configParam.DbType = DbType.Int32;
                configTagExistsCmd.Parameters.Add(configParam);
                var tagParam = factory.CreateParameter();
                tagParam.ParameterName = "@TopicID";
                tagParam.Value = topicId;
                tagParam.DbType = DbType.Int32;
                configTagExistsCmd.Parameters.Add(tagParam);
                if (!Convert.ToBoolean(configTagExistsCmd.ExecuteScalar()))
                {
                    using (var addConfigTagCmd = factory.CreateCommand())
                    {
                        addConfigTagCmd.CommandText = "INSERT INTO ConfigTopic (ConfigID, TopicID) VALUES(@ConfigID, @TopicID)";
                        addConfigTagCmd.Connection = connection;
                        configParam = factory.CreateParameter();
                        configParam.ParameterName = "@ConfigID";
                        configParam.Value = configId;
                        configParam.DbType = DbType.Int32;
                        tagParam = factory.CreateParameter();
                        tagParam.ParameterName = "@TopicID";
                        tagParam.Value = topicId;
                        tagParam.DbType = DbType.Int32;
                        addConfigTagCmd.Parameters.Add(configParam);
                        addConfigTagCmd.Parameters.Add(tagParam);
                        addConfigTagCmd.ExecuteNonQuery();
                    }
                }
            }
        }


        #endregion
    }
}