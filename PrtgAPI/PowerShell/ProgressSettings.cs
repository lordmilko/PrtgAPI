using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.PowerShell
{
    public class ProgressSettings
    {
        public string ActivityName { get; set; }
        public string InitialDescription { get; set; }
        public int TotalRecords { get; set; }
    }
}
