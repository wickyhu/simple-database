using System;
using System.Data;
using System.Data.Common;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace SimpleDatabase
{
    public partial class Db
    {         
        internal DbFieldMemberInfo GetFieldMemberInfo(Type t, string propertyName)
        {
            DbTableEntityMapping tm = GetDbTableEntityMapping(t);
            if (tm.DbFieldInfos.ContainsKey(propertyName))
            {
                string columnName = tm.DbFieldInfos[propertyName].FieldName;
                if (tm.DbFieldMemberInfos.ContainsKey(columnName))
                {
                    return tm.DbFieldMemberInfos[columnName];
                }
            }
            else if (tm.DbFieldMemberInfos.ContainsKey(propertyName))
            {
                return tm.DbFieldMemberInfos[propertyName];
            }
            return null;
        }
      
        protected bool ContainsProperty(Type t, string propertyName)
        {
            return GetFieldMemberInfo(t, propertyName) != null;
        }      


        internal object FindProperObject(DbFieldMemberInfo mi, object obj)
        {
            if (mi.SplittedFullName.Length <= 1)
                return obj;

            Type t = obj.GetType();
            object o = obj;
            PropertyInfo pi;
            for (int i = 0; i < mi.SplittedFullName.Length; i++)
            {
                string pName = mi.SplittedFullName[i];
                pi = t.GetProperty(pName, DbTableEntityMapping.MemberBindingFlags);
                if (pi == null) break;
                o = pi.GetValue(o, null);
                if (o == null) break;
            }
            return o;
        }

        protected object GetPropertyValue(Type t, object obj, string propertyName)
        {
            DbFieldMemberInfo mi = GetFieldMemberInfo(t, propertyName);

            object o = FindProperObject(mi, obj);
            if (o != null)
            {
                switch (mi.Member.MemberType)
                {
                    case MemberTypes.Property:
                        {
                            PropertyInfo pi = (PropertyInfo)mi.Member;
                            if (pi.CanRead)
                            {
                                return pi.GetValue(o, null);
                            }
                        }
                        break;
                    case MemberTypes.Field:
                        {
                            FieldInfo fi = (FieldInfo)mi.Member;
                            return fi.GetValue(o);
                        }
                }
            }

            return null;
        }    

        protected bool SetPropertyValue(Type t, object obj, string propertyName, object v)
        {
            DbFieldMemberInfo mi = GetFieldMemberInfo(t, propertyName);
            if (mi == null) return false;

            object o = FindProperObject(mi, obj);
            if (o == null) return false;

            bool isSet = false;
            switch (mi.Member.MemberType)
            {
                case MemberTypes.Property:
                    {
                        PropertyInfo pi = (PropertyInfo)mi.Member;
                        if (pi.CanWrite)
                        {
                            object converted = DbHelper.ConvertType(v, pi.PropertyType);
                            pi.SetValue(o, converted, null);
                            isSet = true;
                        }                        
                    }
                    break;
                case MemberTypes.Field:
                    {
                        FieldInfo fi = (FieldInfo)mi.Member;
                        object converted = DbHelper.ConvertType(v, fi.FieldType);
                        fi.SetValue(o, converted);
                        isSet = true;
                    }
                    break;
                    
            }
            return isSet;
        }

    }//end 
}
