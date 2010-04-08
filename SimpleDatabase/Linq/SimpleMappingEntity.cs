using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IQToolkit.Data;
using IQToolkit.Data.Common;

namespace SimpleDatabase
{
    public class SimpleMappingEntity : MappingEntity
    {
        public DbTableEntityMapping TableTypeMapping { get; private set; }

        public SimpleMappingEntity(DbTableEntityMapping tm)
        {
            TableTypeMapping = tm;
        }

        public override string TableId
        {
            get { return TableTypeMapping.Table.TableName; }
        }

        public override Type ElementType
        {
            get { return TableTypeMapping.EntityType; }
        }

        public override Type EntityType
        {
            get { return TableTypeMapping.EntityType; }
        }

        public override string ToString()
        {
            return String.Format("Table: {0}, Type: {1}",
                TableTypeMapping.Table.TableName,
                TableTypeMapping.EntityType.Name);
        }

        public override int GetHashCode()
        {
            return TableTypeMapping.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            SimpleMappingEntity o = obj as SimpleMappingEntity;
            return o == null ? false : this.GetHashCode() == o.GetHashCode();
        }
    }

}
