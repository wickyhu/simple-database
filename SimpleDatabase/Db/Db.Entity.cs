using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using IQToolkit;
using IQToolkit.Data;
using IQToolkit.Data.Common;
using SimpleControls;

namespace SimpleDatabase
{
    public partial class Db
    {
        public T GetById<T>(object id)
        {
            IEntityTable<T> table = DbEntityProvider.GetTable<T>(typeof(T).Name);            
            return table.GetById(id);            
        }     


        public int Insert<T>(T instance)
        {
            Type t = typeof(T);
            DbTable table = GetDbTable(t);
            Dictionary<string, object> dic = ToDictionary(table, t, instance);
            int r = Insert(table, dic);
            SetNewRow(t, instance, dic);
            return r;          
        }

        public int Update<T>(T instance)
        {
            Type t = typeof(T);
            DbTable table = GetDbTable(t);
            Dictionary<string, object> dic = ToDictionary(table, t, instance);
            int r = Update(table, dic);
            SetUpdatedRow(t, instance, dic);
            return r;
        }

        private bool IsNewId(object id)
        {
            return id == null || id.Equals(0) || id.Equals(String.Empty);
        }

        public bool IsNew<T>(T instance)
        {
            Type t = typeof(T);
            DbTable table = GetDbTable(t);
            int i = table.GetFieldIndex(DbFieldNames.Id);
            if (i >= 0)
            {
                object id = GetPropertyValue(t, instance, DbFieldNames.Id);
                return IsNewId(id);
            }
            return false;
        }

        public int InsertOrUpdate<T>(T instance)
        {
            IEntityTable<T> table = DbEntityProvider.GetTable<T>(typeof(T).Name);
            if (IsNew<T>(instance))
            {
                return Insert<T>(instance);
            }
            else
            {
                return Update<T>(instance);
            }            
        }

        public int Delete<T>(T instance)
        {
            Type t = typeof(T);
            DbTable table = GetDbTable(t);
            Dictionary<string, object> dic = ToDictionary(table, t, instance);
            return Delete(table, dic);
        }

        public int Delete<T>(Expression<Func<T, bool>> predicate)
        {
            IEntityTable<T> table = DbEntityProvider.GetTable<T>(typeof(T).Name);
            int r = table.Delete<T>(predicate);
            DoAudit(new DbAuditArgs(ExecTypes.Delete, GetDbTable(typeof(T)), null, predicate.ToString()));
            return r;
        }

        protected Dictionary<string, object> ToDictionary(DbTable table, Type t, object instance)
        {
            DbTableEntityMapping tm = GetDbTableEntityMapping(t);
            if (tm == null) return null;
            Dictionary<string, object> dic = new Dictionary<string, object>(tm.DbFieldInfos.Count, StringComparer.OrdinalIgnoreCase);
            foreach (string pName in tm.DbFieldInfos.Keys)
            {
                dic.Add(tm.DbFieldInfos[pName].FieldName, GetPropertyValue(t, instance, pName));
            }
            return dic;
        }

        void SetNewRow(Type t, object instance, Dictionary<string, object> dic)
        {
            if (NextIdProvider != null && dic.ContainsKey(DbFieldNames.Id))
                SetPropertyValue(t, instance, DbFieldNames.Id, dic[DbFieldNames.Id]);
            if (dic.ContainsKey(DbFieldNames.CreatedBy))
                SetPropertyValue(t, instance, DbFieldNames.CreatedBy, dic[DbFieldNames.CreatedBy]);
            if (dic.ContainsKey(DbFieldNames.CreatedOn))
                SetPropertyValue(t, instance, DbFieldNames.CreatedOn, dic[DbFieldNames.CreatedOn]);
        }

        void SetUpdatedRow(Type t, object instance, Dictionary<string, object> dic)
        {
            if (dic.ContainsKey(DbFieldNames.UpdatedBy))
                SetPropertyValue(t, instance, DbFieldNames.UpdatedBy, dic[DbFieldNames.UpdatedBy]);
            if (dic.ContainsKey(DbFieldNames.UpdatedOn))
                SetPropertyValue(t, instance, DbFieldNames.UpdatedOn, dic[DbFieldNames.UpdatedOn]);
        }

    }
}
