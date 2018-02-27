using System;
using System.Collections.Generic;
using System.Data.Common;

namespace InstaBotPrototype.Services
{
    public class StatService: IStatService
    {
        private static string connectionString = AppSettingsProvider.Config["connectionString"];
        private DbProviderFactory factory = DbProviderFactories.GetFactoryByProvider(AppSettingsProvider.Config["dataProvider"]);
        public int CountTags(int userId) {
            using (var connection = factory.CreateConnection()) {
                connection.ConnectionString = connectionString;
                connection.Open();
                var command = factory.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM dbo.MessageTag " +
                    "JOIN Messages ON dbo.Messages.Id = dbo.MessageTag.MessageID " +
                    "JOIN TelegramIntegration ON dbo.TelegramIntegration.ChatId = dbo.Messages.ChatId " +
                    "WHERE dbo.TelegramIntegration.UserId = @id";
                command.Connection = connection;
                var param = factory.CreateParameter();
                param.ParameterName = "@id";
                param.Value = userId;
                param.DbType = System.Data.DbType.Int32;
                command.Parameters.Add(param);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }
        public int CountTopics(int userId)
        {
            using (var connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                var command = factory.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM dbo.MessageTopic " +
                    "JOIN Messages ON dbo.Messages.Id = dbo.MessageTopic.MessageID " +
                    "JOIN TelegramIntegration ON dbo.TelegramIntegration.ChatId = dbo.Messages.ChatId " +
                    "WHERE dbo.TelegramIntegration.UserId = @id";
                command.Connection = connection;
                var param = factory.CreateParameter();
                param.ParameterName = "@id";
                param.Value = userId;
                param.DbType = System.Data.DbType.Int32;
                command.Parameters.Add(param);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }
        public int CountPhotos(int userId)
        {
            using (var connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                var command = factory.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM Messages " +
                    "JOIN TelegramIntegration ON dbo.TelegramIntegration.ChatId = dbo.Messages.ChatId " +
                    "WHERE dbo.TelegramIntegration.UserId = @id;";
                command.Connection = connection;
                var param = factory.CreateParameter();
                param.ParameterName = "@id";
                param.Value = userId;
                param.DbType = System.Data.DbType.Int32;
                command.Parameters.Add(param);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

    }
}
