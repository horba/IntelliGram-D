using System;
using System.Collections.Generic;
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
        private static string connectionString = "Server=DESKTOP-2J18FL2\\SQLEXPRESS;Database=Telegram;Trusted_Connection=True;";
        private static string telegramApiKey = "449290937:AAHERJRQiQCq4nm1fB-8rFJFFIxeWR57Yc8";
        private static readonly TelegramBotClient Bot = new TelegramBotClient(telegramApiKey);

        private delegate void BotAction(Message message);
        private static Dictionary<string, BotAction> botAction = new Dictionary<string, BotAction>()
        {
            { "/start", StartDialog },
            {"/menu", ShowMenu },
            {"/showRandomNumber", ShowRandomNumber },
            {"/showUsers", ShowUsers },
            {"/delete", DeleteUser },
            {"/addToDb", AddUser }
        };

        static void Main(string[] args)
        {
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            Console.WriteLine("Bot starts here...");

            Bot.StartReceiving();
            Console.ReadLine();
            Bot.StopReceiving();
        }

        #region Bot commands

        private static async void ShowUsers(Message message)
        {
            await Bot.SendTextMessageAsync(message.Chat.Id, await GetUsers());
        }

        private static async void ShowRandomNumber(Message message)
        {
            var random = new Random();
            await Bot.SendTextMessageAsync(message.Chat.Id, $"Your number: {random.Next()} :)");
        }

        private static async void StartDialog(Message message)
        {
            await Bot.SendTextMessageAsync(message.Chat.Id, $"Hello, {message.Chat.FirstName}!");
            if (message.Chat.FirstName != null)
            {
                AddUser(message);
            }
        }

        private static async void ShowMenu(Message message)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
                {
                    new []
                    {
                        new KeyboardButton("/showUsers"),
                        new KeyboardButton("/showRandomNumber")
                    },
                    new []
                    {
                        new KeyboardButton("/delete"),
                        new KeyboardButton("/addToDb")
                    }
                });

            await Bot.SendTextMessageAsync(message.Chat.Id, "Choose button",
                replyMarkup: keyboard);
        }

        private static async void AddUser(Message message)
        {
            bool isNewUser = await AddUser((int)message.Chat.Id, message.Chat.FirstName, message.Chat.LastName);
            string answer = isNewUser
                ? $"Successfully added to the database, {message.Chat.FirstName}"
                : $"You are already in the database, {message.Chat.FirstName}";
            await Bot.SendTextMessageAsync(message.Chat.Id, answer);
        }

        private static async void DeleteUser(Message message)
        {
            string answer = await DeleteUser((int)message.Chat.Id) 
                ? "Deleted " 
                : "You have been already deleted";
            await Bot.SendTextMessageAsync(message.Chat.Id, answer);
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
                    string getUsersQuery = "SELECT * FROM Users";
                    using (var getUsersCmd = new SqlCommand(getUsersQuery, connection))
                    {
                        string users = String.Empty;
                        var reader = await getUsersCmd.ExecuteReaderAsync();
                        if (reader.HasRows)
                        {
                            string currentLastName = "";
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
            string checkExistsQuery = "SELECT COUNT(ChatID) FROM Users WHERE ChatID = @ChatID";
            using (var checkExistsCmd = new SqlCommand(checkExistsQuery, connection))
            {
                checkExistsCmd.Parameters.Add(new SqlParameter() { ParameterName = "@ChatID", SqlDbType = System.Data.SqlDbType.Int, Value = chatId });
                return Convert.ToBoolean(await checkExistsCmd.ExecuteScalarAsync());
            }
        }

        private static async Task<bool> AddUser(int chatId, string fisrtName, string lastName)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                if (!await UserExists(chatId, connection))
                {
                    string addQuery =
                        "INSERT INTO Users " +
                        "(ChatID, FirstName, LastName) " +
                        "VALUES(@ChatID, @FirstName, @LastName)";

                    using (var addCmd = new SqlCommand(addQuery, connection))
                    {
                        addCmd.Parameters.Add(new SqlParameter() { ParameterName = "@ChatID", SqlDbType = System.Data.SqlDbType.Int, Value = chatId });
                        addCmd.Parameters.Add(new SqlParameter() { ParameterName = "@FirstName", SqlDbType = System.Data.SqlDbType.NVarChar, Value = fisrtName });
                        addCmd.Parameters.Add(new SqlParameter() { ParameterName = "@LastName", SqlDbType = System.Data.SqlDbType.NVarChar, Value = (object)lastName ?? DBNull.Value });
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
                    string deleteQuery = "DELETE FROM Users WHERE ChatID = @ChatID";
                    using (var deleteCmd = new SqlCommand(deleteQuery, connection))
                    {
                        deleteCmd.Parameters.Add(new SqlParameter() { ParameterName = "@ChatID", SqlDbType = System.Data.SqlDbType.Int, Value = chatId });
                        await deleteCmd.ExecuteScalarAsync();
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

        #region Bot

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            try
            {
                var message = messageEventArgs.Message;
                if (message == null)
                {
                    return;
                }
                else if (message.Type != MessageType.TextMessage)
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, "👀");
                    return;
                }

                botAction.TryGetValue(message.Text, out BotAction currentAction);
                if (currentAction != null)
                {
                    currentAction.Invoke(message);
                }
                else
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Cool message bro!");
                }

                // Debug
                Console.WriteLine($"Message: {message.Text} from {message.Chat.FirstName} {message.Chat.LastName}");
                Console.WriteLine($"Username: {message.Chat.Username}");
                Console.WriteLine($"ChatId: {message.Chat.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Debugger.Break();
        }

        #endregion
    }
}