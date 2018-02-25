using InstaBotPrototype;
using InstaBotPrototype.Models;
using InstaBotPrototype.Services.AI;
using InstaBotPrototype.Services.Instagram;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.Http;

namespace Worker
{
    class MessageCreator
    {
        IInstagramService instagramService = new InstagramService();
        IRecognizer recognizer = new MicrosoftImageRecognizer();
        string connectionString = AppSettingsProvider.Config["connectionString"];
        private DbProviderFactory factory = DbProviderFactories.GetFactoryByProvider(AppSettingsProvider.Config["dataProvider"]);
        private IEnumerable<string> GetUserTopics(int userId)
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
                                            WHERE dbo.Configuration.UserId = @id";
                selectTopics.Connection = DbConnection;
                var parameter = factory.CreateParameter();
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

        private IEnumerable<string> GetUserTags(int userId)
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
                                           WHERE dbo.Configuration.UserId = @id";
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

        private long? GetChatIdByUserId(int id)
        {
            long? userId = null;
            using (var DbConnection = factory.CreateConnection())
            {
                DbConnection.ConnectionString = connectionString;
                DbConnection.Open();
                var selectId = factory.CreateCommand();
                selectId.CommandText = @"SELECT [ChatId]
                                          FROM[dbo].[TelegramIntegration]
                                          WHERE[dbo].[TelegramIntegration].[UserId] = @id;";
                selectId.Connection = DbConnection;
                var parameter = factory.CreateParameter();
                parameter.ParameterName = "@id";
                parameter.Value = id;
                parameter.DbType = System.Data.DbType.Int32;
                selectId.Parameters.Add(parameter);
                var idReader = selectId.ExecuteReader();
                if (idReader.HasRows)
                {
                    idReader.Read();
                    userId = idReader.GetInt64(0);
                }
                idReader.Close();
            }
            return userId;
        }

        private void InsertMessage(Message msg)
        {
            using (var DbConnection = factory.CreateConnection())
            {
                DbConnection.ConnectionString = connectionString;
                DbConnection.Open();
                var insertCmd = factory.CreateCommand();
                insertCmd.Connection = DbConnection;


                var chatIdParam = factory.CreateParameter();
                chatIdParam.ParameterName = "@ChatId";
                chatIdParam.Value = msg.ChatId;

                var msgParam = factory.CreateParameter();
                msgParam.ParameterName = "@Message";
                msgParam.Value = msg.Text;

                var postIdParam = factory.CreateParameter();
                postIdParam.ParameterName = "@PostId";
                postIdParam.Value = msg.PostId;

                insertCmd.Parameters.AddRange(new[] { chatIdParam, msgParam, postIdParam });
                insertCmd.CommandText = "INSERT INTO Messages VALUES (@ChatId,@Message,GETDATE(),NULL,@PostId);";
                insertCmd.ExecuteNonQuery();

            }
        }

        private int? GetUserIdByInstagram(string nickname)
        {
            int? userId = null;
            using (var DbConnection = factory.CreateConnection())
            {
                DbConnection.ConnectionString = connectionString;
                DbConnection.Open();
                var selectId = factory.CreateCommand();
                selectId.CommandText = "SELECT UserId FROM dbo.InstagramIntegration WHERE Nickname = @nickname";
                selectId.Connection = DbConnection;
                var parameter = factory.CreateParameter();
                parameter.ParameterName = "@nickname";
                parameter.Value = nickname;
                selectId.Parameters.Add(parameter);
                var idReader = selectId.ExecuteReader();
                if (idReader.HasRows)
                {
                    idReader.Read();
                    userId = idReader.GetInt32(0);
                }
                idReader.Close();
            }
            return userId;
        }

        private IEnumerable<string> GetAllInstagramUsers()
        {
            var nicknames = new List<string>();
            using (var DbConnection = factory.CreateConnection())
            {
                DbConnection.ConnectionString = connectionString;
                DbConnection.Open();
                var selectNicknames = factory.CreateCommand();
                selectNicknames.CommandText = "SELECT Nickname FROM dbo.InstagramIntegration";
                selectNicknames.Connection = DbConnection;
                var nameReader = selectNicknames.ExecuteReader();
                while (nameReader.Read())
                {
                    nicknames.Add(nameReader.GetString(0));
                }
                nameReader.Close();
            }
            return nicknames;
        }
        private bool ImageIsNew(long? chatID, string url)
        {
            using (var DbConnection = factory.CreateConnection())
            {
                DbConnection.ConnectionString = connectionString;
                DbConnection.Open();
                var command = factory.CreateCommand();
                command.Connection = DbConnection;
                command.CommandText = "select count(ChatId) from dbo.Messages where ChatId = @chatId and Message = @message";
                var p1 = factory.CreateParameter();
                p1.ParameterName = "@chatId";
                p1.Value = chatID.Value;
                var p2 = factory.CreateParameter();
                p2.ParameterName = "@message";
                p2.Value = url;
                command.Parameters.AddRange(new[] { p1, p2 });
                return Convert.ToInt32(command.ExecuteScalar()) == 0;
            }
        }
        public void Start()
        {
            var client = new HttpClient();
            foreach (var user in GetAllInstagramUsers())
            {
                Console.WriteLine(string.Format("User : {0}", user));
                var id = GetUserIdByInstagram(user);
                var tags = GetUserTags(id.Value);
                var topics = GetUserTopics(id.Value);
                var chatID = GetChatIdByUserId(id.Value);
                var counter = 1;
                foreach (var post in instagramService.GetLatestPosts(user))
                {
                    var response = client.GetByteArrayAsync(post.Images.StandartResolution.Url).Result;
                    var matchingTopics = topics.Intersect(recognizer.RecognizeTopic(response));
                    var matchingTags = tags.Intersect(post.Tags);
                    if (matchingTopics.Count() > 0 || matchingTags.Count() > 0)
                    {
                        Console.WriteLine("Image # " + counter);
                        Console.WriteLine(string.Format("Url : {0}", post.Images.StandartResolution.Url));
                        Console.Write("Matching topics : ");
                        foreach (var topic in matchingTopics)
                        {
                            Console.Write(topic + " ");
                        }
                        Console.WriteLine();
                        Console.Write("Matching tags : ");
                        foreach (var tag in matchingTags)
                        {
                            Console.Write(tag + " ");
                        }
                        Console.WriteLine();
                        ++counter;
                        if (chatID.HasValue && ImageIsNew(chatID.Value, post.Images.StandartResolution.Url))
                        {
                            var msg = new Message(chatID.Value, post.Images.StandartResolution.Url);
                            InsertMessage(msg);
                        }
                    }
                }
            }
        }
    }
}