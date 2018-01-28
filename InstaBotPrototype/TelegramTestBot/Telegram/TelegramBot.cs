using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramTestBot
{
    class TelegramBot
    {
        // Bot username: IntelliGram
        private static string telegramApiKey;
        private static readonly TelegramBotClient bot;
        private static readonly TelegramDb telegramDb;

        private delegate void BotAction(Message message);
        private static readonly Dictionary<BotCommand, BotAction> botAction;
        private static readonly Dictionary<string, AccessModifier> commandDict;

        #region Static Constructor

        static TelegramBot()
        {
            try
            {
                telegramApiKey = ConfigurationManager.AppSettings["TelegramApiKey"];
                bot = new TelegramBotClient(telegramApiKey);

                telegramDb = new TelegramDb(ConfigurationManager.ConnectionStrings[1].ConnectionString);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

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

        #endregion

        #region Run method

        public void Run()
        {
            try
            {
                bot.OnMessage += BotOnMessageReceived;
                bot.OnReceiveError += BotOnReceiveError;

                Console.WriteLine("Bot is waiting for messages...");

                bot.StartReceiving();
                //Console.ReadLine();
                //bot.StopReceiving();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        #endregion

        #region Bot commands

        private static async void ShowHelp(Message message) => await Task.Run(() => SendMessage(message.Chat.Id, "Hi there! This bot helps you getting Instagram photos."));

        private static async void DialogStart(Message message)
        {
            var isNewUser = await telegramDb.AddUser(message);

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
            var answer = await telegramDb.DeleteUser((int)message.Chat.Id)
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
                if (await telegramDb.Verify(telegramVerificationKey, chatId))
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

                var isVerified = await telegramDb.CheckVerification(message.Chat.Id);

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
