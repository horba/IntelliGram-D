using InstaBotPrototype.Models;
using System.Data.Common;

namespace RepositoryLibrary
{
    public class UsersRepository : Repository<int, UserModel>
    {
        public override void Add(UserModel model)
        {
            var args = new DbParameter[]
            {
                CreateParameter("@login", model.Login),
                CreateParameter("@password", model.Password),
                CreateParameter("@email", model.Email),
                CreateParameter("@lastLogin", model.LastLogin),
                CreateParameter("@registerDate", model.RegisterDate)
            };

            using (var command = CreateCommand("insert into dbo.Users (Login, Password, Email, LastLogin, RegisterDate) values (@login, @password, @email, @lastLogin, @registerDate)", args))
            {
                command.ExecuteNonQuery();
            }
        }

        public override void Delete(int id)
        {
            var parameter = CreateParameter("@id", id);

            using (var command = CreateCommand("delete from dbo.Users where Id = @id", parameter))
            {
                command.ExecuteNonQuery();
            }
        }

        public override UserModel Get(int id)
        {
            var parameter = CreateParameter("@id", id);

            using (var command = CreateCommand("select * from dbo.Users where Id = @id", parameter))
            using (var reader = command.ExecuteReader())
            {
                reader.Read();
                return new UserModel()
                {
                    Id = id,
                    Login = reader.GetString(1),
                    Password = reader.GetString(2),
                    LastLogin = reader.GetDateTime(3),
                    RegisterDate = reader.GetDateTime(4),
                    Email = reader.GetString(5)
                };
            }
        }

        public override void Update(UserModel model)
        {
            var args = new DbParameter[]
            {
                CreateParameter("@id", model.Id),
                CreateParameter("@login", model.Login),
                CreateParameter("@password", model.Password),
                CreateParameter("@email", model.Email),
                CreateParameter("@lastLogin", model.LastLogin),
                CreateParameter("@registerDate", model.RegisterDate)
            };

            using (var command = CreateCommand("update dbo.Users set Login = @login, Password = @password, Email = @email, LastLogin = @lastLogin, RegisterDate = @registerDate where Id = @id", args))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}