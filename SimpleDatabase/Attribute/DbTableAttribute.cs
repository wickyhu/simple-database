using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleDatabase
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class DbTableAttribute : Attribute
    {
        public string Name { get; set; }
        public TableAuditTypes AuditType { get; set; }

        public DbTableAttribute()
            : this(null, TableAuditTypes.Default)
        {
        }

        public DbTableAttribute(string name)
            : this(name, TableAuditTypes.Default)
        {
        }

        public DbTableAttribute(string name, TableAuditTypes auditType)
        {
            Name = name;
            AuditType = auditType;
        }
    }

}
