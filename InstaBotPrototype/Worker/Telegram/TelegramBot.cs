using InstaBotPrototype;
using InstaBotPrototype.Services.Instagram;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Worker
{
    public class TelegramBot
    {
        // Bot username: IntelliGram
        private static string telegramApiKey;
        private static readonly TelegramBotClient bot;
        private static readonly TelegramDb telegramDb;
        private static readonly InstagramService instagram;

        private delegate void BotAction(Message message);
        private static readonly Dictionary<BotCommand, BotAction> botAction;
        private static readonly Dictionary<string, AccessModifier> commandDict;

        private const int COMMENT_MAX_LENGTH = 300;

        #region Static Constructor

        static TelegramBot()
        {
            try
            {
                telegramApiKey = AppSettingsProvider.Config["telegramApiKey"];
                bot = new TelegramBotClient(telegramApiKey);

                telegramDb = new TelegramDb(AppSettingsProvider.Config["connectionString"]);
                instagram = new InstagramService();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            commandDict = new Dictionary<string, AccessModifier>()
            {
                { "/start",   AccessModifier.Public },
                { "/menu",    AccessModifier.Verified },
                { "/help",    AccessModifier.Public },
                { "/mute",    AccessModifier.Verified },
                { "/unmute",  AccessModifier.Verified },
                { "/like",    AccessModifier.Verified },
                { "/comment", AccessModifier.Verified }
            };

            botAction = new Dictionary<BotCommand, BotAction>(new BotCommandEqualityComparer())
            {
                { new BotCommand { Command = "/start",   Access = AccessModifier.Public   }, DialogStartAsync },
                { new BotCommand { Command = "/menu",    Access = AccessModifier.Verified }, ShowMenuAsync },
                { new BotCommand { Command = "/help",    Access = AccessModifier.Public   }, ShowHelpAsync },
                { new BotCommand { Command = "/mute",    Access = AccessModifier.Verified }, MuteNotificationsAsync },
                { new BotCommand { Command = "/unmute",  Access = AccessModifier.Verified }, UnmuteNotificationsAsync },
                { new BotCommand { Command = "/like",    Access = AccessModifier.Verified }, LikeAsync },
                { new BotCommand { Command = "/comment", Access = AccessModifier.Verified }, CommentAsync }

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

                bot.StartReceiving();
                Console.WriteLine("Bot is waiting for messages...");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        #endregion

        #region Bot commands

        private static async void LikeAsync(Message message)
        {
            // Initialize current user
            string token = telegramDb.GetUsersToken(message.Chat.Id);
            string username = instagram.GetUsername(token);
            string userId = instagram.GetUserId(username);
            string postId = telegramDb.GetLatestPostId(message.Chat.Id);

            instagram.Like(postId);
        }

        private static async void CommentAsync(Message message)
        {
            // Initialize current user
            string token = telegramDb.GetUsersToken(message.Chat.Id);
            string username = instagram.GetUsername(token);
            string userId = instagram.GetUserId(username);
            string postId = telegramDb.GetLatestPostId(message.Chat.Id);

            var args = message.Text.Split(' ');
            if (args.Length > 1)
            {
                if (args[1].Length < COMMENT_MAX_LENGTH)
                {
                    instagram.Comment(postId, args[1]);
                }
                else
                {
                    await SendMessageAsync(message.Chat.Id, $"Comment must be shorter than { COMMENT_MAX_LENGTH } symbols");
                }
            }
            else
            {
                instagram.Comment(postId);
            }           
        }

        private static async void MuteNotificationsAsync(Message message)
        {
            bool isMuted = await telegramDb.IsMutedAsync(message.Chat.Id);

            if (isMuted)
            {
                await SendMessageAsync(message.Chat.Id, "You are unmuted");
            }
            else
            {
                await SendMessageAsync(message.Chat.Id, "You have already been muted");
            }

            // Invert Muted state
            await telegramDb.SetNotificationAsync(message.Chat.Id, !isMuted);
        }

        private static async void UnmuteNotificationsAsync(Message message)
        {
            if (await telegramDb.IsMutedAsync(message.Chat.Id))
            {
                await telegramDb.SetNotificationAsync(message.Chat.Id, false);
                await SendMessageAsync(message.Chat.Id, "You are unmuted");
            }
            else
            {
                await SendMessageAsync(message.Chat.Id, "You have already been unmuted");
            }
        }

        private static async void ShowHelpAsync(Message message) => await Task.Run(() => SendMessageAsync(message.Chat.Id, "Hi there! This bot helps you getting Instagram photos."));

        private static async void DialogStartAsync(Message message)
        {
            var isNewUser = await telegramDb.AddUserAsync(message);

            var answer = isNewUser
                ? $"Hello, {message.Chat.FirstName}!"
                : $"I'm glad to see you again, {message.Chat.FirstName}";

            await bot.SendTextMessageAsync(message.Chat.Id, answer);
        }

        private static async void ShowMenuAsync(Message message)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
                {
                    new []
                    {
                        new KeyboardButton("/like"),
                        new KeyboardButton("/comment")
                    },
                    new []
                    {
                        new KeyboardButton("/mute"),
                        new KeyboardButton("/unmute")
                    }
                });

            await bot.SendTextMessageAsync(message.Chat.Id, "Choose button",
                replyMarkup: keyboard);
        }

        private static async void DeleteUserAsync(Message message)
        {
            var answer = await telegramDb.DeleteUserAsync((int)message.Chat.Id)
                ? "Deleted "
                : "You have been already deleted";
            await bot.SendTextMessageAsync(message.Chat.Id, answer);
        }

        #endregion

        #region Helper methods

        public static async Task SendMessageAsync(long chatId, string text) => await Task.Run(() => bot.SendTextMessageAsync(chatId, text));

        public static void SendMessage(long chatId, string text) => bot.SendTextMessageAsync(chatId, text);

        #endregion

        #region Wrappers

        private static async void VerifyCommandAsync(string telegramText, long chatId)
        {
            if (long.TryParse(telegramText, out var telegramVerificationKey))
            {
                if (await telegramDb.VerifyAsync(telegramVerificationKey, chatId))
                {
                    await SendMessageAsync(chatId, "Congratulations! You have been successfully verified");
                }
                else
                {
                    await SendMessageAsync(chatId, "Please, enter correct verification code.\nThis code is on your web page");
                }
            }
            else
            {
                await SendMessageAsync(chatId, "Please, enter correct verification code");
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

                var isVerified = await telegramDb.CheckVerificationAsync(message.Chat.Id);

                // Get command's name
                var command = new BotCommand() { Command = message.Text.Split(' ')[0] };

                if (commandDict.ContainsKey(command.Command))
                {
                    command.Access = commandDict[command.Command];
                }

                botAction.TryGetValue(command, out var currentAction);

                if (command.Access == AccessModifier.Public && currentAction != null || isVerified)
                {
                    if (currentAction != null)
                    {
                        currentAction(message);
                    }
                    else
                    {
                        await SendMessageAsync(message.Chat.Id, "Please, enter one of the available commands");
                    }
                }
                else
                {
                    VerifyCommandAsync(message.Text, message.Chat.Id);
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

