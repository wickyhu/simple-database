using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using IQToolkit.Data;
using IQToolkit.Data.Common;
using SimpleControls;

namespace SimpleDatabase
{
    public static class DbHelper
    {
        public static bool IsEqual(object o1, object o2)
        {
            if (o1.IsNull())
            {
                if (o2.IsNull())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (o2.IsNull())
                {
                    return false;
                }
                else
                {
                    switch (o1.GetType().FullName)
                    {
                        case "System.DateTime":
                            return o1.ToString().Equals(o2.ToString());
                        default:
                            return o1.Equals(o2);
                    }                    
                }
            }
        }

        public static object ConvertType(object v, Type t)
        {
            if (Convert.IsDBNull(v) || v == null)
            {
                return null;
            }

            if (t.IsEnum)
            {
                return Convert.ToInt32(v);
            }

            Type convertionType = t;
            if (convertionType.IsGenericType && convertionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                NullableConverter nullableConverter = new NullableConverter(convertionType);
                convertionType = nullableConverter.UnderlyingType;
            }

            if (v is IConvertible)
            {
                TypeCode sourceTC = Type.GetTypeCode(v.GetType());
                TypeCode destTC = Type.GetTypeCode(convertionType);
                if (sourceTC != destTC)
                {
                    return Convert.ChangeType(v, convertionType);
                }
            }

            return v;
        }

    }//end of class
}
