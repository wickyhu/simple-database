using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleDatabase
{
    public class DbException : ApplicationException
    {
        public DbException(string msg)
            : base(msg)
        {
        }
    }

    public class TransactionNotStartedException : DbException
    {
        const string msg = "Transaction not started";
        public TransactionNotStartedException()
            : base(msg)
        {
        }
    }

    public class TransactionAlreadyStartedException : DbException
    {
        const string msg = "Transaction already started";
        public TransactionAlreadyStartedException()
            : base(msg)
        {
        }
    }

    public class TransactionCamcelled : DbException
    {
        const string msg = "Transaction cancelled";
        public TransactionCamcelled()
            : base(msg)
        {
        }
    }

    public class MissingPrimaryKeyValueException : DbException
    {
        const string msg = "Primary key value is missing";
        public MissingPrimaryKeyValueException()
            : base(msg)
        {
        }
        public MissingPrimaryKeyValueException(string fieldName)
            : base(String.Format("{0}: {1}", msg, fieldName))
        {
        }
    }

    public class MissingPrimaryKeyException : DbException
    {
        const string msg = "Primary key is missing";
        public MissingPrimaryKeyException()
            : base(msg)
        {
        }
    }
}

