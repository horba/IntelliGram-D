using System;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace InstaBotPrototype.Services.DB
{
    public class ConfigService : IConfigService
    {
        private string connectionString = AppSettingsProvider.Config["connectionString"];
        #region IConfigService implementation
        public int AddConfig(int userId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                    connection.Open();
                    string addConfigQuery =
                       @"INSERT INTO Configuration (UserId) 
                       VALUES (@UserId);
                       SELECT @ConfigID = SCOPE_IDENTITY();";
                    SqlCommand addConfigCmd = new SqlCommand(addConfigQuery, connection);
                    addConfigCmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    SqlParameter configID = new SqlParameter()
                    {
                        ParameterName = "@ConfigID",
                        SqlDbType = SqlDbType.Int,
                        Direction = ParameterDirection.Output
                    };
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
            using (var dbConnection = new SqlConnection())
            {
                if (sessionId != null)
                {
                    dbConnection.ConnectionString = connectionString;
                    var param = new SqlParameter();
                    param.ParameterName = "@Id";
                    param.Value = sessionId;
                    var check = new SqlCommand();
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
            using (var dbConnection = new SqlConnection())
            {
                dbConnection.ConnectionString = connectionString;
                dbConnection.Open();
                var id = GetUserIdBySession(sessionId);
                var selectTelegram = new SqlCommand();
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
            using (var dbConnection = new SqlConnection())
            {
                dbConnection.ConnectionString = connectionString;
                dbConnection.Open();
                var insertCmd = new SqlCommand();
                insertCmd.Connection = dbConnection;
                insertCmd.CommandText = "INSERT INTO InstagramIntegration VALUES (@UserId,@InstaId,@Nick,@Token)";
                var userIdParam = new SqlParameter();
                userIdParam.ParameterName = "@InstaId";
                userIdParam.Value = id;
                var nickParam = new SqlParameter();
                nickParam.ParameterName = "@Nick";
                nickParam.Value = nickname;
                var tokenParam = new SqlParameter();
                tokenParam.ParameterName = "@Token";
                tokenParam.Value = tokenStr;
                var userid = GetUserIdBySession(sessionId);
                var idParam = new SqlParameter();
                idParam.ParameterName = "@UserId";
                idParam.Value = userid.Value;
                insertCmd.Parameters.AddRange(new[] { idParam, userIdParam, nickParam, tokenParam });
                insertCmd.ExecuteNonQuery();
            }
        }
        public bool IsUserVerifiedInInstagram(string sessionId)
        {
            using (var dbConnection = new SqlConnection())
            {
                var id = GetUserIdBySession(sessionId);
                dbConnection.ConnectionString = connectionString;
                dbConnection.Open();
                var insertCmd = new SqlCommand();
                insertCmd.Connection = dbConnection;
                insertCmd.CommandText = "SELECT COUNT (*) FROM InstagramIntegration WHERE UserId = @Id";
                var userIdParam = new SqlParameter();
                userIdParam.ParameterName = "@Id";
                userIdParam.Value = id;
                insertCmd.Parameters.AddRange(new[] { userIdParam });
                return Convert.ToInt32(insertCmd.ExecuteScalar()) == 1;
            }
        }
        public IEnumerable<string> GetUserTopics(int userId)
        {
            var topics = new List<string>();
            using (var DbConnection = new SqlConnection())
            {
                DbConnection.ConnectionString = connectionString;
                DbConnection.Open();
                var selectTopics = new SqlCommand();
                selectTopics.CommandText = @"SELECT Topic FROM dbo.Configuration 
                                            JOIN dbo.ConfigTopic ON dbo.Configuration.ConfigID = dbo.ConfigTopic.ConfigID 
                                            JOIN Topic ON dbo.Topic.TopicID = dbo.ConfigTopic.TopicID
                                            WHERE dbo.ConfigTopic.ConfigID = (SELECT MAX(ConfigID) from dbo.Configuration WHERE dbo.Configuration.UserId = @id)";
                selectTopics.Connection = DbConnection;
                var parameter = new SqlParameter();
                parameter.ParameterName = "@id";
                parameter.Value = userId;
                parameter.DbType = System.Data.DbType.Int32;
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
            using (var DbConnection = new SqlConnection())
            {
                DbConnection.ConnectionString = connectionString;
                DbConnection.Open();
                var selectTags = new SqlCommand();
                selectTags.CommandText = @"SELECT Tag FROM dbo.Configuration 
                                            JOIN dbo.ConfigTag ON dbo.Configuration.ConfigID = dbo.ConfigTag.ConfigID 
                                            JOIN Tag ON dbo.Tag.TagID = dbo.ConfigTag.TagID
                                            WHERE dbo.ConfigTag.ConfigID = (SELECT MAX(ConfigID) from dbo.Configuration WHERE dbo.Configuration.UserId = @id)";
                selectTags.Connection = DbConnection;
                var parameter = new SqlParameter();
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
            using (var DbConnection = new SqlConnection())
            {
                DbConnection.ConnectionString = connectionString;
                DbConnection.Open();
                var selectTags = new SqlCommand();
                selectTags.CommandText = @"SELECT MAX(ConfigID) from dbo.Configuration WHERE dbo.Configuration.UserId = @id";
                selectTags.Connection = DbConnection;
                var parameter = new SqlParameter();
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
            using (var dbConnection = new SqlConnection(connectionString))
            {
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
            using (var dbConnection = new SqlConnection(connectionString))
            {
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
            using (var dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                var tagId = GetTagId(item, dbConnection);
                using (var configTagExistsCmd = new SqlCommand("SELECT COUNT(ConfigID) FROM ConfigTag WHERE ConfigID = @ConfigID AND TagID = @TagID", dbConnection))
                {
                    configTagExistsCmd.Parameters.Add(new SqlParameter { ParameterName = "@ConfigID", SqlDbType = SqlDbType.Int, Value = configId });
                    configTagExistsCmd.Parameters.Add(new SqlParameter { ParameterName = "@TagID", SqlDbType = SqlDbType.Int, Value = tagId });
                    if (Convert.ToBoolean(configTagExistsCmd.ExecuteScalar()))
                    {
                        using (var addConfigTagCmd = new SqlCommand("DELETE FROM ConfigTag WHERE ConfigID = @ConfigID AND TagID = @TagID", dbConnection))
                        {
                            addConfigTagCmd.Parameters.Add("@ConfigID", SqlDbType.Int).Value = configId;
                            addConfigTagCmd.Parameters.Add("@TagID", SqlDbType.Int).Value = tagId;
                            addConfigTagCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
        public void DeleteTopic(string item, int configId)
        {
            using (var dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                var topicId = GetTopicId(item, dbConnection);
                using (var configTopicExistsCmd = new SqlCommand("SELECT COUNT(ConfigID) FROM ConfigTopic WHERE ConfigID = @ConfigID AND TopicID = @TopicID", dbConnection))
                {
                    configTopicExistsCmd.Parameters.Add(new SqlParameter { ParameterName = "@ConfigID", SqlDbType = SqlDbType.Int, Value = configId });
                    configTopicExistsCmd.Parameters.Add(new SqlParameter { ParameterName = "@TopicID", SqlDbType = SqlDbType.Int, Value = topicId });
                    if (Convert.ToBoolean(configTopicExistsCmd.ExecuteScalar()))
                    {
                        using (var addConfigTopicCmd = new SqlCommand("DELETE FROM ConfigTopic WHERE ConfigID = @ConfigID AND TopicID = @TopicID", dbConnection))
                        {
                            addConfigTopicCmd.Parameters.Add("@ConfigID", SqlDbType.Int).Value = configId;
                            addConfigTopicCmd.Parameters.Add("@TopicID", SqlDbType.Int).Value = topicId;
                            addConfigTopicCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
        #endregion

        #region Private helper methods

        private int InsertTag(string tag, SqlConnection connection)
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
        private int InsertTopic(string topic, SqlConnection connection)
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
                tagExistsCmd.Parameters.Add("@Tag", SqlDbType.NVarChar).Value = tag;
                bool tagExists = Convert.ToBoolean(tagExistsCmd.ExecuteScalar());

                if (tagExists)
                {
                    using (var getTagIdCmd = new SqlCommand("SELECT TagID FROM Tag WHERE Tag = @Tag", connection))
                    {
                        getTagIdCmd.Parameters.Add("@Tag", SqlDbType.NVarChar).Value = tag;
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
                topicExistsCmd.Parameters.Add("@Topic", SqlDbType.NVarChar).Value = topic;
                bool topicExists = Convert.ToBoolean(topicExistsCmd.ExecuteScalar());
                if (topicExists)
                {
                    using (var getTopicIdCmd = new SqlCommand("SELECT TopicID FROM Topic WHERE Topic = @Topic", connection))
                    {
                        getTopicIdCmd.Parameters.Add("@Topic", SqlDbType.NVarChar).Value = topic;
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
        private void AssignConfigTopic(int topicId, int configId, SqlConnection connection)
        {
            using (var configTopicExistsCmd = new SqlCommand("SELECT COUNT(ConfigID) FROM ConfigTopic WHERE ConfigID = @ConfigID AND TopicID = @TopicID", connection))
            {
                configTopicExistsCmd.Parameters.Add(new SqlParameter { ParameterName = "@ConfigID", SqlDbType = SqlDbType.Int, Value = configId });
                configTopicExistsCmd.Parameters.Add(new SqlParameter { ParameterName = "@TopicID", SqlDbType = SqlDbType.Int, Value = topicId });
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


        #endregion
    }
}