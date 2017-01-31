using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.PowerShell.Cmdlets.Incomplete
{
    class New_SensorFactory : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            //a factoryrecord has a name, a source sensor and a source channel

            /*
             * get-device *fw*|get-sensor *wan*|get-channel "traffic in"|new-sensorfactory -start 20 -name "{device} out" -total
             * 
             * 
             * 
             */
        }
        //maybe have some switches for extra columns, like -total, -average, -variance
    }
}
