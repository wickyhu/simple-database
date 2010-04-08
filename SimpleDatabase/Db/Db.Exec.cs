using System;
using System.Text;
using System.Data;
using System.Data.Common;

namespace SimpleDatabase
{
    public partial class Db
    {
        public object ExecScalar(DbCommand cm)
        {
            //OnExecuting(ExecTypes.Scalar, cm);

            object r = cm.ExecuteScalar();

            //OnExecuted(ExecTypes.Scalar, cm, r);

            return r;
        }

        public int ExecNonQuery(DbCommand cm)
        {
            //OnExecuting(ExecTypes.NonQuery, cm);

            int r = cm.ExecuteNonQuery();

            //OnExecuted(ExecTypes.NonQuery, cm, r);
            return r;
        }

        public DbDataReader ExecReader(DbCommand cm)
        {
            //OnExecuting(ExecTypes.Reader, cm);

            DbDataReader r = cm.ExecuteReader();

            //OnExecuted(ExecTypes.Reader, cm, null);

            return r;
        }

        public DataTable ExecQuery(DbCommand cm)
        {
            //OnExecuting(ExecTypes.Query, cm);

            DbDataAdapter da = CreateDataAdapter();
            da.SelectCommand = cm;
            DataTable dt = new DataTable();
            da.Fill(dt);

            //OnExecuted(ExecTypes.Query, cm, dt);

            return dt;
        }

        public DataSet ExecQueryEx(DbCommand cm)
        {
            //OnExecuting(ExecTypes.QueryEx, cm);

            DbDataAdapter da = CreateDataAdapter();
            da.SelectCommand = cm;
            DataSet ds = new DataSet();
            da.Fill(ds);

            //OnExecuted(ExecTypes.QueryEx, cm, ds);

            return ds;
        }

    }//end of class
}
