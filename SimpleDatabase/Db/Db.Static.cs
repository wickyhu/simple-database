using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace SimpleDatabase
{
    public partial class Db
    {
        [ThreadStatic]
        static DbAuditTypes _auditType;
        public static DbAuditTypes AuditType
        {
            get { return _auditType; }
            set { _auditType = value; }
        }

        static TableAuditTypes _defaultTableAuditType = TableAuditTypes.SharedTable;
        public static TableAuditTypes DefaultTableAuditType
        {
            get { return _defaultTableAuditType; }
            set
            {
                switch (value)
                {
                    case TableAuditTypes.Default:
                        throw new InvalidOperationException("DefaultTableAuditType shouldn't be set to Default.");
                    default:
                        _defaultTableAuditType = value;
                        break;
                }
            }
        }

        [ThreadStatic]
        static Dictionary<string, TableAuditTypes> _tableAuditTypesList;
        internal static Dictionary<string, TableAuditTypes> TableAuditTypesList
        {
            get
            {
                if (_tableAuditTypesList == null)
                {
                    _tableAuditTypesList = new Dictionary<string, TableAuditTypes>();
                }
                return _tableAuditTypesList;
            }
        }

    }
}
