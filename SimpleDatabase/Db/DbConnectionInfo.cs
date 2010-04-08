using System;
using System.Text;
using System.Data;
using System.Data.Common;
using SimpleControls;

namespace SimpleDatabase
{
    //[Serializable]
    public class DbConnectionInfo : IDisposable
    {
        public Db Db { get; private set; }

        public DbConnectionInfo(Db db)
        {
            Db = db;
            Db.StartUsingConnection();
        }

        public string Database
        {
            get
            {
                return Db.DbEntityProvider.Connection.Database;
            }
        }

        public DbCommand CreateCommand()
        {
            return Db.CreateCommand();
        }

        public DbTransactionInfo BeginTransaction()
        {
            return Db.BeginTransaction();
        }

        public DbTransactionInfo BeginTransaction(IsolationLevel isolation)
        {
            return Db.BeginTransaction(isolation);
        }

        public DataTable GetSchema(string collectionName)
        {
            return Db.DbEntityProvider.Connection.GetSchema(collectionName);
        }

        public DataTable GetSchema(string collectionName, string[] restrictionValues)
        {
            return Db.DbEntityProvider.Connection.GetSchema(collectionName, restrictionValues);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Db.StopUsingConnection();   
        }

        #endregion

    } //end of DbConnectionInfo

}

