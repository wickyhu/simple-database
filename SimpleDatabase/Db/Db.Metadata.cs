using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Linq;
using SimpleControls;
using IQToolkit.Data;
using IQToolkit.Data.Common;

namespace SimpleDatabase
{
    public class DbMetadata
    {
        static DbMetadata()
        {
            _list = new List<DbMetadata>();
            DbMetadata dm;           

            Stream s = null;
            string fileName = "Db.config";
            XmlDocument xd = null;
            try
            {
                if (File.Exists(fileName))
                {
                    s = File.Open(fileName, FileMode.Open, FileAccess.Read);
                }
                else
                {
                    s = Assembly.GetExecutingAssembly().GetManifestResourceStream("SimpleDatabase." + fileName);
                }
                xd = new XmlDocument();
                xd.Load(s);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (s != null) s.Close();
            }
            if (xd != null)
            {
                XmlNode dbNode = xd.SelectSingleNode(@"//databases");
                if (dbNode != null)
                {
                    XmlNodeList nl = dbNode.SelectNodes(@"//add");
                    foreach (XmlNode n in nl)
                    {
                        dm = new DbMetadata();
                        dm.Name = n.Attributes["name"].Value;
                        dm.ProviderName = n.Attributes["providerName"].Value;
                        dm.DbType = (DbTypes)Enum.Parse(typeof(DbTypes), n.Attributes["dbType"].Value);
                        dm.ParameterMarker = n.Attributes["parameterMarker"].Value;
                        dm.ParameterNameMarker = n.Attributes["parameterNameMarker"].Value;
                        //dm.QuoteMarker = n.Attributes["quoteMarker"].Value;
                        dm.ParameterTypeName = n.Attributes["parameterType"].Value;
                        dm.DbEntityProviderName = n.Attributes["entityProviderName"].Value;
                        dm.SchemaInfoTypeName = n.Attributes["schemaInfoType"].Value;

                        _list.Add(dm);
                    }
                }
            }
        }

        public string Name { get; private set; }
        public string ProviderName { get; private set; }
        public DbTypes DbType { get; private set; }
        public string ParameterMarker { get; private set; }
        public string ParameterNameMarker { get; private set; }
        //public string QuoteMarker { get; private set; }
        public string SchemaInfoTypeName { get; private set; }
        public string DbEntityProviderName { get; private set; }
        public string ParameterTypeName { get; private set; }


        Type _schemaInfoType = null;
        public Type SchemaInfoType
        {
            get
            {
                if (_schemaInfoType == null) {
                    _schemaInfoType = Type.GetType(SchemaInfoTypeName, false, true);
                }
                return _schemaInfoType;                
            }
        }

        Type _queryLanguageType = null;
        public Type QueryLanguageType
        {
            get
            {
                if (_queryLanguageType == null)
                {
                    _queryLanguageType = FindInstancesIn(typeof(QueryLanguage), DbEntityProviderName).FirstOrDefault();
                }
                return _queryLanguageType;
            }
        }

        Type _queryProviderType = null;
        public Type QueryProviderType
        {
            get
            {
                if (_queryProviderType == null)
                {
                    _queryProviderType = FindInstancesIn(typeof(DbEntityProvider), DbEntityProviderName).FirstOrDefault();
                }
                return _queryProviderType;
            }
        }

        QueryLanguage _queryLanguage;
        public QueryLanguage QueryLanguage
        {
            get
            {
                if (_queryLanguage == null)
                {
                    _queryLanguage = (QueryLanguage)Activator.CreateInstance(QueryLanguageType);
                }
                return _queryLanguage;
            }
        }


        static List<DbMetadata> _list;
        public static List<DbMetadata> List
        {
            get { return _list; }
        }

        private static DbMetadata GetByProviderName(string providerName)
        {
            foreach (DbMetadata m in List)
            {
                if (m.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase))
                    return m;
            }
            return null;
        }

        private static DbMetadata GetByName(string name)
        {
            foreach (DbMetadata m in List)
            {                
                if (m.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return m;
            }
            return null;
        }

        public static DbMetadata Get(Db db)
        {
            DbMetadata m = GetByProviderName(db.ProviderInvariantName);
            if (m == null)
                m = GetByName(db.DataSourceInfo.DataSourceProductName);
            return m;
        }

        public override string ToString()
        {
            return ProviderName;
        }


        private static IEnumerable<Type> FindInstancesIn(Type type, string assemblyName)
        {
            Assembly assembly = GetAssemblyForNamespace(assemblyName);
            if (assembly != null)
            {
                foreach (var atype in assembly.GetTypes())
                {
                    if (string.Compare(atype.Namespace, assemblyName, 0) == 0
                        && type.IsAssignableFrom(atype))
                    {
                        yield return atype;
                    }
                }
            }
        }

        private static Assembly GetAssemblyForNamespace(string nspace)
        {
            foreach (var assem in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assem.FullName.Contains(nspace))
                {
                    return assem;
                }
            }

            return Load(nspace + ".dll");
        }

        private static Assembly Load(string name)
        {
            // try to load it.
            try
            {
                return Assembly.LoadFrom(name);
            }
            catch
            {
            }
            return null;
        }

    }//end of class
}
