using System;
using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Reloads config files including sensor lookups and device icons used by PRTG Network Monitor.</para>
    /// 
    /// <para type="description">The Load-PrtgConfigFile cmdlet reloads miscellaneous config files used by PRTG Network Monitor,
    /// including lookup definitions used for customizing how sensor values are displayed, as well as other miscellaneous config
    /// files including device icons, report templates and language files.</para>
    /// <para type="description">Config files used by PRTG are automatically reloaded whenever the PRTG Core Service is restarted.
    /// To prevent having to completely restart PRTG whenever a config file is installed or modified, Load-PrtgConfigFile can instead be used.</para>
    /// <para type="description">Device icons, report templates and language files are refreshed together by a single request,
    /// categorised by PrtgAPI as the <see cref="ConfigFileType.General"/> config file type. Sensor lookups are reloaded separately,
    /// under the <see cref="ConfigFileType.Lookups"/> file type.</para>
    /// 
    /// <example>
    ///     <code>C:\> Load-PrtgConfigFile General</code>
    ///     <para>Reload any device icons, report templates or language files that may have been installed or modified since PRTG started.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Load-PrtgConfigFile Lookups</code>
    ///     <para>Loads or reloads any sensor lookups that may have been changed or installed since PRTG was started.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Administrative-Tools#load-config-files-1">Online version:</para>
    /// </summary>
    [Cmdlet(VerbsData.Sync, "PrtgConfigFile", SupportsShouldProcess = true)]
    public class LoadPrtgConfigFile : PrtgCmdlet
    {
        /// <summary>
        /// <para type="description">The type of config files to reload.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public ConfigFileType Type { get; set; }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ShouldProcess($"Load PRTG {GetFileTypeDescription(Type)}"))
            {
                client.LoadConfigFiles(Type);
            }
        }

        private string GetFileTypeDescription(ConfigFileType fileType)
        {
            if (fileType == ConfigFileType.General)
                return "Custom Files";
            if (fileType == ConfigFileType.Lookups)
                return "Lookups";

            throw new NotImplementedException($"Don't know how to handle file type '{fileType}'.");
        }
    }
}
