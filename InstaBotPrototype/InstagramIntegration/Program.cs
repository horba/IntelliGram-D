using InstaBotPrototype.Services.Instagram;
using InstaBotPrototype.Services.AI;
using System;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using System.Data.Common;
using System.Configuration;

namespace InstagramIntegration
{
    class Program
    {
        private static IInstagramService instagramService = new InstagramService();
        private static IRecognizer recognizer = new MicrosoftImageRecognizer();
        private static string connectionString = ConfigurationManager.ConnectionStrings[1].ConnectionString;
        private static DbProviderFactory factory = DbProviderFactories.GetFactory(ConfigurationManager.ConnectionStrings[1].ProviderName);
        static IEnumerable<String> GetUserTopics(int userId)
        {
            List<string> topics = new List<string>();
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
                    topics.Add(topicsReader.GetString(0));
                topicsReader.Close();
            }
            return topics;
        }
        static IEnumerable<String> GetUserTags(int userId)
        {
            List<string> tags = new List<string>();
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
                    tags.Add(tagsReader.GetString(0));
                tagsReader.Close();
            }
            return tags;
        }
        static int? GetUserIdByInstagram(String nickname)
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
                if (idReader.HasRows) {
                    idReader.Read();
                    userId = idReader.GetInt32(0);
                }
                idReader.Close();
            }
            return userId;
        }
        static IEnumerable<String> GetAllInstagramUsers() {
            List<string> nicknames = new List<string>();
            using (var DbConnection = factory.CreateConnection()) {
                DbConnection.ConnectionString = connectionString;
                DbConnection.Open();
                var selectNicknames = factory.CreateCommand();
                selectNicknames.CommandText = "SELECT Nickname FROM dbo.InstagramIntegration";
                selectNicknames.Connection = DbConnection;
                var nameReader = selectNicknames.ExecuteReader();
                while (nameReader.Read()) 
                    nicknames.Add(nameReader.GetString(0));
                nameReader.Close();
            }
            return nicknames;
        }
        static void Main(string[] args)
        {
            var client = new HttpClient();
            foreach (var user in GetAllInstagramUsers())
            {
                Console.WriteLine(String.Format("User : {0}", user));
                var id = GetUserIdByInstagram(user);
                var tags = GetUserTags(id.Value);
                var topics = GetUserTopics(id.Value);
                int counter = 1;
                foreach (var post in instagramService.GetLatestPosts(user))
                {
                    byte[] response = client.GetByteArrayAsync(post.Images.StandartResolution.Url).Result;
                    var matchingTopics = topics.Intersect(recognizer.RecognizeTopic(response));
                    var matchingTags = tags.Intersect(post.Tags);
                    if (matchingTopics.Count() > 0 || matchingTags.Count() > 0) {
                        Console.WriteLine("Image # " + counter);
                        Console.WriteLine(String.Format("Url : {0}", post.Images.StandartResolution.Url));
                        Console.Write("Matching topics : ");
                        foreach (var topic in matchingTopics)
                            Console.Write(topic + " ");
                        Console.WriteLine();
                        Console.Write("Matching tags : ");
                        foreach (var tag in matchingTags)
                            Console.Write(tag + " ");
                        Console.WriteLine();
                        ++counter;
                    }
                    
                }
            }
            Console.ReadKey();
        }
    }
}
