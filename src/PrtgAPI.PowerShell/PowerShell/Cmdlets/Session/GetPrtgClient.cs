using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Runtime.Versioning;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves the current session's <see cref="PrtgClient"/></para>
    /// 
    /// <para type="description">The Get-PrtgClient cmdlet allows you to access the <see cref="PrtgClient"/> of the current session
    /// previously created with a call to Connect-PrtgServer. This allows you to view/edit the properties of the <see cref="PrtgClient"/>
    /// previously defined in your call to Connect-PrtgServer, as well as access the raw C# PrtgAPI should you wish to bypass or access
    /// methods not in the PowerShell interface.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-PrtgClient</code>
    ///     <para>View the settings of the session's PrtgClient. If the session does not have a PrtgClient, the cmdlet returns null</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> (Get-PrtgClient).RetryCount = 5</code>
    ///     <para>Change the RetryCount of the PrtgClient to 5</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> (Get-PrtgClient).GetSensors()</code>
    ///     <para>Invoke the GetSensors method of the PrtgClient</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Getting-Started#session-management-1">Online version:</para>
    /// <para type="link">Connect-PrtgServer</para>
    /// <para type="link">Disconnect-PrtgServer</para>
    /// <para type="link">Set-PrtgClient</para>
    /// </summary>
    [OutputType(typeof(PrtgClient))]
    [Cmdlet(VerbsCommon.Get, "PrtgClient")]
    public class GetPrtgClient : PSCmdlet
    {
        /// <summary>
        /// <para type="description">Displays diagnostic information to assist when raising a PrtgAPI issue.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Diagnostic { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            if (Diagnostic)
                WriteDiagnostic();
            else
                WriteObject(PrtgSessionState.Client);
        }

        private void WriteDiagnostic()
        {
            if (PrtgSessionState.Client == null)
                throw new InvalidOperationException("You are not connected to a PRTG Server. Please connect first using Connect-PrtgServer.");

            var versionTable = (Hashtable) GetVariableValue("PSVersionTable");

            var dict = new Dictionary<string, string>
            {
                ["PSVersion"]      = versionTable["PSVersion"].ToString(),
                ["PSEdition"]      = versionTable["PSEdition"].ToString(),
                ["OS"]             = GetOS(versionTable),
                ["PrtgAPIVersion"] = GetPrtgAPIVersion(),
                ["Culture"]        = CultureInfo.CurrentCulture.ToString(),
                ["CLRVersion"]     = GetCLRVersion(),

                ["PrtgVersion"]    = PrtgSessionState.Client.Version.ToString(),
                ["PrtgLanguage"]   = PrtgLanguageOrUnknown()
            };

            var obj = new PSObject();

            foreach(var item in dict)
                obj.Properties.Add(new PSNoteProperty(item.Key, item.Value));

            WriteObject(obj);
        }

        #region Diagnostic Helpers

        [ExcludeFromCodeCoverage]
        private string GetOS(Hashtable versionTable)
        {
            if (PrtgSessionState.PSEdition == PSEdition.Desktop)
            {
                var result = InvokeCommand.InvokeScript("(Get-WmiObject Win32_OperatingSystem).Caption");

                return ValueOrUnknown(result);
            }

            return versionTable["OS"]?.ToString();
        }

        private string GetPrtgAPIVersion()
        {
            var asmPath = typeof(PrtgClient).Assembly.Location;
            var version = FileVersionInfo.GetVersionInfo(asmPath).ProductVersion;

            var versionPlus = version.IndexOf('+');

            //Strip Git hash information off from SourceLink builds
            if (versionPlus != -1)
                version = version.Substring(0, versionPlus);

            return version;
        }

        [ExcludeFromCodeCoverage]
        private string GetCLRVersion()
        {
            if (PrtgSessionState.PSEdition == PSEdition.Desktop)
                return GetFullCLRVersion();

            var framework = Assembly
                .GetEntryAssembly()?
                .GetCustomAttribute<TargetFrameworkAttribute>()?
                .FrameworkName;

            return framework ?? "Unknown";
        }

        [ExcludeFromCodeCoverage]
        private string GetFullCLRVersion()
        {
            var result = InvokeCommand.InvokeScript("(Get-ItemProperty \"HKLM:SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\").Release");

            var value = result.FirstOrDefault()?.ToString();

            if (value == null)
                return "Unknown";

            string version;

            switch (value)
            {
                case "528049": //All
                case "528040": //Windows 10 May 2019
                    version = "4.8";
                    break;

                case "461814": //All
                case "461808": //Windows 10 April 2018/Windows Server 1803
                    version = "4.7.2";
                    break;

                case "461310": //All
                case "461308": //Windows 10/Server 1709
                    version = "4.7.1";
                    break;

                case "460805": //All
                case "460798": //Windows 10 Creators Update
                    version = "4.7";
                    break;

                case "394806": //All
                case "394802": //Windows 10 Anniversary Update/Server 2016
                    version = "4.6.2";
                    break;

                case "394271": //All
                case "394254": //Windows 10 November Update
                    version = "4.6.1";
                    break;

                case "393297": //All
                case "393295": //Windows 10
                    version = "4.6";
                    break;

                case "379893": //All
                    version = "4.5.2";
                    break;

                default:
                    return value;
            }

            if (version != value)
                version = $"{version} ({value})";

            return $".NET Framework {version}";
        }

        [ExcludeFromCodeCoverage]
        private string PrtgLanguageOrUnknown()
        {
            string language;

            if (PrtgSessionState.Client.GetObjectPropertiesRaw(810).TryGetValue("languagefile", out language))
                return language;

            return "Unknown";
        }

        [ExcludeFromCodeCoverage]
        private string ValueOrUnknown(Collection<PSObject> collection)
        {
            return collection.FirstOrDefault()?.ToString() ?? "Unknown";
        }

        #endregion
    }
}
