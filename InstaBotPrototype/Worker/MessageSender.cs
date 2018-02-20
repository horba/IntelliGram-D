using InstaBotPrototype;
using InstaBotPrototype.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Worker
{
    class MessageSender
    {
        string connString = AppSettingsProvider.Config["connectionString"];
        TelegramBot Bot { get; set; }

        public MessageSender(TelegramBot bot) => Bot = bot ?? throw new ArgumentNullException(nameof(bot));

        public void Start()
        {

            var dbConnection = new SqlConnection();
            dbConnection.ConnectionString = connString;

            dbConnection.Open();

            var getMsgCmd = new SqlCommand();
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


                    var setDateCommand = new SqlCommand();
                    setDateCommand.Connection = dbConnection;
                    setDateCommand.CommandText = "UPDATE Messages SET Send = GETDATE() WHERE Send IS NULL AND Id = @Id;";
                    var idParam = new SqlParameter();
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