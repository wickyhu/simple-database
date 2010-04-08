using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace SimpleDatabase
{
    public partial class Db
    {      
        public DbParameter CreateParameter(string fieldName)
        {
            return CreateParameter(fieldName, null);
        }

        private void SetParameterValue(DbParameter p, object value)
        {
            if (value == null)
            {
                p.Value = DBNull.Value;
                p.Size = 1;
            }
            else
            {
                p.Value = value;

                if (p.Value is byte[])
                {
                    byte[] bytes = (byte[])p.Value;
                    p.Size = bytes.Length;
                }
                else if (p.Value is string)
                {
                    string s = (string)p.Value;
                    if (s.Length == 0)
                        p.Size = 1;
                }
            }
        }

        public DbParameter CreateParameter(string fieldName, object value)
        {
            DbParameter p = DbFactory.CreateParameter();
            if (!String.IsNullOrEmpty(fieldName))
            {
                p.ParameterName = GetParameterName(fieldName);
            }

            SetParameterValue(p, value);
            return p;
        }

        public DbParameter CreateParameter(DbField field, object value)
        {
            DbParameter p = CreateParameter(field.FieldName, value);
            p.DbType = field.DbType;
            if (field.ProviderType >= 0)
            {
                string pName = DbMetadata.ParameterTypeName;
                if (!String.IsNullOrEmpty(pName))
                {
                    Type t = p.GetType();
                    PropertyInfo pi = t.GetProperty(pName);
                    pi.SetValue(p, field.ProviderType, null);
                }
            }

            //SetParameterValue(p, value);
            return p;
        }

        public string GetParameterMarker(string fieldName)
        {
            //this.DataSourceInfo.ParameterMarkerFormat
            return String.Format(this.DbMetadata.ParameterMarker, fieldName);
        }

        public string GetParameterName(string fieldName)
        {
            return String.Format(this.DbMetadata.ParameterNameMarker, fieldName);
        }
    }
}
