using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SimpleDatabase
{
    public class KeyTableNextIdProvider : INextIdProvider
    {
        internal class CachedId
        {
            public long NextId;
            public long MaxId;
        }

        string _idTable = "DbIdTable";
        public string IdTable
        {
            get { return _idTable; }
            set { _idTable = value; }
        }

        string _idIndexField = "IdIndex";
        public string IdIndexField
        {
            get { return _idIndexField; }
        }

        string _idValueField = "NextId";
        public string IdValueField
        {
            get { return _idValueField; }
        }

        short _idIndex = 0;
        public short IdIndex
        {
            get { return _idIndex; }
        }

        int _cacheSize = 50;
        public int CacheSize
        {
            get { return _cacheSize; }
            set { _cacheSize = value < 1 ? 1 : value; }
        }

        public Db Db { get; private set; }

        public KeyTableNextIdProvider(Db db)
        {
            Db = db;
        }

        readonly object _lockId = new object();

        public long Get()
        {
            return GetNextId(IdIndex);
        }

        Dictionary<int, CachedId> _cachedId = new Dictionary<int, CachedId>();

        void RetrieveId(int currentIdIndex)
        {
            using (DbConnection cn = Db.CreateAdoConnection())
            {
                using (DbTransaction tr = cn.BeginTransaction())
                {
                    using (DbCommand cm = cn.CreateCommand())
                    {
                        cm.Transaction = tr;

                        cm.CommandText = String.Format("SELECT {0} FROM {1} WHERE {2}={3}", IdValueField, IdTable, IdIndexField, currentIdIndex);
                        long nextId = Convert.ToInt64(cm.ExecuteScalar());
                        cm.CommandText = String.Format("UPDATE {0} SET {1}={1}+{2} WHERE {3}={4}", IdTable, IdValueField, CacheSize, IdIndexField, currentIdIndex);
                        cm.ExecuteNonQuery();

                        CachedId cid;
                        if (!_cachedId.TryGetValue(currentIdIndex, out cid))
                        {
                            cid = new CachedId();
                            _cachedId.Add(currentIdIndex, cid);
                        }
                        cid.NextId = nextId;
                        cid.MaxId = nextId + CacheSize - 1;
                    }
                    tr.Commit();
                }
            }

        }

        long GetNextId(int idIndex)
        {
            lock (_lockId)
            {
                if (!_cachedId.ContainsKey(idIndex))
                {
                    RetrieveId(idIndex);
                }
                CachedId cid = _cachedId[idIndex];
                if (cid.NextId > cid.MaxId)
                {
                    RetrieveId(idIndex);
                }
                long nextId = cid.NextId++;
                return nextId;
            }
        }

    }
}
