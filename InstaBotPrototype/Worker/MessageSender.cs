using InstaBotPrototype;
using InstaBotPrototype.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Worker
{
    class MessageSender
    {
        private string connString = AppSettingsProvider.Config["connectionString"];
        public TelegramBot Bot { get; private set; }
        private DbProviderFactory factory = DbProviderFactories.GetFactoryByProvider(AppSettingsProvider.Config["dataProvider"]);
        public MessageSender(TelegramBot bot) => Bot = bot ?? throw new ArgumentNullException(nameof(bot));

        public void Start()
        {

            var dbConnection = factory.CreateConnection();
            dbConnection.ConnectionString = connString;

            dbConnection.Open();

            var getMsgCmd = factory.CreateCommand();
            getMsgCmd.Connection = dbConnection;
            getMsgCmd.CommandText = "SELECT * FROM Messages WHERE Send IS NULL ORDER BY Timestamp;";
            var reader = getMsgCmd.ExecuteReader();

            List<Message> messages = new List<Message>();


            while (reader.Read())
            {
                var id = Convert.ToInt32(reader["Id"]);
                var chatId = Convert.ToInt64(reader["ChatId"]);
                var message = Convert.ToString(reader["Message"]);
                messages.Add(new Message(id, chatId, message));
            }
            reader.Close();

            foreach (var m in messages)
            {
                try
                {
                    TelegramBot.SendMessageAsync(m.ChatId, m.Text);


                    var setDateCommand = factory.CreateCommand();
                    setDateCommand.Connection = dbConnection;
                    setDateCommand.CommandText = "UPDATE Messages SET Send = GETDATE() WHERE Send IS NULL AND Id = @Id;";
                    var idParam = factory.CreateParameter();
                    idParam.ParameterName = "@Id";
                    idParam.Value = m.Id;

                    setDateCommand.Parameters.AddRange(new[] { idParam });
                    setDateCommand.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Something went wrong during sending the message to the user with chatId:{0}", m.ChatId);
                }

            }

            dbConnection.Close();

            Console.ReadKey();

        }
    }
}