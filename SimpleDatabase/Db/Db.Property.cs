using System;
using System.Text;
using System.Data;
using System.Data.Common;
using IQToolkit;
using IQToolkit.Data;

namespace SimpleDatabase
{
    public partial class Db
    {
        public const int DefaultCommandTimeout = 5 * 60 * 1000;

        public string ConnectionString { get; private set; }
        public string ProviderInvariantName { get; private set; }
        public DbTypes DbType { get; private set; }
        public int CommandTimeout { get; set; }

        internal DbEntityProvider DbEntityProvider { get; private set; }
        internal DbProviderFactory DbFactory { get; private set; }

        public SimplePolicy Policy
        {
            get
            {
                return DbEntityProvider.Policy as SimplePolicy;
            }
        }

        private DataSourceInfo _dataSourceInfo;
        public DataSourceInfo DataSourceInfo 
        {
            get
            {
                if (_dataSourceInfo == null)
                {
                    _dataSourceInfo = new DataSourceInfo(this);
                }
                return _dataSourceInfo;
            }
        }

        private DbMetadata _dbMetadata;
        public DbMetadata DbMetadata
        {
            get
            {
                if (_dbMetadata == null)
                {
                    _dbMetadata = DbMetadata.Get(this);
                }
                return _dbMetadata;
            }
        }

        private SchemaInfo _schemaInfo;
        public SchemaInfo SchemaInfo
        {
            get
            {
                if (_schemaInfo == null)
                {
                    _schemaInfo = (SchemaInfo)Activator.CreateInstance(DbMetadata.SchemaInfoType, this);
                }
                return _schemaInfo;
            }
        }


        INextIdProvider _nextId;
        public INextIdProvider NextIdProvider
        {
            get
            {
                if (_nextId == null)
                {
                    _nextId = new KeyTableNextIdProvider(this);
                }
                return _nextId;
            }
            set
            {
                _nextId = value;
            }
        }

        public IUserInfoProvider UserInfoProvider { get; set; }
        public IFunctionInfoProvider FunctionInfoProvider { get; set; }

    }//end of class
}
