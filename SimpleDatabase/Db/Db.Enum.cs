using System;
using System.Text;

namespace SimpleDatabase
{
    public enum DbTypes
    {
        Unknown = 0,
        Oracle,
        SqlServer,
        PostgreSQL,
        MySql,
        DB2,
        SQLite,
        Firebird,
        Access
    }

    public enum ExecTypes
    {
        NonQuery,
        Query,
        QueryEx,
        Scalar,
        Reader,
        Insert,
        Update,
        Delete
    }    

    public enum TableAuditTypes
    {
        /// <summary>
        /// Follow Db Audit Type
        /// </summary>
        Default,
        /// <summary>
        /// No audit
        /// </summary>
        None,
        /// <summary>
        /// Use shared tables to save audit records
        /// </summary>
        SharedTable,
        /// <summary>
        /// Use standalone tables to save audit records
        /// </summary>
        StandaloneTable
    }

    //Explicit should be first one, Db.AuditType rely on this to be initialized to Explicit by default
    public enum DbAuditTypes
    {
        /// <summary>
        /// Audit is disabled for all entities by default, except explicit enabled
        /// </summary>
        Explicit,
        /// <summary>
        /// Audit is enabled and controlled by DefaultTableAuditType
        /// </summary>
        Implicit,
        /// <summary>
        /// No audit
        /// </summary>
        None
    }

}
