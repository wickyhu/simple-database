using System;
using System.Data;
using System.Data.Common;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using IQToolkit;

namespace SimpleDatabase
{
    public class DbFieldMemberInfo
    {
        public MemberInfo Member { get; private set; }
        public string[] SplittedFullName { get; private set; }

        string _fullName;
        public string FullName
        {
            get { return _fullName; }
            private set
            {
                _fullName = value;
                if (String.IsNullOrEmpty(value))
                {
                    SplittedFullName = null;
                }
                else
                {
                    SplittedFullName = value.Split('.');
                }
            }
        }

        public DbFieldMemberInfo(string fullName, MemberInfo memberInfo)
        {
            FullName = fullName;
            Member = memberInfo;
        }

        public override string ToString()
        {
            return FullName;
        }
    }

    public class EmbeddedInfo
    {
        public DbEmbeddedAttribute EmbeddedAttribute { get; private set; }
        public MemberInfo Member { get; private set; }

        public EmbeddedInfo(MemberInfo mi, DbEmbeddedAttribute ea)
        {
            Member = mi;
            EmbeddedAttribute = ea;
        }

        public override string ToString()
        {
            return DbTableEntityMapping.GetFullMemberName(Member.DeclaringType.Name, Member.Name);
        }
    }

    public class AssociationInfo
    {
        public MemberInfo Member { get; private set; }
        public List<MemberInfo> KeyMembers { get; private set; }
        public Type RelatedEnetityType { get; private set; }
        public List<MemberInfo> RelatedKeyMembers { get; private set; }

        public bool IsIncluded { get; set; }
        public bool IsDeferLoaded { get; set; }

        public AssociationInfo(MemberInfo mi, List<MemberInfo> keyMembers, Type relatedEntityType, List<MemberInfo> relatedKeyMembers)
        {
            Member = mi;
            KeyMembers = keyMembers;
            RelatedEnetityType = relatedEntityType;
            RelatedKeyMembers = relatedKeyMembers;

            DbAssociationAttribute aa = mi.GetDbAssociationAttribute();
            if (aa == null)
            {
                IsIncluded = false;
                IsDeferLoaded = false;
            }
            else
            {
                IsIncluded = aa.IsIncluded;
                IsDeferLoaded = aa.IsDeferLoaded;
            }
        }

        public override string ToString()
        {
            return DbTableEntityMapping.GetFullMemberName(Member.DeclaringType.Name, Member.Name);
        }
    }

    public class DbTableEntityMapping
    {
        public Db Db { get; private set; }
        public Type EntityType { get; private set; }
        public DbTable Table { get; private set; }

        /// <summary>
        /// Field Name -> Member Info
        /// </summary>
        public Dictionary<string, DbFieldMemberInfo> DbFieldMemberInfos { get; private set; }
        /// <summary>
        /// Member Name -> DbField 
        /// </summary>
        public Dictionary<string, DbField> DbFieldInfos { get; private set; }
        /// <summary>
        /// Member Name -> Embedded
        /// </summary>
        public Dictionary<string, EmbeddedInfo> EmbeddedInfos { get; private set; }
        /// <summary>
        /// Memeber Name -> Association
        /// </summary>
        public Dictionary<string, AssociationInfo> AssociationInfos { get; private set; }

        public const BindingFlags MemberBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.IgnoreCase;

        void BuildMemberInfo(Type type, string parentTypeFullName)
        {
            bool notEmbedded = String.IsNullOrEmpty(parentTypeFullName);

            List<MemberInfo> list = new List<MemberInfo>();
            list.AddRange(type.GetProperties(MemberBindingFlags));
            list.AddRange(type.GetFields(MemberBindingFlags));

            foreach (MemberInfo mi in list)
            {
                Type pType = TypeHelper.GetMemberType(mi);
                if (pType == null) continue;

                if (!Db.DbMetadata.QueryLanguage.IsScalar(pType)
                    && !pType.IsEnum
                    && !pType.Equals(typeof(System.Object))
                    )
                {
                    #region Embedded
                    DbEmbeddedAttribute ea = mi.GetEmbeddedAttribute();
                    if (ea == null) continue;

                    EmbeddedInfos.Add(mi.Name, new EmbeddedInfo(mi, ea));
                    BuildMemberInfo(pType, notEmbedded ? pType.Name : GetFullMemberName(parentTypeFullName, pType.Name)
                        );
                    #endregion Embedded

                    continue;
                }

                DbFieldAttribute fa = mi.GetDbFieldAttribute();
                string columnName, pName;

                if (notEmbedded) pName = mi.Name;
                else pName = GetNestedPropertyName(type, mi.Name);

                if (fa != null && !String.IsNullOrEmpty(fa.Name))
                    columnName = fa.Name;
                else if (notEmbedded)
                    columnName = mi.Name;
                else
                    columnName = GetNestedFieldName(type, mi.Name);

                if (Table.ContainsField(columnName))
                {
                    string fullName = String.IsNullOrEmpty(parentTypeFullName) ? mi.Name : GetFullMemberName(parentTypeFullName, pType.Name);
                    DbFieldMemberInfos.Add(columnName, new DbFieldMemberInfo(fullName, mi));
                    DbFieldInfos.Add(pName, Table.Fields[columnName]);
                }
            }
        }

        internal void BuildAssociationInfo(Db db)
        {
            Type type = EntityType;

            List<MemberInfo> list = new List<MemberInfo>();
            list.AddRange(type.GetProperties(MemberBindingFlags));
            list.AddRange(type.GetFields(MemberBindingFlags));

            foreach (MemberInfo mi in list)
            {
                Type pType = TypeHelper.GetMemberType(mi);
                if (pType == null
                    || Db.DbMetadata.QueryLanguage.IsScalar(pType)
                    || pType.Equals(typeof(System.Object))
                    )
                    continue;

                Type enumerableType = TypeHelper.FindIEnumerable(pType);
                if (enumerableType == null)
                {
                    DbTableEntityMapping relatedMapping = Db.GetDbTableEntityMapping(pType);
                    if (relatedMapping == null) continue;

                    SchemaForeignKey fk = Table.ForeignKeys.Where(x => x.ReferencedTableName.Equals(relatedMapping.Table.TableName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    if (fk == null) return;

                    List<MemberInfo> keyMembers = new List<MemberInfo>(fk.FieldNames.Count);
                    foreach (string s in fk.FieldNames)
                    {
                        keyMembers.Add(this.DbFieldMemberInfos[s].Member);
                    }
                    List<MemberInfo> relatedKeyMembers = new List<MemberInfo>(fk.ReferencedFieldNames.Count);
                    foreach (string s in fk.ReferencedFieldNames)
                    {
                        relatedKeyMembers.Add(relatedMapping.DbFieldMemberInfos[s].Member);
                    }

                    AssociationInfo ai = new AssociationInfo(mi, keyMembers, relatedMapping.EntityType, relatedKeyMembers);
                    AssociationInfos.Add(mi.Name, ai);
                    if (ai.IsIncluded)
                    {
                        db.Cache.IncludedAssociationInfos.Add(ai);
                        db.Policy.Include(ai.Member);
                    }
                }
                else if (pType.IsGenericType)
                {
                    pType = pType.GetGenericArguments().FirstOrDefault();
                    if (pType == null) continue;

                    DbTableEntityMapping relatedMapping = Db.GetDbTableEntityMapping(pType);
                    if (relatedMapping == null) continue;

                    DbTable relatedTable = relatedMapping.Table;
                    SchemaForeignKey fk = relatedTable.ForeignKeys.Where(x => x.ReferencedTableName.Equals(Table.TableName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    if (fk == null) return;

                    List<MemberInfo> keyMembers = new List<MemberInfo>(fk.ReferencedFieldNames.Count);
                    foreach (string s in fk.ReferencedFieldNames)
                    {
                        keyMembers.Add(this.DbFieldMemberInfos[s].Member);
                    }
                    List<MemberInfo> relatedKeyMembers = new List<MemberInfo>(fk.FieldNames.Count);
                    foreach (string s in fk.FieldNames)
                    {
                        relatedKeyMembers.Add(relatedMapping.DbFieldMemberInfos[s].Member);
                    }

                    AssociationInfo ai = new AssociationInfo(mi, keyMembers, relatedMapping.EntityType, relatedKeyMembers);
                    AssociationInfos.Add(mi.Name, ai);
                    if (ai.IsIncluded)
                    {
                        db.Cache.IncludedAssociationInfos.Add(ai);
                        db.Policy.Include(ai.Member);
                    }
                }
            }
            
        }

        public DbTableEntityMapping(Db db, Type tableType, DbTable table)
        {
            EntityType = tableType;
            Table = table;
            Db = db;

            DbFieldMemberInfos = new Dictionary<string, DbFieldMemberInfo>(Table.Fields.Count, StringComparer.OrdinalIgnoreCase);
            DbFieldInfos = new Dictionary<string, DbField>(StringComparer.OrdinalIgnoreCase);
            EmbeddedInfos = new Dictionary<string, EmbeddedInfo>(StringComparer.OrdinalIgnoreCase);
            AssociationInfos = new Dictionary<string, AssociationInfo>(StringComparer.OrdinalIgnoreCase);

            BuildMemberInfo(tableType, null);
        }

        public override string ToString()
        {
            return String.Format("Table: {0}, Type: {1}", Table.TableName, EntityType.Name);
        }

        public override int GetHashCode()
        {
            return String.Concat(Db.ConnectionString, EntityType.FullName, Table.TableName.ToUpper()).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            DbTableEntityMapping o = obj as DbTableEntityMapping;
            return o == null ? false : this.GetHashCode() == o.GetHashCode();
        }


        private string GetNestedPropertyName(Type t, string name)
        {
            return GetFullMemberName(t.Name, name);
        }

        private string GetNestedFieldName(Type t, string name)
        {
            return String.Format("{0}{1}", t.Name, name);
        }

        public static string GetFullMemberName(string parentTypeFullName, string memberName)
        {
            return String.Format("{0}.{1}", parentTypeFullName, memberName);
        }

    }

}
