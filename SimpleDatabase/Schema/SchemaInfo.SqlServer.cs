using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Text;
using SimpleControls;

namespace SimpleDatabase
{
    public class SqlServerSchemaInfo : SchemaInfo
    {
        public SqlServerSchemaInfo(Db db)
            : base(db)
        {
        }

        public override List<SchemaDatabase> GetDatabases()
        {
            using (DbConnectionInfo cn = Db.CreateConnection())
            {
                using (DbCommand cm = cn.CreateCommand())
                {
                    string sql = "select name,dbid from master.dbo.sysdatabases order by name";
                    
                    cm.CommandText = sql;
                    DataTable dt = Db.ExecQuery(cm);
                    DataRowCollection drc = dt.Rows;
                    List<SchemaDatabase> list = new List<SchemaDatabase>(drc.Count);
                    for (int i = 0; i < drc.Count; i++)
                    {
                        SchemaDatabase item = new SchemaDatabase();
                        item.Id = drc[i]["dbid"].IfNull(0);
                        item.Name = drc[i]["name"].IfNull(String.Empty);
                        list.Add(item);
                    }
                    return list;
                }
            }
        }

        public override List<SchemaTable> GetTables(string owner)
        {
            using (DbConnectionInfo cn = Db.CreateConnection())
            {
                using (DbCommand cm = cn.CreateCommand())
                {
                    //catalog,schema,table name 
                    //DataTable dt = cn.GetSchema("Tables", new string[] { cn.Database, owner, null });  
                    string sql = String.Format(@"SELECT * FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_CATALOG={0} 
                        AND (TABLE_SCHEMA={1} OR {1}='NULL') 
                        AND TABLE_TYPE='BASE TABLE' 
                        ORDER BY TABLE_NAME",
                        Db.GetParameterMarker("database"),
                        Db.GetParameterMarker("owner"));
                    
                    cm.CommandText = sql;
                    cm.Parameters.Add(Db.CreateParameter("database",cn.Database));
                    cm.Parameters.Add(Db.CreateParameter("owner",owner.IfNull("NULL")));
                    DataTable dt = Db.ExecQuery(cm);
                    DataRowCollection drc = dt.Rows;
                    List<SchemaTable> list = new List<SchemaTable>(drc.Count);
                    for (int i = 0; i < drc.Count; i++)
                    {
                        //if (drc[i]["table_type"].Equals("BASE TABLE"))
                        {
                            SchemaTable item = new SchemaTable();
                            item.Owner = drc[i]["table_schema"].IfNull(String.Empty);
                            item.Name = drc[i]["table_name"].IfNull(String.Empty);
                            list.Add(item);
                        }
                    }
                    return list;
                }
            }
        }

        public override List<SchemaView> GetViews(string owner)
        {
            using (DbConnectionInfo cn = Db.CreateConnection())
            {
                using (DbCommand cm = cn.CreateCommand())
                {
                    string sql = String.Format(@"SELECT * FROM INFORMATION_SCHEMA.VIEWS 
                        WHERE TABLE_CATALOG={0} and (TABLE_SCHEMA={1} or {1}='NULL') 
                        ORDER BY TABLE_NAME",
                        Db.GetParameterMarker("database"),
                        Db.GetParameterMarker("owner"));
                    
                    cm.CommandText = sql;
                    cm.Parameters.Add(Db.CreateParameter("database", cn.Database));
                    cm.Parameters.Add(Db.CreateParameter("owner", owner.IfNull("NULL")));
                    DataTable dt = Db.ExecQuery(cm);
                    DataRowCollection drc = dt.Rows;
                    List<SchemaView> list = new List<SchemaView>(drc.Count);
                    for (int i = 0; i < drc.Count; i++)
                    {
                        SchemaView item = new SchemaView();
                        item.Owner = drc[i]["TABLE_SCHEMA"].IfNull(String.Empty);
                        item.Name = drc[i]["TABLE_NAME"].IfNull(String.Empty);
                        item.IsUpdatable = drc[i]["IS_UPDATABLE"].Equals("YES");
                        item.Definition = drc[i]["VIEW_DEFINITION"].IfNull(String.Empty);
                        list.Add(item);
                    }
                    return list;
                }
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
                    item.Id = drc[i]["uid"].IfNull(0);
                    item.Name = drc[i]["user_name"].IfNull(String.Empty);
                    list.Add(item);
                }
                return list;
            }
        }

        public override List<SchemaColumn> GetColumns(string owner, string tableName)
        {
            using (DbConnectionInfo cn = Db.CreateConnection())
            {
                //catalog,schema,table name,column name
                DataTable dt = cn.GetSchema("Columns", new string[] { cn.Database, owner, tableName, null });
                DataRowCollection drc = dt.Rows;
                List<SchemaColumn> list = new List<SchemaColumn>(drc.Count);
                for (int i = 0; i < drc.Count; i++)
                {
                    SchemaColumn item = new SchemaColumn();
                    item.Owner = drc[i]["table_schema"].IfNull(String.Empty);
                    item.TableName = drc[i]["table_name"].IfNull(String.Empty);
                    item.ColumnName = drc[i]["column_name"].IfNull(String.Empty);
                    item.OrdinalPostion = drc[i]["ordinal_position"].IfNull(0);
                    item.DataType = drc[i]["data_type"].IfNull(String.Empty);
                    item.Length = drc[i]["character_maximum_length"].IfNull(0);
                    item.Precision = drc[i]["numeric_precision"].IfNull(0);
                    item.Scale = drc[i]["numeric_scale"].IfNull(0);
                    item.IsNullable = drc[i]["is_nullable"].Equals("YES");
                    item.DefaultValue = drc[i]["column_default"];
                    list.Add(item);
                }
                return list;
            }
        }

        protected List<SchemaRoutine> GetRoutines(string owner, string type)
        {
            using (DbConnectionInfo cn = Db.CreateConnection())
            {
                using (DbCommand cm = cn.CreateCommand())
                {
                    string sql = String.Format(@"SELECT ROUTINE_SCHEMA,ROUTINE_NAME,ROUTINE_DEFINITION FROM INFORMATION_SCHEMA.ROUTINES 
                        WHERE ROUTINE_CATALOG={0} AND ROUTINE_TYPE={1} AND (ROUTINE_SCHEMA={2} OR {2}='NULL')",
                        Db.GetParameterMarker("database"),
                        Db.GetParameterMarker("type"),
                        Db.GetParameterMarker("owner"));
                    
                    cm.CommandText = sql;
                    cm.Parameters.Add(Db.CreateParameter("database", cn.Database));
                    cm.Parameters.Add(Db.CreateParameter("type", type));
                    cm.Parameters.Add(Db.CreateParameter("owner", owner.IfNull("NULL")));
                    DataTable dt = Db.ExecQuery(cm);
                    DataRowCollection drc = dt.Rows;
                    List<SchemaRoutine> list = new List<SchemaRoutine>(drc.Count);
                    for (int i = 0; i < drc.Count; i++)
                    {
                        SchemaRoutine item = new SchemaRoutine();
                        item.Owner = drc[i]["ROUTINE_SCHEMA"].IfNull(String.Empty);
                        item.Name = drc[i]["ROUTINE_NAME"].IfNull(String.Empty);
                        item.Definition = drc[i]["ROUTINE_DEFINITION"].IfNull(String.Empty);
                        list.Add(item);
                    }
                    return list;
                }
            }

        }

        public override List<SchemaRoutine> GetProcedures(string owner)
        {
            return GetRoutines(owner, "PROCEDURE");
        }

        public override List<SchemaRoutine> GetFunctions(string owner)
        {
            return GetRoutines(owner, "FUNCTION");
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
                    item.IndexOwner = drc[i]["constraint_schema"].IfNull(String.Empty);
                    item.IndexName = drc[i]["constraint_name"].IfNull(String.Empty);
                    item.TableOwner = drc[i]["table_schema"].IfNull(String.Empty);
                    item.TableName = drc[i]["table_name"].IfNull(String.Empty);
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
                    item.IndexOwner = drc[i]["constraint_schema"].IfNull(String.Empty);
                    item.IndexName = drc[i]["constraint_name"].IfNull(String.Empty);
                    item.TableOwner = drc[i]["table_schema"].IfNull(String.Empty);
                    item.TableName = drc[i]["table_name"].IfNull(String.Empty);
                    item.OrdinalPosition = drc[i]["ordinal_position"].IfNull(0);
                    item.ColumnName = drc[i]["column_name"].IfNull(String.Empty);
                    //drc[i]["keytype"] //56 unique key, 167 index, 108 primary key...
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
                    string sql = String.Format(@"SELECT
                CONSTRAINT_NAME = REF_CONST.CONSTRAINT_NAME,
                TABLE_CATALOG = FK.TABLE_CATALOG,
                TABLE_SCHEMA = FK.TABLE_SCHEMA,
                TABLE_NAME = FK.TABLE_NAME,
                COLUMN_NAME = FK_COLS.COLUMN_NAME,
                REFERENCED_TABLE_CATALOG = PK.TABLE_CATALOG,
                REFERENCED_TABLE_SCHEMA = PK.TABLE_SCHEMA,
                REFERENCED_TABLE_NAME = PK.TABLE_NAME,
                REFERENCED_COLUMN_NAME = PK_COLS.COLUMN_NAME
                FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS REF_CONST 
                INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK
	                ON REF_CONST.CONSTRAINT_CATALOG = FK.CONSTRAINT_CATALOG
	                AND REF_CONST.CONSTRAINT_SCHEMA = FK.CONSTRAINT_SCHEMA
	                AND REF_CONST.CONSTRAINT_NAME = FK.CONSTRAINT_NAME
	                AND FK.CONSTRAINT_TYPE = 'FOREIGN KEY'
                INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK 
	                ON REF_CONST.UNIQUE_CONSTRAINT_CATALOG = PK.CONSTRAINT_CATALOG
	                AND REF_CONST.UNIQUE_CONSTRAINT_SCHEMA = PK.CONSTRAINT_SCHEMA
	                AND REF_CONST.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME
	                AND PK.CONSTRAINT_TYPE = 'PRIMARY KEY'
                INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE FK_COLS 
	                ON REF_CONST.CONSTRAINT_NAME = FK_COLS.CONSTRAINT_NAME
                INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE PK_COLS 
	                ON PK.CONSTRAINT_NAME = PK_COLS.CONSTRAINT_NAME
	                AND FK_COLS.ORDINAL_POSITION = PK_COLS.ORDINAL_POSITION
                WHERE FK.TABLE_CATALOG={0} 
                AND (FK.TABLE_SCHEMA={1} OR {1}='NULL') 
                AND (FK.TABLE_NAME={2} OR {2}='NULL') 
                ORDER BY FK.TABLE_CATALOG, FK.TABLE_SCHEMA, FK.TABLE_NAME, REF_CONST.CONSTRAINT_NAME, FK_COLS.COLUMN_NAME",
                        Db.GetParameterMarker("database"),
                        Db.GetParameterMarker("owner"),
                        Db.GetParameterMarker("tablename"));
                    
                    cm.CommandText = sql;
                    cm.Parameters.Add(Db.CreateParameter("database", cn.Database));                    
                    cm.Parameters.Add(Db.CreateParameter("owner", owner.IfNull("NULL")));
                    cm.Parameters.Add(Db.CreateParameter("tablename", tableName.IfNull("NULL")));
                    DataTable dt = Db.ExecQuery(cm);
                    DataRowCollection drc = dt.Rows;
                    List<SchemaForeignKeyColumn> list = new List<SchemaForeignKeyColumn>(drc.Count);
                    for (int i = 0; i < drc.Count; i++)
                    {
                        SchemaForeignKeyColumn item = new SchemaForeignKeyColumn();
                        item.Owner = drc[i]["TABLE_SCHEMA"].IfNull(String.Empty);
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

