using System;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;
using InstaBotPrototype.Services.DB;

namespace InstaBotPrototype.Models
{
    public class ConfigService : IConfigService
    {
        private string connectionString;
        private readonly char[] splitSeparator = { ',', ';' };
        private readonly char[] trimChar = { ' ', '\n', '\t' };
        private const int fieldLength = 128;

        public ConfigService()
        {
            //TODO: Get the connection string from app.config
            //connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Dbconnection"].ConnectionString;
            connectionString = "Server=DESKTOP-2J18FL2\\SQLEXPRESS;Database=Configuration;Trusted_Connection=True;";
        }

        public ConfigService(string connectionStr)
        {
            connectionString = connectionStr;
        }

        #region IConfigService implementation
        
        public ConfigurationModel GetConfig()
        {
            return new ConfigurationModel { InstaUsername = "defaultName", InstaPassword = "defaultPass", TelegramUsername = "defaultTelegram", Tags = "def, ault", Topics = "def, ault" };
        }

        /// <summary>
        /// Returns model from db
        /// </summary>
        /// <param name="id">Config ID</param>
        /// <returns>ConfigurationModel</returns>
        public ConfigurationModel GetConfig(int id)
        {
            if (id > 0)
            {
                ConfigurationModel configModel = new ConfigurationModel();
                using (SqlConnection connection =
                    new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        string getConfigQuery =
                            "SELECT InstaUsername, InstaPassword, TelegramUsername " +
                            "FROM Configuration WHERE ConfigID = @ConfigID";

                        SqlCommand command = new SqlCommand(getConfigQuery, connection);
                        SqlParameter configID = new SqlParameter
                        {
                            ParameterName = "@ConfigID",
                            SqlDbType = SqlDbType.Int,
                            Value = id
                        };
                        command.Parameters.Add(configID);
                        SqlDataReader reader = command.ExecuteReader();

                        // Get main info
                        bool isConfigExists = reader.HasRows;
                        if (isConfigExists)
                        {
                            reader.Read();
                            configModel.InstaUsername = reader.GetString(0);
                            configModel.InstaPassword = reader.GetString(1);
                            configModel.TelegramUsername = reader.GetString(2);

                            reader.Close();

                            // Get Tags
                            string getTag =
                                "SELECT T.Tag " +
                                "FROM ConfigTag CT " +
                                "JOIN Tag T ON T.TagID = CT.TagID " +
                                "WHERE CT.ConfigID = @ConfigID;";

                            SqlCommand getTagCmd = new SqlCommand(getTag, connection);
                            configID = new SqlParameter
                            {
                                ParameterName = "@ConfigID",
                                SqlDbType = SqlDbType.Int,
                                Value = id
                            };
                            getTagCmd.Parameters.Add(configID);
                            reader = getTagCmd.ExecuteReader();
                            string tags = String.Empty;
                            while (reader.Read())
                            {
                                tags += reader.GetString(0).Trim() + ",";
                            }
                            reader.Close();

                            // Delete the last comma
                            configModel.Tags = (tags == String.Empty) ? "" : tags.Substring(0, tags.LastIndexOf(','));

                            // Get Topics
                            string getTopic =
                                "SELECT T.Topic " +
                                "FROM ConfigTopic CT  " +
                                "JOIN Topic T ON T.TopicID = CT.TopicID " +
                                "WHERE CT.ConfigID = @ConfigID;";

                            SqlCommand getTopicCmd = new SqlCommand(getTopic, connection);
                            configID = new SqlParameter
                            {
                                ParameterName = "@ConfigID",
                                SqlDbType = SqlDbType.Int,
                                Value = id
                            };
                            getTopicCmd.Parameters.Add(configID);
                            reader = getTopicCmd.ExecuteReader();
                            string topics = String.Empty;
                            while (reader.Read())
                            {
                                topics += reader.GetString(0).Trim() + ",";
                            }
                            reader.Close();

                            // Delete the last comma
                            configModel.Topics = (topics == String.Empty) ? "" : topics.Substring(0, topics.LastIndexOf(','));
                            return configModel;
                        }
                        else
                            return null;
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
            return null;
        }

        public void SaveConfig(ConfigurationModel newConfig)
        {
            using (SqlConnection connection =
                new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string checkUniqueInstagram =
                        "SELECT COUNT(ConfigID) FROM Configuration WHERE InstaUsername = @InstaUsername";

                    var checkUniqueInstagramCmd = new SqlCommand(checkUniqueInstagram, connection);
                    var instaUsername = new SqlParameter
                    {
                        ParameterName = "@InstaUsername",
                        SqlDbType = SqlDbType.NVarChar,
                        Value = newConfig.InstaUsername
                    };
                    checkUniqueInstagramCmd.Parameters.Add(instaUsername);

                    bool unique =
                        !Convert.ToBoolean(checkUniqueInstagramCmd.ExecuteScalar());

                    if (newConfig.Id == null && unique)
                    {
                        AddConfig(newConfig, connection);
                    }
                    else if (newConfig.Id != null && newConfig.Id > 0 && !unique)
                    {
                        UpdateConfig(newConfig, connection);
                    }
                    else
                    {
                        throw new Exception();
                    }
                    
                }
                catch
                {
                    throw;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        #endregion

        #region Private methods

        private int AddTag(string tag, SqlConnection connection)
        {
            var addTagCmd =
                new SqlCommand("INSERT INTO Tag (Tag) VALUES (@Tag);SELECT @TagID = SCOPE_IDENTITY()", connection);
            addTagCmd.Parameters.Add(new SqlParameter() { ParameterName = "@Tag", SqlDbType = SqlDbType.NVarChar, Value = tag.Trim() });
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
            addTopicCmd.Parameters.Add(new SqlParameter() { ParameterName = "@Topic", SqlDbType = SqlDbType.NVarChar, Value = topic.Trim() });
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

        private bool TagExists(string tag, SqlConnection connection, out int tagId)
        {
            var tagCommand = new SqlCommand(
                "SELECT COUNT(TagID) FROM Tag WHERE Tag = @Tag;" +
                "SELECT @TagID = TagID FROM Tag WHERE Tag = @Tag", connection);
            var tagID = new SqlParameter
            {
                ParameterName = "@TagID",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };
            tagCommand.Parameters.Add(tagID);
            tagCommand.Parameters.Add("@Tag", SqlDbType.NVarChar, fieldLength).Value = tag.Trim();

            bool result = Convert.ToBoolean(tagCommand.ExecuteScalar());
            tagId = result ? (int)tagID.Value : -1;

            return result;
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
            var addConfigTagCmd = new SqlCommand(
                         "INSERT INTO ConfigTag (ConfigID, TagID) " +
                         "VALUES(@ConfigID, @TagID)", connection);
            addConfigTagCmd.Parameters.Add("@ConfigID", SqlDbType.Int).Value = configId;
            addConfigTagCmd.Parameters.Add("@TagID", SqlDbType.Int).Value = tagId;
            addConfigTagCmd.ExecuteNonQuery();
        }

        private void AssignConfigTopic(int topicId, int? configId, SqlConnection connection)
        {
            var addConfigTopicCmd = new SqlCommand(
                         "INSERT INTO ConfigTopic (ConfigID, TopicID) " +
                         "VALUES(@ConfigID, @TopicID)", connection);
            addConfigTopicCmd.Parameters.Add("@ConfigID", SqlDbType.Int).Value = configId;
            addConfigTopicCmd.Parameters.Add("@TopicID", SqlDbType.Int).Value = topicId;
            addConfigTopicCmd.ExecuteNonQuery();
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
            updateCmd.Parameters.Add(new SqlParameter() { ParameterName = "@ConfigID", SqlDbType = SqlDbType.Int, SqlValue = model.Id });
            updateCmd.ExecuteScalar();

            string deleteTags =
                "DELETE FROM ConfigTag WHERE ConfigID = @ConfigID";
            var deleteTagsCmd = new SqlCommand(deleteTags, connection);
            deleteTagsCmd.Parameters.Add(new SqlParameter() { ParameterName = "@ConfigID", SqlDbType = SqlDbType.Int, SqlValue = model.Id });
            deleteTagsCmd.ExecuteNonQuery();

            foreach (var tag in model.Tags.Split(splitSeparator))
            {
                if (!TagExists(tag, connection, out int tagId))
                {
                    tagId = AddTag(tag, connection);
                }
                AssignConfigTag(tagId, (int)model.Id, connection);
            }

            string deleteTopics =
                "DELETE FROM ConfigTopic WHERE ConfigID = @ConfigID";
            var deleteTopicsCmd = new SqlCommand(deleteTopics, connection);
            deleteTopicsCmd.Parameters.Add(new SqlParameter() { ParameterName = "@ConfigID", SqlDbType = SqlDbType.Int, SqlValue = model.Id });
            deleteTopicsCmd.ExecuteNonQuery();

            foreach (var topic in model.Topics.Split(splitSeparator))
            {
                if (!TopicExists(topic, connection, out int topicId))
                {
                    topicId = AddTopic(topic, connection);
                }
                AssignConfigTopic(topicId, (int)model.Id, connection);
            }
        }

        /// <summary>
        /// Сreates a new configuration if it doesn't not exist. 
        /// Otherwise, it updates the existing configuration
        /// </summary>
        /// <param name="model">Input model</param>
        /// <param name="connection">Open sql connection</param>
        private void AddConfig(ConfigurationModel model, SqlConnection connection)
        {
            try
            {
                string addConfigQuery =
                    "INSERT INTO Configuration (InstaUsername, InstaPassword, TelegramUsername) " +
                    "VALUES (@InstaUsername, @InstaPassword,  @TelegramUsername); " +
                    "SELECT @ConfigID = SCOPE_IDENTITY();";

                SqlCommand addConfigCmd = new SqlCommand(addConfigQuery, connection);
                addConfigCmd.Parameters.Add("@InstaUsername", SqlDbType.NVarChar, fieldLength).Value = model.InstaUsername;
                addConfigCmd.Parameters.Add("@InstaPassword", SqlDbType.NVarChar, fieldLength).Value = model.InstaPassword;
                addConfigCmd.Parameters.Add("@TelegramUsername", SqlDbType.NVarChar, fieldLength).Value = model.TelegramUsername;
                SqlParameter configID = new SqlParameter()
                {
                    ParameterName = "@ConfigID",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                };
                addConfigCmd.Parameters.Add(configID);
                addConfigCmd.ExecuteNonQuery();

                List<int> tagIDList = new List<int>();
                string[] tags = model.Tags.Split(splitSeparator);

                foreach (string tag in tags)
                {
                    if (!TagExists(tag, connection, out int tagId))
                    {
                        tagId = AddTag(tag, connection);
                    }
                    tagIDList.Add(tagId);
                }

                SqlCommand addConfigTagCommand;
                foreach (int currentTagID in tagIDList)
                {
                    addConfigTagCommand = new SqlCommand(
                        "SELECT COUNT(ConfigID) FROM ConfigTag " +
                        "WHERE ConfigID = @ConfigID AND TagID = @TagID", connection);
                    addConfigTagCommand.Parameters.Add("@ConfigID", SqlDbType.Int).Value = configID.Value;
                    addConfigTagCommand.Parameters.Add("@TagID", SqlDbType.Int).Value = currentTagID;
                    bool configTagExists = Convert.ToBoolean(addConfigTagCommand.ExecuteScalar());
                    if (!configTagExists)
                    {
                        addConfigTagCommand.CommandText = "INSERT INTO ConfigTag (ConfigID, TagID) VALUES(@ConfigID, @TagID)";
                        addConfigTagCommand.ExecuteNonQuery();
                    }
                }

                List<int> topicIDList = new List<int>();
                string[] topics = model.Topics.Split(splitSeparator);
                foreach (string topic in topics)
                {
                    if (!TopicExists(topic, connection, out int topicId))
                    {
                        topicId = AddTopic(topic, connection);
                    }
                    topicIDList.Add(topicId);
                }

                SqlCommand addConfigTopicCommand;
                foreach (int currentTopicID in topicIDList)
                {
                    addConfigTopicCommand = new SqlCommand("SELECT COUNT(ConfigID) FROM ConfigTopic " +
                        "WHERE ConfigID = @ConfigID AND TopicID = @TopicID", connection);
                    addConfigTopicCommand.Parameters.Add("@ConfigID", SqlDbType.Int).Value = configID.Value;
                    addConfigTopicCommand.Parameters.Add("@TopicID", SqlDbType.Int).Value = currentTopicID;
                    var isTopicExists = Convert.ToBoolean(addConfigTopicCommand.ExecuteScalar());
                    if (!isTopicExists)
                    {
                        addConfigTopicCommand.CommandText = "INSERT INTO ConfigTopic (ConfigID, TopicID) VALUES(@ConfigID, @TopicID)";
                        addConfigTopicCommand.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        #endregion
    }
}
