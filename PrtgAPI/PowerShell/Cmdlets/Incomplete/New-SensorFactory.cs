using System.Management.Automation;

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
