using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleDatabase
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class DbAssociationAttribute : Attribute
    {
        public bool IsIncluded { get; set; }
        public bool IsDeferLoaded { get; set; }

        public DbAssociationAttribute() : this(true, false)
        {
        }

        public DbAssociationAttribute(bool isIncluded, bool isDeferLoaded)
        {
            IsIncluded = isIncluded;
            IsDeferLoaded = isDeferLoaded;
        }

    }

}
