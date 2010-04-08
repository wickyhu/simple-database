using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using IQToolkit;
using IQToolkit.Data;
using IQToolkit.Data.Common;

namespace SimpleDatabase
{
    public class SimpleMapping : BasicMapping
    {
        public Db Db { get; private set; }

        public SimpleMapping(Db db)
            : base()
        {
            Db = db;
        }

        public override QueryMapper CreateMapper(QueryTranslator translator)
        {
            return new SimpleMapper(Db, this, translator);
        }

        public override MappingEntity GetEntity(Type elementType, string tableId)
        {
            DbTableEntityMapping tm = Db.GetDbTableEntityMapping(elementType);
            if (tm == null) return null;
            return new SimpleMappingEntity(tm);
        }

        public override string GetTableName(MappingEntity entity)
        {
            DbTableEntityMapping tm = ((SimpleMappingEntity)entity).TableTypeMapping;
            return tm.Table.TableName;
        }

        public DbField FindDbField(MappingEntity entity, MemberInfo member)
        {
            DbTableEntityMapping tm = ((SimpleMappingEntity)entity).TableTypeMapping;

            string pName;
            if (entity.EntityType.Equals(member.ReflectedType))
                pName = member.Name;
            else
                pName = DbTableEntityMapping.GetFullMemberName(member.DeclaringType.Name, member.Name);

            if (tm.DbFieldInfos.ContainsKey(pName))
                return tm.DbFieldInfos[pName];
            return null;
        }

        public override bool IsPrimaryKey(MappingEntity entity, MemberInfo member)
        {
            DbField f = FindDbField(entity, member);
            return f == null ? false : f.IsKey;
        }

        public override bool IsColumn(MappingEntity entity, MemberInfo member)
        {
            return FindDbField(entity, member) != null;
        }

        public override bool IsMapped(MappingEntity entity, MemberInfo member)
        {
            //return IsColumn(entity, member);
            return true;
        }

        public override string GetColumnName(MappingEntity entity, MemberInfo member)
        {
            DbField f = FindDbField(entity, member);
            return f == null ? null : f.FieldName;
        }

        public override bool IsGenerated(MappingEntity entity, MemberInfo member)
        {
            DbField f = FindDbField(entity, member);
            return f == null ? false : f.IsAutoIncrement;
        }

        public override IEnumerable<MemberInfo> GetMappedMembers(MappingEntity entity)
        {
            DbTableEntityMapping tm = ((SimpleMappingEntity)entity).TableTypeMapping;
            foreach (DbFieldMemberInfo mi in tm.DbFieldMemberInfos.Values)
            {
                yield return mi.Member;
            }
            foreach (AssociationInfo ai in tm.AssociationInfos.Values)
            {
                yield return ai.Member;
            }
        }


        public override bool IsAssociationRelationship(MappingEntity entity, MemberInfo member)
        {
            DbTableEntityMapping tm = ((SimpleMappingEntity)entity).TableTypeMapping;
            return tm.AssociationInfos.ContainsKey(member.Name);
        }

        public override IEnumerable<MemberInfo> GetAssociationKeyMembers(MappingEntity entity, MemberInfo member)
        {
            DbTableEntityMapping tm = ((SimpleMappingEntity)entity).TableTypeMapping;
            return tm.AssociationInfos[member.Name].KeyMembers;
        }

        public override IEnumerable<MemberInfo> GetAssociationRelatedKeyMembers(MappingEntity entity, MemberInfo member)
        {
            DbTableEntityMapping tm = ((SimpleMappingEntity)entity).TableTypeMapping;
            return tm.AssociationInfos[member.Name].RelatedKeyMembers;
        }

        public override bool IsRelationshipSource(MappingEntity entity, MemberInfo member)
        {
            //DbTableEntityMapping tm = ((SimpleImplicitMappingEntity)entity).TableTypeMapping;
            //AssociationInfo ai = tm.AssociationInfos[member.Name];
            return !IsRelationshipTarget(entity, member);

        }

        public override bool IsRelationshipTarget(MappingEntity entity, MemberInfo member)
        {
            //DbTableEntityMapping tm = ((SimpleImplicitMappingEntity)entity).TableTypeMapping;
            //AssociationInfo ai = tm.AssociationInfos[member.Name];            
            return typeof(IEnumerable).IsAssignableFrom(TypeHelper.GetMemberType(member));
        }

    }
}
