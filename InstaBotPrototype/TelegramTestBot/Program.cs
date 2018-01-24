using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Example
{
    class Program
    {
        // Bot username: @isdTestBot
        private static string connectionString;
        private static string telegramApiKey;
        private static readonly TelegramBotClient bot;

        private delegate void BotAction(Message message);
        private static readonly Dictionary<BotCommand, BotAction> botAction;
        private static readonly Dictionary<string, AccessModifier> commandDict;

        static Program()
        {
            telegramApiKey = "449290937:AAHERJRQiQCq4nm1fB-8rFJFFIxeWR57Yc8";
            bot = new TelegramBotClient(telegramApiKey);

            commandDict = new Dictionary<string, AccessModifier>()
            {
                { "/start", AccessModifier.Public },
                { "/menu", AccessModifier.Verified },
                { "/help", AccessModifier.Public }
            };

            botAction = new Dictionary<BotCommand, BotAction>(new BotCommandEqualityComparer())
            {
                { new BotCommand { Command = "/start", Access = AccessModifier.Public }, DialogStart },
                { new BotCommand { Command = "/menu", Access = AccessModifier.Verified }, ShowMenu },
                { new BotCommand { Command = "/help", Access = AccessModifier.Public }, ShowHelp }
            };
        }

        static void Main()
        {
            try
            {
                connectionString = ConfigurationManager.ConnectionStrings[1].ConnectionString;

                bot.OnMessage += BotOnMessageReceived;
                bot.OnReceiveError += BotOnReceiveError;

                Console.WriteLine("Bot is waiting for messages...");

                bot.StartReceiving();
                Console.ReadLine();
                bot.StopReceiving();
            }
            catch (DbException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        #region Bot commands

        private static async void ShowHelp(Message message) => await bot.SendTextMessageAsync(message.Chat.Id, "Hi there! This bot helps you getting Instagram photos.");

        private static async void DialogStart(Message message)
        {
            var isNewUser = await AddUser(message);

            var answer = isNewUser
                ? $"Hello, {message.Chat.FirstName}!"
                : $"I'm glad to see you again, {message.Chat.FirstName}";

            await bot.SendTextMessageAsync(message.Chat.Id, answer);
        }

        private static async void ShowMenu(Message message)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
                {
                    new []
                    {
                        new KeyboardButton("/privateOption1"),
                        new KeyboardButton("/privateOption2")
                    },
                    new []
                    {
                        new KeyboardButton("/privateOption3"),
                        new KeyboardButton("/privateOption4")
                    }
                });

            await bot.SendTextMessageAsync(message.Chat.Id, "Choose button",
                replyMarkup: keyboard);
        }

        private static async void DeleteUser(Message message)
        {
            var answer = await DeleteUser((int)message.Chat.Id)
                ? "Deleted "
                : "You have been already deleted";
            await bot.SendTextMessageAsync(message.Chat.Id, answer);
        }

        #endregion

        #region Helper methods

        private static async void SendMessage(long chatId, string text) => await bot.SendTextMessageAsync(chatId, text);

        #endregion

        #region Wrappers

        private static async void VerifyCommand(string telegramText, long chatId)
        {
            if (long.TryParse(telegramText, out var telegramVerificationKey))
            {
                if (await Verify(telegramVerificationKey, chatId))
                {
                    SendMessage(chatId, "Congratulations! You have been successfully verified");
                }
                else
                {
                    SendMessage(chatId, "Please, enter correct verification code.\nThis code is on your web page");
                }
            }
            else
            {
                SendMessage(chatId, "Please, enter correct verification code");
            }
        }

        #endregion

        #region DB methods

        private static async Task<string> GetUsers()
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

        private static async Task<bool> UserExists(int chatId, SqlConnection connection)
        {
            var checkExistsQuery = "SELECT COUNT(ChatId) FROM TelegramIntegration WHERE ChatId = @ChatId";
            using (var checkExistsCmd = new SqlCommand(checkExistsQuery, connection))
            {
                checkExistsCmd.Parameters.Add(new SqlParameter() { ParameterName = "@ChatId", SqlDbType = System.Data.SqlDbType.Int, Value = chatId });
                return Convert.ToBoolean(await checkExistsCmd.ExecuteScalarAsync());
            }
        }

        private static async Task<bool> AddUser(Message message)
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

        private static async Task<bool> DeleteUser(int chatId)
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

        private static async Task<bool> CheckVerification(long chatId)
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

        private static async Task<bool> Verify(long telegramVerificationKey, long chatId)
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

        #region Bot

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            try
            {
                var message = messageEventArgs.Message;
                if (message == null || message.Type != MessageType.TextMessage)
                {
                    await bot.SendTextMessageAsync(message.Chat.Id, "Write the correct command 👀");
                    return;
                }

                var isVerified = await CheckVerification(message.Chat.Id);

                var command = new BotCommand() { Command = message.Text };


                if (commandDict.ContainsKey(command.Command))
                {
                    command.Access = commandDict[command.Command];
                }

                botAction.TryGetValue(command, out var currentAction);

                if (command.Access == AccessModifier.Public && currentAction != null || isVerified)
                {
                    if (currentAction != null)
                    {
                        currentAction.Invoke(message);
                    }
                    else
                    {
                        SendMessage(message.Chat.Id, "Please, enter one of the available commands");
                    }
                }
                else
                {
                    VerifyCommand(message.Text, message.Chat.Id);
                }

                // Debug
                Console.WriteLine($"Message: {message.Text} from {message.Chat.FirstName} {message.Chat.LastName}");
                Console.WriteLine($"Username: {message.Chat.Username}");
                Console.WriteLine($"ChatId: {message.Chat.Id}");
                Console.WriteLine("---------------------------------");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs) => Debugger.Break();

        #endregion
    }
}