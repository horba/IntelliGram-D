using System;
using System.Data.Common;

namespace DBMigrator
{
    internal class DBMigration
    {
        #region Public Constructors

        public DBMigration(DbCommand applyCommand, DbCommand reverseCommand, string name, DbConnection connection = null)
        {
            ApplyCommand = applyCommand;
            ReverseCommand = reverseCommand;
            Name = name;
            Connection = connection;
        }

        #endregion Public Constructors

        #region Public Properties

        public DbCommand ApplyCommand { get; private set; }
        public DbConnection Connection { get; set; }
        public DbProviderFactory Factory { get; set; }
        public bool IsApplied { get; set; }
        public string Name { get; private set; }
        public DbCommand ReverseCommand { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public bool Apply()
        {
            IsApplied = Run(ApplyCommand);

            if (IsApplied)
            {
                AddLog();
            }

            return IsApplied;
        }

        public bool Reverse()
        {
            IsApplied = !Run(ReverseCommand);

            if (!IsApplied)
            {
                RemoveLog();
            }

            return IsApplied;
        }

        #endregion Public Methods

        #region Private Methods

        private void AddLog()
        {
            DbCommand command = Factory.CreateCommand();
            command.Connection = Connection;
            command.CommandText = $"insert into dbo.Migrations VALUES({Name}, {DateTime.Now});";

            command.ExecuteNonQuery();
        }

        private void RemoveLog() => throw new NotImplementedException(); //todo: delete log entry

        private bool Run(DbCommand dbCommand)
        {
            bool correct = true;
            try
            {
                Connection.Open();
                dbCommand.ExecuteNonQuery();
            }
            catch
            {
                correct = false;
            }
            finally
            {
                Connection?.Close();
            }
            return correct;
        }

        #endregion Private Methods
    }
}