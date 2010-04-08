using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace SimpleDatabase
{
    public static class AttributeHelper
    {
        public static Attribute GetAttribute(this ICustomAttributeProvider cap, Type attrType)
        {
            Attribute a = null;
            object[] os = cap.GetCustomAttributes(attrType, false);
            if (os != null && os.Length > 0)
            {
                a = os[0] as Attribute;
            }
            return a;
        }

        public static T GetAttribute<T>(this ICustomAttributeProvider cap) where T : Attribute
        {
            return (T) GetAttribute(cap, typeof(T));
        }


        public static DbTableAttribute GetDbTableAttribute(this Type t)
        {
            DbTableAttribute ta = t.GetAttribute<DbTableAttribute>();
            if (ta != null)
            {
                if (String.IsNullOrEmpty(ta.Name))
                {
                    ta.Name = t.Name;
                }
            }
            return ta;
        }

        public static DbFieldAttribute GetDbFieldAttribute(this MemberInfo mi)
        {
            DbFieldAttribute fa = mi.GetAttribute<DbFieldAttribute>();
            if (fa != null)
            {
                if (String.IsNullOrEmpty(fa.Name))
                {
                    fa.Name = mi.Name;
                }
            }
            return fa;
        }

        public static DbEmbeddedAttribute GetEmbeddedAttribute(this MemberInfo mi)
        {
            DbEmbeddedAttribute ea = mi.GetAttribute<DbEmbeddedAttribute>();
            return ea;
        }

        public static DbAssociationAttribute GetDbAssociationAttribute(this MemberInfo mi)
        {
            DbAssociationAttribute aa = mi.GetAttribute<DbAssociationAttribute>();
            return aa;
        }
     
    }
}
