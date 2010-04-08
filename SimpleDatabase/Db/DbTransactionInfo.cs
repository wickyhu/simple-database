using System;
using System.Text;
using System.Data;
using System.Data.Common;
using SimpleControls;

namespace SimpleDatabase
{
    //[Serializable]
    public class DbTransactionInfo : IDisposable
    {
        public Db Db { get; private set; }

        public DbTransactionInfo(Db db)
            : this(db, db.Isolation)
        {
        }

        public DbTransactionInfo(Db db, IsolationLevel isolation)
        {
            if (db.IsTransactionReady)
                throw new TransactionAlreadyStartedException();

            Db = db;
            Db.StartUsingConnection();
            Db.DbEntityProvider.Transaction = Db.DbEntityProvider.Connection.BeginTransaction(isolation);
        }

        public DbCommand CreateCommand()
        {
            return Db.CreateCommand();
        }

        public void Commit()
        {
            if (!Db.IsTransactionReady)
                throw new TransactionNotStartedException();

            try
            {
                Db.DbEntityProvider.Transaction.Commit();
            }
            catch
            {
                Db.DbEntityProvider.Transaction.Rollback();
                throw;
            }
            finally
            {
                Db.DbEntityProvider.Transaction.Dispose();
                Db.DbEntityProvider.Transaction = null;
                Db.StopUsingConnection();
            }
        }

        public void Rollback()
        {
            if (!Db.IsTransactionReady)
                throw new TransactionNotStartedException();

            try
            {
                Db.DbEntityProvider.Transaction.Rollback();
            }
            catch
            {
                throw;
            }
            finally
            {
                Db.DbEntityProvider.Transaction.Dispose();
                Db.DbEntityProvider.Transaction = null;
                Db.StopUsingConnection();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (Db.IsTransactionReady)
            {
                this.Rollback();
            }
        }

        #endregion

    } //end of DbConnectionInfo

}

