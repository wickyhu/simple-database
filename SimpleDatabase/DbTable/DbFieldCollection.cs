using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;

namespace SimpleDatabase
{
    //[Serializable]
    public class DbFieldCollection : CollectionBase
    {
        private Dictionary<string, int> _nameToIndex;

        private DbFieldCollection()
        {
        }

        public static DbFieldCollection Create(DataTable columns)
        {
            DbFieldCollection fc = new DbFieldCollection();

            fc._nameToIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < columns.Rows.Count; i++)
            {
                DataRow dr = columns.Rows[i];
                DbField f = DbField.Create(dr);

                //if (fc.IndexOf(f.FieldName) >= 0)
                //    continue;

                fc.Add(f);
            }

            return fc;
        }

        public DbField this[int index]
        {
            get
            {
                return ((DbField)List[index]);
            }
            set
            {
                List[index] = value;
            }
        }

        public DbField this[string fieldName]
        {
            get
            {
                int index = IndexOf(fieldName);
                if (index < 0) return null;
                return ((DbField)List[index]);
            }
            set
            {
                int index = IndexOf(fieldName);
                List[index] = value;
            }
        }

        public int Add(DbField value)
        {
            return (List.Add(value));
        }

        public int IndexOf(DbField value)
        {
            return IndexOf(value.FieldName);
        }

        public int IndexOf(string fieldName)
        {
            int index = -1;
            if (_nameToIndex.ContainsKey(fieldName))
            {
                index = _nameToIndex[fieldName];
            }
            return index;
        }

        public bool Contains(string fieldName)
        {
            return IndexOf(fieldName) >= 0;
        }

        public void Insert(int index, DbField value)
        {
            List.Insert(index, value);
        }

        public void Remove(DbField value)
        {
            List.Remove(value);
        }

        public bool Contains(DbField value)
        {
            return (List.Contains(value));
        }

        protected override void OnInsert(int index, Object value)
        {
            DbField f = (DbField)value;
            _nameToIndex.Add(f.FieldName, index);
        }

        protected override void OnRemove(int index, Object value)
        {
            DbField f = (DbField)value;
            _nameToIndex.Remove(f.FieldName);
        }

        protected override void OnSet(int index, Object oldValue, Object newValue)
        {
            DbField fOld = (DbField)oldValue;
            DbField fNew = (DbField)newValue;

            _nameToIndex.Remove(fOld.FieldName);
            _nameToIndex.Add(fNew.FieldName, index);
        }

        //protected override void OnValidate( Object value )  
        //{
        //	if ( value.GetType().Name != "DbField" )
        //		throw new ArgumentException( "Value must be of type DbField.", "value" );
        //}

        public override string ToString()
        {
            return String.Format("Count={0}", this.Count);
        }
    } // end of class
}

