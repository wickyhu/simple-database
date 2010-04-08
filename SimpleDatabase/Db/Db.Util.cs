using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;

namespace SimpleDatabase
{
    public partial class Db
    {
        public string Quote(string name)
        {
            //return String.Format(DbMetadata.QuoteMarker, name);
            return DbEntityProvider.Language.Quote(name);
        }

    }//end of class
}
