using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Clones a sensor or group within PRTG.
    /// </summary>
    public abstract class CloneSensorOrGroup<T> : CloneObject<T>
    {
        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        /// <param name="objectId">The ID of the object to clone.</param>
        /// <param name="name">The name of the object to clone. If <see cref="Name"/> is not specified, this value will be used.</param>
        /// <param name="getObjects">The function to execute to retrieve the cloned object that was created.</param>
        protected void ProcessRecordEx(int objectId, string name, Func<int, List<T>> getObjects)
        {
            var nameToUse = Name ?? name;
            ExecuteOperation(() => CloneObject(objectId, nameToUse, getObjects), name, objectId);
        }

        private void CloneObject(int objectId, string nameToUse, Func<int, List<T>> getObjects)
        {
            var id = client.CloneObject(objectId, nameToUse, DestinationId);

            if (Resolve)
            {
                ResolveObject(id, getObjects);
            }
            else
            {
                var response = new PSObject();
                response.Properties.Add(new PSNoteProperty("Id", id));
                response.Properties.Add(new PSNoteProperty("Name", nameToUse));

                WriteObject(response);
            }
        }
    }
}