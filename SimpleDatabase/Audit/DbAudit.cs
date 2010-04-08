using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;

namespace SimpleDatabase
{
    [DbTable(AuditType=TableAuditTypes.None)]
    public class DbAudit
    {
        public long Id { get; set; }
        public DateTime CreatedOn { get; private set; }

        public string Action { get; set; }
        public string TableName { get; set; }

        public string UserName { get; set; }
        public string HostName { get; set; }
        public string IPAddress { get; set; }
        public string OSUser { get; set; }

        public int FunctionId { get; set; }

        public string Remarks { get; set; }

        //public List<DbAuditDetail> Details;

        public DbAudit()
        {
        }

        public static DbAudit Create(Db db, DbAuditArgs arg)
        {
            if (!db.IsAuditEnabled(arg.Table.TableName)) 
                return null;

            DbAudit a = new DbAudit();
            INextIdProvider nextId = db.NextIdProvider;
            if (nextId != null)
            {
                a.Id = nextId.Get();
            }
            a.Action = arg.Action.ToString();
            a.TableName = arg.Table.TableName;
            IUserInfoProvider userInfo = db.UserInfoProvider;
            if (userInfo != null)
            {
                a.UserName = userInfo.UserName;
                a.HostName = userInfo.HostName;
                a.IPAddress = userInfo.IPAddress;
                a.OSUser = userInfo.OSUser;
            }
            IFunctionInfoProvider funcInfo = db.FunctionInfoProvider;
            if (funcInfo != null)
            {
                a.FunctionId = funcInfo.Id;
            }

            if (arg.Data != null && !(arg.Data is Dictionary<string, object>))
            {
                a.Remarks = arg.Data.ToString();
            }
            else if(arg.Action == ExecTypes.Delete) 
            {
                a.Remarks = arg.Sql;
            }

            db.Insert<DbAudit>(a);

            //details
            if (arg.Data is Dictionary<string, object>)
            {
                DbAuditDetail.Create(db, a, arg);
            }

            return a;
        }
        
    }
}
