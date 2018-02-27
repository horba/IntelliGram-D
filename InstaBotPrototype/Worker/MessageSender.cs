using InstaBotPrototype;
using InstaBotPrototype.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Linq;
namespace Worker
{
    class MessageSender
    {
        private string connString = AppSettingsProvider.Config["connectionString"];
        public TelegramBot Bot { get; private set; }
        private DbProviderFactory factory = DbProviderFactories.GetFactoryByProvider(AppSettingsProvider.Config["dataProvider"]);
        public MessageSender(TelegramBot bot) => Bot = bot ?? throw new ArgumentNullException(nameof(bot));

        private string GetTagById(int tagId) {
           using (var connection = factory.CreateConnection())
             {
                connection.ConnectionString = connString;
                connection.Open();
                using (var getTagIdCmd = factory.CreateCommand())
                {
                       getTagIdCmd.CommandText = "SELECT Tag FROM Tag WHERE TagID = @Tag";
                       getTagIdCmd.Connection = connection;
                       var tagParam = factory.CreateParameter();
                       tagParam.Value = tagId;
                       tagParam.ParameterName = "@Tag";
                       getTagIdCmd.Parameters.Add(tagParam);
                       return Convert.ToString(getTagIdCmd.ExecuteScalar());
                }
                }
        }
        private String BuildFromIEnumerable(IEnumerable<string> items)
        {
            var builder = new StringBuilder();
            foreach (var item in items)
            {
                builder.Append(item + " ");
            }
            return builder.ToString();
        }
        private string GetTopicById(int tagId)
        {
            using (var connection = factory.CreateConnection())
            {
                connection.ConnectionString = connString;
                connection.Open();
                using (var getTagIdCmd = factory.CreateCommand())
                {
                    getTagIdCmd.CommandText = "SELECT Topic FROM Topic WHERE TopicID = @Tag";
                    getTagIdCmd.Connection = connection;
                    var tagParam = factory.CreateParameter();
                    tagParam.Value = tagId;
                    tagParam.ParameterName = "@Tag";
                    getTagIdCmd.Parameters.Add(tagParam);
                    return Convert.ToString(getTagIdCmd.ExecuteScalar());
                }
            }
        }
        private IEnumerable<int> GetMessageTopic(int msgId)
        {
            List<int> topics = new List<int>();
            using (var connection = factory.CreateConnection())
            {
                connection.ConnectionString = connString;
                connection.Open();
                using (var getTagIdCmd = factory.CreateCommand())
                {
                    getTagIdCmd.CommandText = "SELECT TopicID FROM MessageTopic WHERE MessageID = @Message";
                    getTagIdCmd.Connection = connection;
                    var tagParam = factory.CreateParameter();
                    tagParam.Value = msgId;
                    tagParam.ParameterName = "@Message";
                    tagParam.DbType = System.Data.DbType.Int32;
                    getTagIdCmd.Parameters.Add(tagParam);
                    var reader = getTagIdCmd.ExecuteReader();
                    while (reader.Read()) {
                        topics.Add(reader.GetInt32(0));    
                    }
                }
            }
            return topics;
        }
        private IEnumerable<int> GetMessageTags(int msgId)
        {
            List<int> topics = new List<int>();
            using (var connection = factory.CreateConnection())
            {
                connection.ConnectionString = connString;
                connection.Open();
                using (var getTagIdCmd = factory.CreateCommand())
                {
                    getTagIdCmd.CommandText = "SELECT TagID FROM MessageTag WHERE MessageID = @Message";
                    getTagIdCmd.Connection = connection;
                    var tagParam = factory.CreateParameter();
                    tagParam.Value = msgId;
                    tagParam.ParameterName = "@Message";
                    tagParam.DbType = System.Data.DbType.Int32;
                    getTagIdCmd.Parameters.Add(tagParam);
                    var reader = getTagIdCmd.ExecuteReader();
                    while (reader.Read())
                    {
                        topics.Add(reader.GetInt32(0));
                    }
                }
            }
            return topics;
        }
        public void Start()
        {
            var dbConnection = factory.CreateConnection();
            dbConnection.ConnectionString = connString;
            dbConnection.Open();
            var getMsgCmd = factory.CreateCommand();
            getMsgCmd.Connection = dbConnection;
            getMsgCmd.CommandText =
                @"SELECT Messages.Id, Messages.ChatId, Messages.Message FROM Messages
                  JOIN TelegramIntegration ON TelegramIntegration.ChatId = Messages.ChatId
                  WHERE Send IS NULL AND Muted = 0 ORDER BY Timestamp;"; 
            var reader = getMsgCmd.ExecuteReader();

            List<Message> messages = new List<Message>();


            while (reader.Read())
            {
                var id = Convert.ToInt32(reader["Id"]);
                var chatId = Convert.ToInt64(reader["ChatId"]);
                var message = Convert.ToString(reader["Message"]);
                messages.Add(new Message(id, chatId, message));
            }
            reader.Close();

            foreach (var m in messages)
            {
                try
                {
                    var builder = new StringBuilder();
                    builder.Append(m.Text);
                    builder.AppendLine();
                    builder.Append("Matched topics: ");
                    builder.Append(BuildFromIEnumerable(GetMessageTopic(m.Id).Select(x => GetTopicById(x))));
                    builder.AppendLine();
                    builder.Append("Matched tags: ");
                    builder.Append(BuildFromIEnumerable(GetMessageTags(m.Id).Select(x => GetTagById(x))));
                    builder.AppendLine();
                    TelegramBot.SendMessageAsync(m.ChatId, builder.ToString());


                    var setDateCommand = factory.CreateCommand();
                    setDateCommand.Connection = dbConnection;
                    setDateCommand.CommandText = "UPDATE Messages SET Send = GETDATE() WHERE Send IS NULL AND Id = @Id;";
                    var idParam = factory.CreateParameter();
                    idParam.ParameterName = "@Id";
                    idParam.Value = m.Id;

                    setDateCommand.Parameters.AddRange(new[] { idParam });
                    setDateCommand.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Something went wrong during sending the message to the user with chatId:{0}", m.ChatId);
                }

            }

            dbConnection.Close();

            Console.ReadKey();

        }
    }
}