using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Requests a PRTG Network Monitor server create a backup of its configuration database.</para>
    /// 
    /// <para type="description">The Backup-PrtgConfig cmdlet requests a PRTG Network Monitor server create a backup of its configuration
    /// database after first writing the current running configuration to disk. Configuration backups are stored under the
    /// Configuration Auto-Backups folder, by default located at C:\ProgramData\Paessler\PRTG Network Monitor\Configuration Auto-Backups</para>
    /// 
    /// <para type="description">When this cmdlet is executed, PRTG will asynchronously start creating a configuration backup. As such,
    /// this cmdlet will return before the backup has completed. Depending on the size of your configuration database, this may take
    /// several seconds to complete. In order to guarantee a backup has been successfully completed, it is recommended to probe the contents
    /// of the Configuration Auto-Backups folder before and after running this script, to identify the backup file that was created
    /// as a result of running this cmdlet. The created backup can also be identified by parsing the DateTime timestamp in the created filename,
    /// typically "PRTG Configuration (Snapshot yyyy-MM-dd HH-mm-ss)"</para>
    /// 
    /// <example>
    ///     <code>C:\> Backup-PrtgConfig</code>
    ///     <para>Trigger a backup of the PRTG Configuration Database.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Administrative-Tools#backup-config-database-1">Online version:</para>
    /// </summary>
    [Cmdlet(VerbsData.Backup, "PrtgConfig", SupportsShouldProcess = true)]
    public class BackupPrtgConfig : PrtgCmdlet
    {
        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ShouldProcess("Backup PRTG Configuration"))
            {
                client.BackupConfigDatabase();
            }
        }
    }
}
