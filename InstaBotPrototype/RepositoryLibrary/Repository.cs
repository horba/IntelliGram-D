using System;
using System.Configuration;
using System.Data.Common;

namespace RepositoryLibrary
{
    public abstract class Repository<TKey, TValue> : IDisposable, IRepository<TKey, TValue>
    {
        public abstract void Add(TValue model);
        public abstract void Delete(TKey id);
        public abstract TValue Get(TKey id);
        public abstract void Update(TValue model);

        DbProviderFactory factory = DbProviderFactories.GetFactory(ConfigurationManager.ConnectionStrings[1].ProviderName);
        DbConnection connection;

        public Repository()
        {
            connection = factory.CreateConnection();
            connection.ConnectionString = ConfigurationManager.ConnectionStrings[1].ConnectionString;
            connection.Open();
        }

        bool disposed = false;

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                connection.Close();
            }
        }

        ~Repository()
        {
            if (!disposed)
            {
                Dispose();
            }
        }

        protected DbCommand CreateCommand(string text, params DbParameter[] args)
        {
            var command = factory.CreateCommand();
            command.Connection = connection;
            command.Parameters.AddRange(args);
            return command;
        }

        protected DbParameter CreateParameter(string name, object value)
        {
            var parameter = factory.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            return parameter;
        }
    }
}