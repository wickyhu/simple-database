using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleDatabase
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class DbFieldAttribute : Attribute
    {
        public string Name { get; set; }

        public DbFieldAttribute()
        {
        }

        public DbFieldAttribute(string name)
        {
            Name = name;
        }
    }

}
