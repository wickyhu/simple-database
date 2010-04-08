using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Reflection;
using IQToolkit.Data;

namespace SimpleDatabase
{
    public partial class Db
    {
        public bool IsTransactionReady
        {
            get
            {
                return DbEntityProvider.Transaction != null;
                
            }
        }

        public IsolationLevel Isolation
        {
            get { return DbEntityProvider.Isolation; }
            set { DbEntityProvider.Isolation = value; }
        }

        public DbTransactionInfo BeginTransaction()
        {
            return BeginTransaction(DbEntityProvider.Isolation);
        }

        public DbTransactionInfo BeginTransaction(IsolationLevel isolation)
        {
            return new DbTransactionInfo(this, isolation);
        }

        internal void StartUsingConnection()
        {
            typeof(DbEntityProvider).InvokeMember("StartUsingConnection",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod,
                null,
                this.DbEntityProvider,
                null);
        }

        internal void StopUsingConnection()
        {
            typeof(DbEntityProvider).InvokeMember("StopUsingConnection",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod,
                null,
                this.DbEntityProvider,
                null);
        }

        public int ConnectedActions
        {
            get
            {
                return (int)typeof(DbEntityProvider).InvokeMember("nConnectedActions",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField,
                null,
                this.DbEntityProvider,
                null);
            }
        }
        

    }//end of class
}
