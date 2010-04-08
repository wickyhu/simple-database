using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace SimpleDatabase
{
    internal class DbCache
    {
        static Dictionary<string, DbCache> _dbCaches = new Dictionary<string, DbCache>(StringComparer.OrdinalIgnoreCase);
        public static DbCache Get(string connectionString)
        {
            DbCache c;
            _dbCaches.TryGetValue(connectionString, out c);
            if (c == null)
            {
                c = new DbCache();
                _dbCaches.Add(connectionString, c);
            }
            return c;
        }
        public static DbCache Get(Db db)
        {
            return Get(db.ConnectionString);
        }
        public static void Remove(string connectionString)
        {
            if (_dbCaches.ContainsKey(connectionString))
                _dbCaches.Remove(connectionString);
        }
        public static void Remove(Db db)
        {
            Remove(db.ConnectionString);
        }

        /// <summary>
        /// use Get function
        /// </summary>
        private DbCache()
        {
        }

        /// <summary>
        /// Table Name -> Schema Table
        /// </summary>
        private Dictionary<string, DataTable> _cachedSchemaTables = new Dictionary<string, DataTable>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, DataTable> SchemaTables
        {
            get
            {
                return _cachedSchemaTables;
            }        
        }

        /// <summary>
        /// Table Name -> DbTable
        /// </summary>
        private Dictionary<string, DbTable> _cachedDbTables = new Dictionary<string, DbTable>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, DbTable> DbTables
        {
            get
            {
                return _cachedDbTables;
            }
        }

        /// <summary>
        /// Type Name -> DbTableEntityMapping
        /// </summary>
        private Dictionary<string, DbTableEntityMapping> _cachedTypeMappings = new Dictionary<string, DbTableEntityMapping>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, DbTableEntityMapping> TypeMappings
        {
            get
            {
                return _cachedTypeMappings;
            }
        }

        /// <summary>
        /// Table Name -> DbTableEntityMapping 
        /// </summary>    
        private Dictionary<string, DbTableEntityMapping> _cachedTableMappings = new Dictionary<string, DbTableEntityMapping>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, DbTableEntityMapping> TableMappings
        {
            get
            {
                return _cachedTableMappings;
            }
        }
        private List<AssociationInfo> _cachedIncludedAssociationInfos = new List<AssociationInfo>();        
        public List<AssociationInfo> IncludedAssociationInfos
        {
            get
            {
                return _cachedIncludedAssociationInfos;
            }
        }

        /*
        /// <summary>
        /// Table Name -> InsertStatementParameter 
        /// </summary>    
        private Dictionary<string, InsertStatementParameter> _cachedInsertStatementParameters = new Dictionary<string, InsertStatementParameter>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, InsertStatementParameter> InsertStatementParameters
        {
            get
            {
                return _cachedInsertStatementParameters;
            }
        }
        */

    }//end of class

    /*
    internal class InsertStatementParameter
    {
        public string Sql { get; set; }
        public List<DbField> Fields = new List<DbField>();
    }
    */

    public partial class Db
    {
        private DbCache _cache = null;
        internal DbCache Cache
        {
            get
            {
                if (_cache == null)
                {
                    _cache = DbCache.Get(this);
                }
                return _cache;
            }    
        }
    
    }//end of class
}
