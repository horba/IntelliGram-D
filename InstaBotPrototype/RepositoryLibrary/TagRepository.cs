using System;
using System.Data.Common;

namespace RepositoryLibrary
{
    class TagRepository : Repository<int, string>
    {
        public override void Add(string tag)
        {
            var args = new DbParameter[]
            {
                CreateParameter("@tag", tag)
            };

            using (var command = CreateCommand("insert into dbo.Tag (Tag) values (@tag)", args))
            {
                command.ExecuteNonQuery();
            }
        }

        public override void Delete(int tagId)
        {
            var parameter = CreateParameter("@tagId", tagId);

            using (var command = CreateCommand("delete from dbo.Tag where TagID = @tagId", parameter))
            {
                command.ExecuteNonQuery();
            }
        }

        public override string Get(int tagId)
        {
            var parameter = CreateParameter("@tagId", tagId);

            using (var command = CreateCommand("select Tag from dbo.Tag where TagID = @tagId", parameter))
            using (var reader = command.ExecuteReader())
            {
                reader.Read();
                return reader.GetString(0);
            }
        }

        public override void Update(string tag) => throw new NotImplementedException();
    }
}