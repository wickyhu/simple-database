using System;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Configuration;
using IQToolkit;
using IQToolkit.Data;
using IQToolkit.Data.Common;

namespace SimpleDatabase
{
    public partial class Db : IDisposable
    {
        public static Db Get(string providerInvariantName, string connectionString)
        {
            return new Db(providerInvariantName, connectionString);
        }

        public static Db Get(string configName)
        {
            string providerInvariantName = ConfigurationManager.ConnectionStrings[configName].ProviderName;
            string connectionString = ConfigurationManager.ConnectionStrings[configName].ConnectionString;
            Db db = Db.Get(providerInvariantName, connectionString);
            return db;
        }

        private Db(string providerInvariantName, string connectionString)
        {
            ProviderInvariantName = providerInvariantName;
            ConnectionString = connectionString;
            if (String.IsNullOrEmpty(ProviderInvariantName))
                throw new ArgumentNullException("providerInvariantName");
            if (String.IsNullOrEmpty(ConnectionString))
                throw new ArgumentNullException("connectionString");

            DbFactory = DbProviderFactories.GetFactory(ProviderInvariantName);
            CommandTimeout = DefaultCommandTimeout;

            DbEntityProvider = CreateEntityProvider();
        }

        #region overrides 
        public override int GetHashCode()
        {
            return ConnectionString.GetHashCode();
        }

        public override string ToString()
        {
            return ConnectionString;
        }

        public override bool Equals(object obj)
        {
            Db db = obj as Db;
            if (null == db) return false;
            return db.ConnectionString.Equals(this.ConnectionString, StringComparison.OrdinalIgnoreCase);
        }
        #endregion overrides

        #region create & release
        DbEntityProvider CreateEntityProvider()
        {
            Type providerType = DbMetadata.QueryProviderType;

            if (providerType != null)
            {
                DbConnection cn = DbFactory.CreateConnection();
                cn.ConnectionString = ConnectionString;

                return (DbEntityProvider)Activator.CreateInstance(
                    providerType,
                    new object[] { 
                        cn, 
                        new SimpleMapping(this),
                        new SimplePolicy(this)
                        }
                    );
            }
            return null;
        }

        public DbCommand CreateCommand()
        {
            DbCommand cm = this.DbEntityProvider.Connection.CreateCommand();
            cm.CommandTimeout = this.CommandTimeout;
            cm.CommandType = CommandType.Text;
            if (IsTransactionReady && cm.Transaction == null)
            {
                cm.Transaction = this.DbEntityProvider.Transaction;
            }
            return cm;
        }

        public DbConnectionInfo CreateConnection()
        {
            return new DbConnectionInfo(this);
        }

        public DbConnection CreateAdoConnection()
        {
            DbConnection cn = DbFactory.CreateConnection();
            cn.ConnectionString = this.ConnectionString;
            cn.Open();
            return cn;
        }

        private DbDataAdapter CreateDataAdapter()
        {
            DbDataAdapter da = DbFactory.CreateDataAdapter();
            return da;
        }
        
        #endregion create & release

        #region IDisposable Members

        public void Dispose()
        {
            int count = ConnectedActions;
            while (count > 0)
            {
                StopUsingConnection();
                count--;
            }
        }

        #endregion IDisposable Members 

    }
}
