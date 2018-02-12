using System;
using System.Data.Common;

namespace RepositoryLibrary
{
    class TopicRepository : Repository<int, string>
    {
        public override void Add(string topic)
        {
            var args = new DbParameter[]
            {
                CreateParameter("@topic", topic)
            };

            using (var command = CreateCommand("insert into dbo.Topic (Topic) values (@topic)", args))
            {
                command.ExecuteNonQuery();
            }
        }

        public override void Delete(int topicId)
        {
            var parameter = CreateParameter("@topicId", topicId);

            using (var command = CreateCommand("delete from dbo.Topic where TopicID = @topicId", parameter))
            {
                command.ExecuteNonQuery();
            }
        }

        public override string Get(int topicId)
        {
            var parameter = CreateParameter("@topicId", topicId);

            using (var command = CreateCommand("select Topic from dbo.Topic where TopicID = @topicId", parameter))
            using (var reader = command.ExecuteReader())
            {
                reader.Read();
                return reader.GetString(0);
            }
        }

        public override void Update(string tag) => throw new NotImplementedException();
    }
}