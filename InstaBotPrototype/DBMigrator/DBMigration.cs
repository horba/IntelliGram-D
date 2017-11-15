using System;
using System.Data.Common;

namespace DBMigrator
{
    internal class DBMigration
    {
        #region Public Constructors

        public DBMigration(DbCommand applyCommand, DbCommand reverseCommand, string name)
        {
            ApplyCommand = applyCommand;
            ReverseCommand = reverseCommand;
            Name = name;
        }

        #endregion Public Constructors

        #region Public Properties

        public DbCommand ApplyCommand { get; private set; }
        public string ConnectionString { get; set; }
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
            command.CommandText = $"INSERT INTO dbo.Migrations (Name, ApplyTime) VALUES ('{Name}', GetDate())";

            Run(command);
        }

        private void RemoveLog()
        {
            DbCommand command = Factory.CreateCommand();
            command.CommandText = $"delete from dbo.Migrations where Name='{Name}'";

            Run(command);
        }

        private bool Run(DbCommand command)
        {
            bool correct = true;
            DbConnection dbConnection = Factory.CreateConnection();
            dbConnection.ConnectionString = ConnectionString;

            command.Connection = dbConnection;

            try
            {
                dbConnection.Open();
                command.ExecuteNonQuery();
            }
            catch
            {
                correct = false;
            }
            finally
            {
                dbConnection?.Close();
            }
            return correct;
        }

        #endregion Private Methods
    }
}