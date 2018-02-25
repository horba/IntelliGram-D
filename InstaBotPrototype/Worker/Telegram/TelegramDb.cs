using InstaBotPrototype;
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Worker
{
    class TelegramDb
    {
        private string connectionString;
        private DbProviderFactory factory = DbProviderFactories.GetFactoryByProvider(AppSettingsProvider.Config["dataProvider"]);

        public TelegramDb(string connectionStr) => connectionString = connectionStr;

        #region Db methods

        #region Instagram methods

        public string GetUsersToken(long chatId)
        {
            using (var connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                var getUsersToken = factory.CreateCommand();
                getUsersToken.Connection = connection;
                getUsersToken.CommandText = 
                        @"SELECT AccessToken FROM InstagramIntegration
                         JOIN TelegramIntegration ON InstagramIntegration.UserId = TelegramIntegration.UserId
                         WHERE TelegramIntegration.ChatId = @ChatId";
                var idParam = factory.CreateParameter();
                idParam.ParameterName = "ChatId";
                idParam.Value = chatId;
                getUsersToken.Parameters.Add(idParam);
                var reader = getUsersToken.ExecuteReader();
                string accessToken;
                if (reader.HasRows)
                {
                    reader.Read();
                    accessToken = reader.GetString(0);
                    return accessToken;
                }
                else
                {
                    throw new Exception("Token was not found");
                }
            }
        }

        #endregion

        public async Task SetNotificationAsync(long chatId, bool muted = true)
        {
            using (var connection = CreateConnection())
            {
                connection.ConnectionString = connectionString;
                await connection.OpenAsync();
                string updateQuery = "UPDATE dbo.TelegramIntegration SET Muted = @MuteOption WHERE ChatId = @ChatId;";
                var muteCmd = CreateCommand(updateQuery, connection);
                muteCmd.Parameters.Add(CreateParameter("@ChatId", chatId, DbType.Int64));
                muteCmd.Parameters.Add(CreateParameter("@MuteOption", muted, DbType.Boolean));
                muteCmd.Connection = connection;
                await muteCmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<bool> IsMutedAsync(long chatId)
        {
            using (var dbConnection = CreateConnection())
            {
                dbConnection.ConnectionString = connectionString;
                dbConnection.Open();

                string muteQuery = "SELECT Muted FROM dbo.TelegramIntegration WHERE ChatId = @ChatId;";
                var selectMute = CreateCommand(muteQuery, dbConnection);
                selectMute.Parameters.Add(CreateParameter("@ChatId", chatId, DbType.Int64));
                selectMute.Connection = dbConnection;

                return (bool)await selectMute.ExecuteScalarAsync();
            }
        }

        public async Task<string> GetUsersAsync()
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    await connection.OpenAsync();
                    var getUsersQuery = "SELECT * FROM TelegramIntegration";
                    using (var getUsersCmd = CreateCommand(getUsersQuery, connection))
                    {
                        var users = string.Empty;
                        var reader = await getUsersCmd.ExecuteReaderAsync();
                        if (reader.HasRows)
                        {
                            var currentLastName = "";
                            while (reader.Read())
                            {
                                currentLastName = reader.IsDBNull(2) ? "null" : reader.GetString(2);
                                users += $"{reader.GetInt32(0)} {reader.GetString(1)} {currentLastName}\n";
                            }
                        }
                        reader.Close();
                        return users;
                    }
                }
            }
            catch
            {
                return "Db error";
            }

        }

        public async Task<bool> UserExistsAsync(int chatId, DbConnection connection)
        {
            var checkExistsQuery = "SELECT COUNT(ChatId) FROM TelegramIntegration WHERE ChatId = @ChatId";
            using (var checkExistsCmd = CreateCommand(checkExistsQuery, connection))
            {
                checkExistsCmd.Parameters.Add(CreateParameter("@ChatId", chatId, DbType.Int64));
                return Convert.ToInt32(await checkExistsCmd.ExecuteScalarAsync()) != 0;
            }
        }

        public async Task<bool> AddUserAsync(Message message)
        {
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();
                if (!await UserExistsAsync((int)message.Chat.Id, connection))
                {
                    var addQuery =
                        "INSERT INTO TelegramIntegration " +
                        "(ChatId, FirstName, LastName) " +
                        "VALUES(@ChatId, @FirstName, @LastName)";

                    using (var addCmd = CreateCommand(addQuery, connection))
                    {
                        addCmd.Parameters.Add(CreateParameter("@ChatId", message.Chat.Id, DbType.Int32));
                        addCmd.Parameters.Add(CreateParameter("@FirstName", message.Chat.FirstName));
                        addCmd.Parameters.Add(CreateParameter("@LastName", (object)message.Chat.LastName ?? DBNull.Value));
                        await addCmd.ExecuteNonQueryAsync();
                        return true;
                    }
                }
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int chatId)
        {
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();
                if (await UserExistsAsync(chatId, connection))
                {
                    var deleteQuery = "DELETE FROM TelegramIntegration WHERE ChatId = @ChatId";
                    using (var deleteCmd = CreateCommand(deleteQuery, connection))
                    {
                        deleteCmd.Parameters.Add(CreateParameter("@ChatId",chatId,DbType.Int32));
                        await deleteCmd.ExecuteScalarAsync();
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<bool> CheckVerificationAsync(long chatId)
        {
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();
                var checkVerificationQuery = "SELECT COUNT(UserId) FROM TelegramIntegration WHERE ChatId = @ChatId";
                using (var checkVerificationCmd = CreateCommand(checkVerificationQuery, connection))
                {
                    checkVerificationCmd.Parameters.Add(CreateParameter("@ChatId", chatId,DbType.Int64));
                    return Convert.ToBoolean(Convert.ToInt32(await checkVerificationCmd.ExecuteScalarAsync()) != 0);
                }
            }
        }

        public async Task<bool> VerifyAsync(long telegramVerificationKey, long chatId)
        {
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();
                var checkVerificationKeyQuery =
                    "SELECT UserId " +
                    "FROM TelegramVerification " +
                    "WHERE TelegramVerificationKey = @TelegramVerificationKey";

                using (var verificationCommand = CreateCommand(checkVerificationKeyQuery, connection))
                {
                    verificationCommand.Parameters.Add(CreateParameter("@TelegramVerificationKey",telegramVerificationKey));
                    var userId = await verificationCommand.ExecuteScalarAsync();

                    if (userId != null)
                    {
                        await UpdateUserIdAsync((int)userId);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                #region Helper db methods

                async Task UpdateUserIdAsync(int userId)
                {
                    var insertUserIdQuery = "UPDATE TelegramIntegration SET UserId = @UserId WHERE ChatId = @ChatId";
                    using (var insertUserIdCommand = CreateCommand(insertUserIdQuery, connection))
                    {
                        insertUserIdCommand.Parameters.Add(CreateParameter("UserId",userId,DbType.Int32));
                        insertUserIdCommand.Parameters.Add(CreateParameter("ChatId",chatId,DbType.Int64));
                        await insertUserIdCommand.ExecuteScalarAsync();
                    }
                }
                #endregion
            }
        }

        #endregion

        #region Factory

        private DbConnection CreateConnection()
        {
            var command = factory.CreateConnection();
            command.ConnectionString = connectionString;
            return command;
        }

        private DbCommand CreateCommand(string query, DbConnection connection) {
            var command = factory.CreateCommand();
            command.CommandText = query;
            command.Connection = connection;
            return command;
        }

        private DbParameter CreateParameter(string name, object value, DbType type = DbType.String)
        {
            var param = factory.CreateParameter();
            param.ParameterName = name;
            param.Value = value;
            param.DbType = type;
            return param;
        }

        #endregion

    }
}
