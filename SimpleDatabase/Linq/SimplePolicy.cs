using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using IQToolkit;
using IQToolkit.Data;
using IQToolkit.Data.Common;

namespace SimpleDatabase
{

    public class SimplePolicy : EntityPolicy
    {
        public Db Db { get; private set; }
        
        public SimplePolicy(Db db)
            : base()
        {
            Db = db;
            foreach (AssociationInfo ai in db.Cache.IncludedAssociationInfos)
            {
                if (ai.IsIncluded)
                {
                    this.Include(ai.Member, ai.IsDeferLoaded);
                }
            }
        }

        public void Exclude(MemberInfo member)
        {
            if (this.included.Contains(member))
                this.included.Remove(member);
        }

    }

}
