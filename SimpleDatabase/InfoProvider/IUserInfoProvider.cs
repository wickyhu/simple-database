using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleDatabase
{
    public interface IUserInfoProvider
    {
        string UserName { get; }
        string HostName { get; }
        string IPAddress { get; }
        string OSUser { get; }
    }
}
