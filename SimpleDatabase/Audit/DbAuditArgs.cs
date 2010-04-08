using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;

namespace SimpleDatabase
{
    public class DbAuditArgs
    {
        public ExecTypes Action;
        public DbTable Table;
        public string Sql;
        public object Data;

        public DbAuditArgs(ExecTypes action, DbTable table, string sql, object data)
        {
            Action = action;
            Table = table;
            Sql = sql;
            Data = data;
        }
    }
}
