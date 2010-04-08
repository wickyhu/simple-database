using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using IQToolkit;

namespace SimpleDatabase
{
    public partial class Db
    {
        public IQueryable<T> Query<T>()
        {
            IEntityTable<T> table = this.DbEntityProvider.GetTable<T>(typeof(T).Name);
            return (IQueryable<T>)table;
        }

    }
}
