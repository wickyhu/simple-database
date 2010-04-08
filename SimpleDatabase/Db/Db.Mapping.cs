using System;
using System.Data;
using System.Data.Common;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace SimpleDatabase
{
    public partial class Db
    {

        internal DbTableEntityMapping GetDbTableEntityMapping(Type t)
        {
            string key = t.FullName;
            if (Cache.TypeMappings.ContainsKey(key))
                return Cache.TypeMappings[key];

            DbTableAttribute ta = t.GetDbTableAttribute();
            string tableName;

            if (ta != null && !String.IsNullOrEmpty(ta.Name))
            {
                tableName = ta.Name;
            }
            else
            {
                tableName = t.Name;
            }

            if (tableName.StartsWith("<>")) return null;
            DbTable table = GetDbTable(tableName); // DON'T use GetDbTable(Type), it's dead loop
            if (table == null) return null;

            if (ta != null)
            {
                table.DefaultAuditType = ta.AuditType;
            }

            DbTableEntityMapping tm = new DbTableEntityMapping(this, t, table);
            Cache.TypeMappings.Add(key, tm);
            Cache.TableMappings.Add(tableName, tm);

            // temporarily put here (after cached) to avoid dead loop
            tm.BuildAssociationInfo(this);

            return tm;
        }

        internal DbTableEntityMapping GetDbTableEntityMapping(string tableName)
        {
            DbTableEntityMapping tt;
            Cache.TableMappings.TryGetValue(tableName, out tt);
            return tt;
        }

        public DbTable GetDbTable(Type t)
        {
            DbTableEntityMapping tm = this.GetDbTableEntityMapping(t);
            if (tm == null) return null;
            return tm.Table;
        }

        public DbTable GetDbTable(string tableName)
        {
            string key = tableName;
            if (Cache.DbTables.ContainsKey(key))
                return Cache.DbTables[key];

            DbTable t = DbTable.Create(this, tableName);
            if (t.Fields == null) t = null;
            Cache.DbTables.Add(key, t);
            return t;
        }

        //public void SetAssociation(Type t, string name, bool isIncluded, bool isDeferLoaded)
        //{
        //    DbTableEntityMapping tm = GetDbTableEntityMapping(t);
        //    if (tm == null) return;
        //    if (!tm.AssociationInfos.ContainsKey(name)) return;
        //    AssociationInfo ai = tm.AssociationInfos[name];
        //    ai.IsDeferLoaded = isDeferLoaded;
        //    ai.IsIncluded = isIncluded;
        //}


        public DataTable GetSchemaTable(string tableName)
        {
            DataTable dt;

            if (Cache.SchemaTables.TryGetValue(tableName, out dt))
            {
                return dt;
            }

            try
            {
                using (DbConnectionInfo cn = CreateConnection())
                {
                    using (DbCommand cm = cn.CreateCommand())
                    {
                        cm.CommandText = String.Format("SELECT * FROM {0} WHERE 0=1", Quote(tableName));
                        

                        using (DbDataReader r = cm.ExecuteReader(CommandBehavior.KeyInfo |
                            CommandBehavior.SchemaOnly))
                        {
                            dt = r.GetSchemaTable();
                            dt.TableName = tableName;
                            Cache.SchemaTables.Add(tableName, dt);
                        }
                    }
                }
            }
            catch
            {
                //table not exists or no permission 
            }
            return dt;
        }
        

    }//end of class
}
