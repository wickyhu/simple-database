using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Text;
using SimpleControls;

namespace SimpleDatabase
{
    public class OracleSchemaInfo : SchemaInfo
    {
        public OracleSchemaInfo(Db db)
            : base(db)
        {
        }

        public override List<SchemaTable> GetTables(string owner)
        {
            using (DbConnectionInfo cn = Db.CreateConnection())
            {
                //owner ,table name 
                DataTable dt = cn.GetSchema("Tables", new string[] { owner, null });
                DataRowCollection drc = dt.Rows;
                List<SchemaTable> list = new List<SchemaTable>(drc.Count);
                for (int i = 0; i < drc.Count; i++)
                {
                    SchemaTable item = new SchemaTable();
                    item.Owner = drc[i]["OWNER"].IfNull(String.Empty);
                    item.Name = drc[i]["TABLE_NAME"].IfNull(String.Empty);
                    //drc[i]["TYPE"] User or System
                    list.Add(item);
                }
                return list;
            }
        }

        public override List<SchemaView> GetViews(string owner)
        {
            using (DbConnectionInfo cn = Db.CreateConnection())
            {
                // owner, view name
                DataTable dt = cn.GetSchema("Views", new string[] { owner, null });
                DataRowCollection drc = dt.Rows;
                List<SchemaView> list = new List<SchemaView>(drc.Count);
                for (int i = 0; i < drc.Count; i++)
                {
                    SchemaView item = new SchemaView();
                    item.Owner = drc[i]["OWNER"].IfNull(String.Empty);
                    item.Name = drc[i]["VIEW_NAME"].IfNull(String.Empty);
                    item.IsUpdatable = false;
                    item.Definition = drc[i]["TEXT"].IfNull(String.Empty);
                    list.Add(item);
                }
                return list;
            }
        }

        public override List<SchemaUser> GetUsers()
        {
            using (DbConnectionInfo cn = Db.CreateConnection())
            {
                DataTable dt = cn.GetSchema("Users");
                DataRowCollection drc = dt.Rows;
                List<SchemaUser> list = new List<SchemaUser>(drc.Count);
                for (int i = 0; i < drc.Count; i++)
                {
                    SchemaUser item = new SchemaUser();
                    item.Id = drc[i]["ID"].IfNull(0);
                    item.Name = drc[i]["NAME"].IfNull(String.Empty);
                    list.Add(item);
                }
                return list;
            }
        }

        public override List<SchemaColumn> GetColumns(string owner, string tableName)
        {
            using (DbConnectionInfo cn = Db.CreateConnection())
            {
                //owner, table name, column_name
                DataTable dt = cn.GetSchema("Columns", new string[] { owner, tableName, null });
                DataRowCollection drc = dt.Rows;
                List<SchemaColumn> list = new List<SchemaColumn>(drc.Count);
                for (int i = 0; i < drc.Count; i++)
                {
                    SchemaColumn item = new SchemaColumn();
                    item.Owner = drc[i]["OWNER"].IfNull(String.Empty);
                    item.TableName = drc[i]["TABLE_NAME"].IfNull(String.Empty);
                    item.ColumnName = drc[i]["COLUMN_NAME"].IfNull(String.Empty);
                    item.OrdinalPostion = drc[i]["ID"].IfNull(0);
                    item.DataType = drc[i]["DATA_TYPE"].IfNull(String.Empty);
                    item.Length = drc[i]["LENGTH"].IfNull(0);
                    item.Precision = drc[i]["PRECISION"].IfNull(0);
                    item.Scale = drc[i]["SCALE"].IfNull(0);
                    item.IsNullable = drc[i]["NULLABLE"].Equals("Y");
                    //item.DefaultValue = 
                    list.Add(item);
                }
                return list;
            }
        }

        protected List<SchemaRoutine> GetSources(string owner, string type)
        {
            using (DbConnectionInfo cn = Db.CreateConnection())
            {
                using (DbCommand cm = cn.CreateCommand())
                {
                    string sql = String.Format(@"SELECT * FROM ALL_SOURCE 
                        WHERE (OWNER={0} OR {0}='NULL') 
                        AND TYPE={1} 
                        ORDER BY OWNER,NAME,LINE", 
                         Db.GetParameterMarker("p_owner"),
                         Db.GetParameterMarker("p_type"));
                    
                    cm.CommandText = sql;
                    cm.Parameters.Add(Db.CreateParameter("p_owner", owner.IfNull("NULL").ToUpper()));
                    cm.Parameters.Add(Db.CreateParameter("p_type", type));                                                 
                    DataTable dt = Db.ExecQuery(cm);

                    DataRowCollection drc = dt.Rows;
                    StringBuilder sb = new StringBuilder();
                    string owner1 = null;
                    string name1 = null;

                    List<SchemaRoutine> list = new List<SchemaRoutine>();

                    for (int i = 0; i < drc.Count; i++)
                    {
                        if (!drc[i]["OWNER"].Equals(owner1) || !drc[i]["NAME"].Equals(name1))
                        {
                            if (!String.IsNullOrEmpty(name1))
                            {
                                SchemaRoutine item = new SchemaRoutine();
                                item.Owner = owner1;
                                item.Name = name1;
                                item.Definition = sb.ToString();
                                list.Add(item);
                            }
                            owner1 = drc[i]["OWNER"].IfNull(String.Empty);
                            name1 = drc[i]["NAME"].IfNull(String.Empty);
                            sb.Remove(0, sb.Length);
                        }
                        sb.Append(drc[i]["TEXT"].IfNull(String.Empty));
                    }

                    if (drc.Count > 0)
                    {
                        SchemaRoutine item = new SchemaRoutine();
                        item.Owner = owner1;
                        item.Name = name1;
                        item.Definition = sb.ToString();
                        list.Add(item);
                    }

                    return list;
                }
            }
        }

        public override List<SchemaRoutine> GetProcedures(string owner)
        {
            return GetSources(owner, "PROCEDURE");
        }

        public override List<SchemaRoutine> GetFunctions(string owner)
        {
            return GetSources(owner, "FUNCTION");
        }

        public List<SchemaRoutine> GetPackages(string owner)
        {
            return GetSources(owner, "PACKAGE");
        }

        public List<SchemaRoutine> GetPackageBodies(string owner)
        {
            return GetSources(owner, "PACKAGE BODY");
        }

        public override List<SchemaIndex> GetIndexes(string owner, string tableName)
        {
            using (DbConnectionInfo cn = Db.CreateConnection())
            {
                DataTable dt = cn.GetSchema("Indexes", new string[] { cn.Database, owner, tableName });
                DataRowCollection drc = dt.Rows;
                List<SchemaIndex> list = new List<SchemaIndex>(drc.Count);
                for (int i = 0; i < drc.Count; i++)
                {
                    SchemaIndex item = new SchemaIndex();
                    item.IndexOwner = drc[i]["OWNER"].IfNull(String.Empty);
                    item.IndexName = drc[i]["INDEX_NAME"].IfNull(String.Empty);
                    item.TableOwner = drc[i]["TABLE_OWNER"].IfNull(String.Empty);
                    item.TableName = drc[i]["TABLE_NAME"].IfNull(String.Empty);
                    list.Add(item);
                }
                return list;
            }
        }

        public override List<SchemaIndexColumn> GetIndexColumns(string owner, string tableName, string indexName)
        {
            using (DbConnectionInfo cn = Db.CreateConnection())
            {
                DataTable dt = cn.GetSchema("IndexColumns", new string[] { cn.Database, owner, tableName, indexName });
                DataRowCollection drc = dt.Rows;
                List<SchemaIndexColumn> list = new List<SchemaIndexColumn>(drc.Count);
                for (int i = 0; i < drc.Count; i++)
                {
                    SchemaIndexColumn item = new SchemaIndexColumn();
                    item.IndexOwner = drc[i]["INDEX_OWNER"].IfNull(String.Empty);
                    item.IndexName = drc[i]["INDEX_NAME"].IfNull(String.Empty);
                    item.TableOwner = drc[i]["TABLE_OWNER"].IfNull(String.Empty);
                    item.TableName = drc[i]["TABLE_NAME"].IfNull(String.Empty);
                    item.OrdinalPosition = drc[i]["COLUMN_POSITION"].IfNull(0);
                    item.ColumnName = drc[i]["COLUMN_NAME"].IfNull(String.Empty);
                    //drc[i]["DESCEND"] 
                    list.Add(item);
                }
                return list;
            }
        }

        public override List<SchemaForeignKeyColumn> GetForeignKeyColumns(string owner, string tableName)
        {
            using (DbConnectionInfo cn = Db.CreateConnection())
            {
                using (DbCommand cm = cn.CreateCommand())
                {
                    string sql = String.Format(@"SELECT c.constraint_name, c.owner AS table_owner, c.table_name AS table_name,
                   ctc.column_name, p.constraint_name AS referenced_constraint_name,
                   p.owner AS referenced_table_owner,
                   p.table_name AS referenced_table_name,
                   ptc.column_name AS referenced_column_name
              FROM all_tab_columns ptc,
                   all_tab_columns ctc,
                   all_cons_columns pcc,
                   all_cons_columns ccc,
                   all_constraints p,
                   all_constraints c
             WHERE c.constraint_type = 'R'
               AND p.constraint_name = c.r_constraint_name
               AND p.owner = c.r_owner
               AND ccc.owner = c.owner
               AND ccc.constraint_name = c.constraint_name
               AND ctc.owner = c.owner
               AND ctc.table_name = ccc.table_name
               AND ctc.column_name = ccc.column_name
               AND pcc.owner = c.r_owner
               AND pcc.constraint_name = p.constraint_name
               AND ptc.owner = c.r_owner
               AND ptc.table_name = pcc.table_name
               AND ptc.column_name = pcc.column_name
               AND ccc.POSITION = pcc.POSITION 
               AND (c.owner = {0} or {0}='NULL') 
               AND (c.table_name = {1} or {1}='NULL') 
              ORDER BY c.owner, c.table_name, c.constraint_name, ctc.column_name",
                         Db.GetParameterMarker("p_owner"),
                         Db.GetParameterMarker("p_tablename"));
                    
                    cm.CommandText = sql;
                    cm.Parameters.Add(Db.CreateParameter("p_owner", owner.IfNull("NULL").ToUpper()));
                    cm.Parameters.Add(Db.CreateParameter("p_tablename", tableName.IfNull("NULL").ToUpper()));
                    DataTable dt = Db.ExecQuery(cm);

                    DataRowCollection drc = dt.Rows;
                    List<SchemaForeignKeyColumn> list = new List<SchemaForeignKeyColumn>(drc.Count);
                    for (int i = 0; i < drc.Count; i++)
                    {
                        SchemaForeignKeyColumn item = new SchemaForeignKeyColumn();
                        item.Owner = drc[i]["TABLE_OWNER"].IfNull(String.Empty);
                        item.ConstraintName = drc[i]["CONSTRAINT_NAME"].IfNull(String.Empty);
                        item.TableName = drc[i]["TABLE_NAME"].IfNull(String.Empty);
                        item.ColumnName = drc[i]["COLUMN_NAME"].IfNull(String.Empty);
                        item.ReferencedTableName = drc[i]["REFERENCED_TABLE_NAME"].IfNull(String.Empty);
                        item.ReferencedColumnName = drc[i]["REFERENCED_COLUMN_NAME"].IfNull(String.Empty);
                        list.Add(item);
                    }
                    return list;
                }
            }
        }
    }//end of class
}
