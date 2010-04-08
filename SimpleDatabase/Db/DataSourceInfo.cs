using System;
using System.Text;
using System.Data;
using System.Data.Common;
using SimpleControls;

namespace SimpleDatabase
{
    //[Serializable]
    public class DataSourceInfo
    {
        public string CompositeIdentifierSeparatorPattern { get; private set; }
        public string DataSourceProductName { get; private set; }
        public string DataSourceProductVersion { get; private set; }
        public string DataSourceProductVersionNormalized { get; private set; }
        public GroupByBehavior GroupByBehavior { get; private set; }
        public string IdentifierPattern { get; private set; }
        public IdentifierCase IdentifierCase { get; private set; }
        public bool OrderByColumnsInSelect { get; private set; }
        public string ParameterMarkerFormat { get; private set; }
        public string ParameterMarkerPattern { get; private set; }
        public int ParameterNameMaxLength { get; private set; }
        public string ParameterNamePattern { get; private set; }
        public string QuotedIdentifierPattern { get; private set; }
        public IdentifierCase QuotedIdentifierCase { get; private set; }
        public string StatementSeparatorPattern { get; private set; }
        public string StringLiteralPattern { get; private set; }
        public SupportedJoinOperators SupportedJoinOperators { get; private set; }

        public DataSourceInfo(Db db)
        {
            using (DbConnectionInfo cn = db.CreateConnection())
            {
                DataTable dt = cn.GetSchema("DataSourceInformation");
                DataRow dr = dt.Rows[0];
                CompositeIdentifierSeparatorPattern = dr["CompositeIdentifierSeparatorPattern"].IfNull(String.Empty);
                CompositeIdentifierSeparatorPattern = dr["CompositeIdentifierSeparatorPattern"].IfNull(String.Empty);
                DataSourceProductName = dr["DataSourceProductName"].IfNull(String.Empty);
                DataSourceProductVersion = dr["DataSourceProductVersion"].IfNull(String.Empty);
                GroupByBehavior = (GroupByBehavior)dr["GroupByBehavior"].IfNull(GroupByBehavior.Unknown);
                DataSourceProductVersionNormalized = dr["DataSourceProductVersionNormalized"].IfNull(String.Empty);
                IdentifierCase = (IdentifierCase)dr["IdentifierCase"].IfNull(IdentifierCase.Unknown);
                IdentifierPattern = dr["IdentifierPattern"].IfNull(String.Empty);
                OrderByColumnsInSelect = dr["OrderByColumnsInSelect"].IfNull(false);
                ParameterMarkerFormat = dr["ParameterMarkerFormat"].IfNull(String.Empty);
                ParameterMarkerPattern = dr["ParameterMarkerPattern"].IfNull(String.Empty);
                ParameterNameMaxLength = dr["ParameterNameMaxLength"].IfNull(0);
                ParameterNamePattern = dr["ParameterNamePattern"].IfNull(String.Empty);
                QuotedIdentifierCase = (IdentifierCase)dr["QuotedIdentifierCase"].IfNull(IdentifierCase.Unknown);
                QuotedIdentifierPattern = dr["QuotedIdentifierPattern"].IfNull(String.Empty);
                StatementSeparatorPattern = dr["StatementSeparatorPattern"].IfNull(String.Empty);
                StringLiteralPattern = dr["StringLiteralPattern"].IfNull(String.Empty);
                SupportedJoinOperators = (SupportedJoinOperators)dr["SupportedJoinOperators"].IfNull(SupportedJoinOperators.None);
            }
        }

    } //end of DataSourceInformation

}

