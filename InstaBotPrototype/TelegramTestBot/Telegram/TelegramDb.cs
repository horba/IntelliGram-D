using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TelegramTestBot
{
    class TelegramDb
    {
        private string connectionString;
          
        public TelegramDb(string connectionStr)
        {
            connectionString = connectionStr;
        }

        #region DB methods

        public async Task<string> GetUsers()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var getUsersQuery = "SELECT * FROM TelegramIntegration";
                    using (var getUsersCmd = new SqlCommand(getUsersQuery, connection))
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

        public async Task<bool> UserExists(int chatId, SqlConnection connection)
        {
            var checkExistsQuery = "SELECT COUNT(ChatId) FROM TelegramIntegration WHERE ChatId = @ChatId";
            using (var checkExistsCmd = new SqlCommand(checkExistsQuery, connection))
            {
                checkExistsCmd.Parameters.Add(new SqlParameter() { ParameterName = "@ChatId", SqlDbType = System.Data.SqlDbType.Int, Value = chatId });
                return Convert.ToBoolean(await checkExistsCmd.ExecuteScalarAsync());
            }
        }

        public async Task<bool> AddUser(Message message)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                if (!await UserExists((int)message.Chat.Id, connection))
                {
                    var addQuery =
                        "INSERT INTO TelegramIntegration " +
                        "(ChatId, FirstName, LastName, UserId) " +
                        "VALUES(@ChatId, @FirstName, @LastName, @UserId)";

                    using (var addCmd = new SqlCommand(addQuery, connection))
                    {
                        addCmd.Parameters.Add(new SqlParameter() { ParameterName = "@ChatId", SqlDbType = System.Data.SqlDbType.Int, Value = message.Chat.Id });
                        addCmd.Parameters.Add(new SqlParameter() { ParameterName = "@FirstName", SqlDbType = System.Data.SqlDbType.NVarChar, Value = message.Chat.FirstName });
                        // Last name can be null
                        addCmd.Parameters.Add(new SqlParameter() { ParameterName = "@LastName", SqlDbType = System.Data.SqlDbType.NVarChar, Value = (object)message.Chat.LastName ?? DBNull.Value });
                        // By default it sets to null
                        addCmd.Parameters.Add(new SqlParameter() { ParameterName = "@UserId", SqlDbType = System.Data.SqlDbType.Int, Value = DBNull.Value });
                        await addCmd.ExecuteNonQueryAsync();
                        return true;
                    }
                }
                return false;
            }
        }

        public async Task<bool> DeleteUser(int chatId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                if (await UserExists(chatId, connection))
                {
                    var deleteQuery = "DELETE FROM TelegramIntegration WHERE ChatId = @ChatId";
                    using (var deleteCmd = new SqlCommand(deleteQuery, connection))
                    {
                        deleteCmd.Parameters.Add(new SqlParameter() { ParameterName = "@ChatId", SqlDbType = System.Data.SqlDbType.Int, Value = chatId });
                        await deleteCmd.ExecuteScalarAsync();
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<bool> CheckVerification(long chatId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var checkVerificationQuery = "SELECT UserId FROM TelegramIntegration WHERE ChatId = @ChatId";
                using (var checkVerificationCmd = new SqlCommand(checkVerificationQuery, connection))
                {
                    checkVerificationCmd.Parameters.Add(new SqlParameter() { ParameterName = "@ChatId", Value = chatId });
                    return Convert.ToBoolean(await checkVerificationCmd.ExecuteScalarAsync() != DBNull.Value);
                }
            }
        }

        public async Task<bool> Verify(long telegramVerificationKey, long chatId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var checkVerificationKeyQuery =
                    "SELECT UserId " +
                    "FROM TelegramVerification " +
                    "WHERE TelegramVerificationKey = @TelegramVerificationKey";

                using (var verificationCommand = new SqlCommand(checkVerificationKeyQuery, connection))
                {
                    verificationCommand.Parameters.Add(new SqlParameter { ParameterName = "@TelegramVerificationKey", Value = telegramVerificationKey });
                    var userId = await verificationCommand.ExecuteScalarAsync();

                    if (userId != null)
                    {
                        await UpdateUserId((int)userId);
                        await DeleteVerificationRecord();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                #region Helper db methods

                async Task UpdateUserId(int userId)
                {
                    var insertUserIdQuery = "UPDATE TelegramIntegration SET UserId = @UserId WHERE ChatId = @ChatId";

                    using (var insertUserIdCommand = new SqlCommand(insertUserIdQuery, connection))
                    {
                        insertUserIdCommand.Parameters.Add(new SqlParameter { ParameterName = "UserId", Value = userId });
                        insertUserIdCommand.Parameters.Add(new SqlParameter { ParameterName = "ChatId", Value = chatId });
                        await insertUserIdCommand.ExecuteScalarAsync();
                    }
                }

                async Task DeleteVerificationRecord()
                {
                    var deleteVerificationQuery = "DELETE FROM TelegramVerification WHERE TelegramVerificationKey = @TelegramVerificationKey";

                    using (var deleteVerificationCommand = new SqlCommand(deleteVerificationQuery, connection))
                    {
                        deleteVerificationCommand.Parameters.Add(new SqlParameter { ParameterName = "@TelegramVerificationKey", Value = telegramVerificationKey });
                        await deleteVerificationCommand.ExecuteScalarAsync();
                    }
                }

                #endregion

            }
        }

        #endregion
    }
}
