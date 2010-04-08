using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleDatabase
{
    public interface INextIdProvider
    {
        long Get();
        //int CacheSize { get; set; }
    }
}
