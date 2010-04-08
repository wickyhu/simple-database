using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleDatabase
{
    public interface IFunctionInfoProvider
    {
        int Id { get; }
        string Name { get; }
    }
}
