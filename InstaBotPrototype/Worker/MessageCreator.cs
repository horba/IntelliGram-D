using InstaBotPrototype;
using InstaBotPrototype.Models;
using InstaBotPrototype.Services.AI;
using InstaBotPrototype.Services.Instagram;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net.Http;

namespace Worker
{
    class MyEqualityComparer : EqualityComparer<TopicModel>
    {
        public override bool Equals(TopicModel x, TopicModel y)
        {
            return x.Topic.Equals(y.Topic);
        }

        public override int GetHashCode(TopicModel obj)
        {
            return obj.Topic.GetHashCode();
        }
    }
    class MessageCreator
    {
        IInstagramService instagramService = new InstagramService();
        IRecognizer recognizer = new MicrosoftImageRecognizer();
        string connectionString = AppSettingsProvider.Config["connectionString"];
        private DbProviderFactory factory = DbProviderFactories.GetFactoryByProvider(AppSettingsProvider.Config["dataProvider"]);
        private IEnumerable<TopicModel> GetUserTopics(int userId)
        {
            var topics = new List<TopicModel>();
            using (var DbConnection = factory.CreateConnection())
            {
                DbConnection.ConnectionString = connectionString;
                DbConnection.Open();
                var selectTopics = factory.CreateCommand();
                selectTopics.CommandText = @"SELECT Topic,dbo.Topic.TopicID FROM dbo.Configuration 
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
                    topics.Add(new TopicModel(topicsReader.GetInt32(1),topicsReader.GetString(0)));
                }
                topicsReader.Close();
            }
            return topics;
        }

        private IEnumerable<TopicModel> GetUserTags(int userId)
        {
            var tags = new List<TopicModel>();
            using (var DbConnection = factory.CreateConnection())
            {
                DbConnection.ConnectionString = connectionString;
                DbConnection.Open();
                var selectTags = factory.CreateCommand();
                selectTags.CommandText = @"SELECT Tag,dbo.Tag.TagID FROM dbo.Configuration 
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
                    tags.Add(new TopicModel(tagsReader.GetInt32(1), tagsReader.GetString(0)));
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
                insertCmd.CommandText = "INSERT INTO Messages VALUES (@ChatId,@Message,GETDATE(),NULL,@PostId);SELECT @MessageID = SCOPE_IDENTITY()";

                var messageIDParam = factory.CreateParameter();
                messageIDParam.DbType = System.Data.DbType.Int32;
                messageIDParam.Direction = ParameterDirection.Output;
                messageIDParam.ParameterName = "@MessageID";
                insertCmd.Parameters.Add(messageIDParam);

                insertCmd.ExecuteNonQuery();
                var msgID = Convert.ToInt32(messageIDParam.Value);
                foreach (var tag in msg.Tags) {
                    var insertTag = factory.CreateCommand();
                    insertTag.CommandText = "INSERT INTO MessageTag VALUES (@MessageID,@TagID)";
                    insertTag.Connection = DbConnection;
                    var tagIDParam = factory.CreateParameter();
                    tagIDParam.DbType = System.Data.DbType.Int32;
                    tagIDParam.Value = tag;
                    tagIDParam.ParameterName = "@TagID";
                    insertTag.Parameters.Add(tagIDParam);
                    messageIDParam = factory.CreateParameter();
                    messageIDParam.DbType = System.Data.DbType.Int32;
                    messageIDParam.Value = msgID;
                    messageIDParam.ParameterName = "@MessageID";
                    insertTag.Parameters.Add(messageIDParam);
                    insertTag.ExecuteNonQuery();
                }
                foreach (var topic in msg.Topics)
                {
                    var insertTopic = factory.CreateCommand();
                    insertTopic.CommandText = "INSERT INTO MessageTopic VALUES (@MessageID,@TopicID)";
                    insertTopic.Connection = DbConnection;
                    var topicIDParam = factory.CreateParameter();
                    topicIDParam.DbType = System.Data.DbType.Int32;
                    topicIDParam.Value = topic;
                    topicIDParam.ParameterName = "@TopicID";
                    insertTopic.Parameters.Add(topicIDParam);
                    messageIDParam = factory.CreateParameter();
                    messageIDParam.DbType = System.Data.DbType.Int32;
                    messageIDParam.Value = msgID;
                    messageIDParam.ParameterName = "@MessageID";
                    insertTopic.Parameters.Add(messageIDParam);
                    insertTopic.ExecuteNonQuery();
                }
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
                command.CommandText = "select count(ChatId) from dbo.Messages where ChatId = @chatId and PostId = @message";
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
                    var description = recognizer.RecognizeTopic(response);
                    var topicsRec = description.Tags.Select(x => new TopicModel(0, x));
                    var matchingTopics = topics.Intersect(topicsRec, new MyEqualityComparer());
                    var matchingTags = tags.Intersect(post.Tags.Select(x => new TopicModel(0, x)), new MyEqualityComparer());
                    if (matchingTopics.Count() > 0 || matchingTags.Count() > 0)
                    {
                        var caption = description.Captions.FirstOrDefault();
                        var txt = post.Link;
                        if (caption != null)
                            txt += Environment.NewLine + "Description : " + caption.Text;
                        var msg = new Message(chatID.Value, txt , post.Id);
                        Console.WriteLine("Image # " + counter);
                        Console.WriteLine(string.Format("Url : {0}", post.Images.StandartResolution.Url));
                        Console.Write("Matching topics : ");
                        foreach (var topic in matchingTopics)
                        {
                            msg.Topics.Add(topic.TopicId.Value);
                            Console.Write(topic.Topic + " ");
                        }
                        Console.WriteLine();
                        Console.Write("Matching tags : ");
                        foreach (var tag in matchingTags)
                        {
                            msg.Tags.Add(tag.TopicId.Value);
                            Console.Write(tag.Topic + " ");
                        }
                        Console.WriteLine();
                        ++counter;
                        if (chatID.HasValue && ImageIsNew(chatID.Value, post.Id))
                        {
                            InsertMessage(msg);
                        }
                    }
                }
            }
        }
    }
}