using System;
using System.Collections.Generic;

using System.Data.SqlClient;

namespace InstaBotPrototype.Services.Autocompletion
{
    public class AutocompletionService : IAutocompletionService
    {
        private static string connectionString = AppSettingsProvider.Config["connectionString"];

        private const int tagLimit = 3;
        private const int topicLimit = 3;

        public IEnumerable<string> GetTagСompletion(string tag)
        {
            if (tag != null)
            {
                var tags = new List<string>();

                using (var dbConnection = new SqlConnection())
                {
                    dbConnection.ConnectionString = connectionString;
                    dbConnection.Open();

                    var selectTags = new SqlCommand();
                    selectTags.CommandText = "SELECT TOP (@TagCount) Tag FROM dbo.Tag WHERE (LOWER(Tag) LIKE CONCAT(@TagToFind, '%'));";
                    selectTags.Connection = dbConnection;

                    var tagParam = new SqlParameter();
                    tagParam.ParameterName = "@TagToFind";
                    tagParam.Value = tag.ToLower();
                    tagParam.DbType = System.Data.DbType.String;
                    selectTags.Parameters.Add(tagParam);

                    var tagLimitParam = new SqlParameter();
                    tagLimitParam.ParameterName = "@TagCount";
                    tagLimitParam.Value = tagLimit;
                    tagLimitParam.DbType = System.Data.DbType.Int32;
                    selectTags.Parameters.Add(tagLimitParam);

                    var tagReader = selectTags.ExecuteReader();
                    while (tagReader.Read())
                    {
                        tags.Add(tagReader.GetString(0));
                    }
                    dbConnection.Close();
                }
                return tags;
            }
            else
            {
                throw new ArgumentNullException("Tag can't be null");
            }
        }
        public IEnumerable<string> GetTopicCompletion(string topic)
        {
            if (topic != null)
            {
                var topics = new List<string>();

                using (var dbConnection = new SqlConnection())
                {
                    dbConnection.ConnectionString = connectionString;
                    dbConnection.Open();

                    var selectTopics = new SqlCommand();
                    selectTopics.CommandText = "SELECT TOP (@TopicCount) Topic FROM dbo.Topic WHERE (LOWER(Topic) LIKE CONCAT(@TopicToFind, '%'));";
                    selectTopics.Connection = dbConnection;

                    var topicParam = new SqlParameter();
                    topicParam.ParameterName = "@TopicToFind";
                    topicParam.Value = topic.ToLower();
                    topicParam.DbType = System.Data.DbType.String;
                    selectTopics.Parameters.Add(topicParam);

                    var topicLimitParam = new SqlParameter();
                    topicLimitParam.ParameterName = "@TopicCount";
                    topicLimitParam.Value = topicLimit;
                    topicLimitParam.DbType = System.Data.DbType.Int32;
                    selectTopics.Parameters.Add(topicLimitParam);

                    var topicReader = selectTopics.ExecuteReader();
                    while (topicReader.Read())
                    {
                        topics.Add(topicReader.GetString(0));
                    }
                    dbConnection.Close();
                }
                return topics;
            }
            else
            {
                throw new ArgumentNullException("Topic can't be null");
            }
        }
    }
}
