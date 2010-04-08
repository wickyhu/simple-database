using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleDatabase
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class DbEmbeddedAttribute : Attribute
    {
        public DbEmbeddedAttribute()
        {            
        }
    }

}
