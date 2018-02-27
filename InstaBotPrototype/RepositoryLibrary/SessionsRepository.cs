using InstaBotPrototype.Models;
using System;
using System.Data.Common;

namespace RepositoryLibrary
{
    class SessionsRepository : Repository<Guid, SessionModel>
    {
        public override void Add(SessionModel model)
        {
            var args = new DbParameter[]
            {
                CreateParameter("@userId", model.UserId),
                CreateParameter("@sessionId", model.SessionId)
            };

            using (var command = CreateCommand("insert into dbo.Sessions (UserId, SessionId) values (@userId, @sessionId)", args))
            {
                command.ExecuteNonQuery();
            }
        }

        public override void Delete(Guid sessionId)
        {
            var parameter = CreateParameter("@sessionId", sessionId);

            using (var command = CreateCommand("delete from dbo.Sessions where SessionId = @sessionId", parameter))
            {
                command.ExecuteNonQuery();
            }
        }

        public override SessionModel Get(Guid sessionId)
        {
            var parameter = CreateParameter("@sessionId", sessionId);

            using (var command = CreateCommand("select * from dbo.Sessions where SessionId = @sessionId", parameter))
            using (var reader = command.ExecuteReader())
            {
                reader.Read();
                return new SessionModel()
                {
                    UserId = reader.GetInt32(0),
                    SessionId = sessionId,
                    LoginTime = reader.GetDateTime(2),
                    LastActive = reader.GetDateTime(3)
                };
            }
        }

        public override void Update(SessionModel model)
        {
            var args = new DbParameter[]
            {
                CreateParameter("@userId", model.UserId),
                CreateParameter("@sessionId", model.SessionId),
                CreateParameter("@loginTime", model.LoginTime),
                CreateParameter("@lastActive", model.LastActive)
            };

            using (var command = CreateCommand("update dbo.Sessions set UserId = @userId, LoginTime = @loginTime, LastActive = @lastActive where SessionId = @sessionId", args))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}