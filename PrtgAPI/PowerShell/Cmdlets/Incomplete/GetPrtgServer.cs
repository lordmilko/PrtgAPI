using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Retrieves the current session's <see cref="PrtgClient"/> 
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "PrtgServer")]
    public class GetPrtgServer : PrtgCmdlet
    {
        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            //client.blah();
            //WriteObject(client.GetStatus());

            //find out how prtgshell or whatever showed the server status, that looked pretty good

            WriteObject(PrtgSessionState.Client);
        }

        /*
         * 1. iterate over getsensors, devices etc returning a few at a time till we have all of them
         * 2. change getsensors to return an ienumerable and do it all internally?
         * 3. implement async?
         * 4. how do cmdlets normally process long running items
         * 5. if we ctrl+c while iterating in our cmdlet over a long running item does it stop?
                *  yes!
                *  
              i think we should change this to be get-prtgclient, and then have an actual one that gets deets about the prtg server like version details etc
         * 
         */
    }
}
