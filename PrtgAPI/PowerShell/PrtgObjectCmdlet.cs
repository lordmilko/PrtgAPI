using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Objects.Shared;

namespace PrtgAPI.PowerShell
{
    public abstract class PrtgObjectCmdlet<T> : PrtgCmdlet where T : PrtgObject
    {
        protected abstract List<T> GetRecords();

        protected override void ProcessRecord()
        {
            var records = GetRecords();

            WriteList(records);
        }
    }
}
