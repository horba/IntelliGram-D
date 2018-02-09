using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramTestBot;
using InstaBotPrototype.Models;

namespace MessageSender
{
    class MessageSender
    {
        string connString = ConfigurationManager.ConnectionStrings[1].ConnectionString;
        string provider = ConfigurationManager.ConnectionStrings[1].ProviderName;
        TelegramBot bot;
        public void SendMessages()
        {
            var factory = DbProviderFactories.GetFactory(provider);
            var dbConnection = factory.CreateConnection();
            dbConnection.ConnectionString = connString;

            dbConnection.Open();
            bot = new TelegramBot();
            bot.Run();

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

                    //var chatIdParam = factory.CreateParameter();
                    //chatIdParam.ParameterName = "@ChatId";
                    //chatIdParam.Value = chatId;
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
