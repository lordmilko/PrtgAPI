using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using PrtgAPI.PowerShell.GoPrtg;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Base class for all cmdlets that provide GoPrtg functionality.
    /// </summary>
    public abstract class GoPrtgCmdlet : PSCmdlet
    {
        internal const string goPrtgFunction = "__goPrtgGetServers";

        internal bool GoPrtgFunctionInstalled => GetGoPrtgFunction() != null;

        internal string profile => GetVariableValue("global:Profile")?.ToString();

        internal PrtgClient client => PrtgSessionState.Client;

        internal GoPrtgProfile GoPrtgProfile => goPrtgProfile.Value;

        private Lazy<GoPrtgProfile> goPrtgProfile;

        internal GoPrtgCmdlet()
        {
            //Required so XmlDoc2CmdletDoc doesn't throw an exception attempting to call profile => GetVariableValue
            goPrtgProfile = new Lazy<GoPrtgProfile>(() => new GoPrtgProfile(profile));
        }

        internal void AssertProfileExists()
        {
            if (!File.Exists(profile))
                throw new InvalidOperationException("No GoPrtg servers are installed. To install a GoPrtg server, run Install-GoPrtgServer.");
        }

        private CommandInfo GetGoPrtgFunction()
        {
            var function = InvokeCommand.GetCommand(goPrtgFunction, CommandTypes.Function);

            return function;
        }

        internal void LoadFunction(string str)
        {
            InvokeCommand.InvokeScript(str.Replace("function ", "function global:"));
        }

        internal void RemoveFunction()
        {
            InvokeProvider.Item.Remove($@"Function:\{goPrtgFunction}", false);
        }

        internal List<GoPrtgServer> GetServers()
        {
            var servers = InvokeCommand.InvokeScript($"{goPrtgFunction} | ConvertFrom-Csv -Header \"{nameof(GoPrtgServer.Server)}\",\"{nameof(GoPrtgServer.Alias)}\",\"{nameof(GoPrtgServer.UserName)}\",\"{nameof(GoPrtgServer.PassHash)}\"");

            var objs = servers.Select(o =>
            {
                var s = new GoPrtgServer(o);

                if (client != null && s.Server == client.Server && s.UserName == client.UserName)
                    s.IsActive = true;

                return s;
            }).ToList();

            return objs;
        }

        internal void WriteColorOutput(string message, ConsoleColor color)
        {
            ConsoleColor fg = Host.UI.RawUI.ForegroundColor;

            Host.UI.RawUI.ForegroundColor = color;

            try
            {
                WriteObject(message);
            }
            finally
            {
                Host.UI.RawUI.ForegroundColor = fg;
            }
        }

        internal void ValidateGoPrtgBlock()
        {
            var str = "has been removed from PowerShell profile. Please reinstate line or remove all lines pertaining to GoPrtg from your profile.";

            if (GoPrtgProfile.HeaderMissing && GoPrtgProfile.FooterMissing)
            {
                //If our function isn't installed, we're installing a brand new GoPrtg server
                if (!GoPrtgFunctionInstalled)
                    return;

                //Otherwise, it looks like our header and footer have been removed!
                var builder = new StringBuilder($"GoPrtg Servers start line '{GoPrtgProfile.GoPrtgHeader}' ")
                    .Append($"and end line '{GoPrtgProfile.GoPrtgFooter}' have been removed from your ")
                    .Append($" PowerShell profile, however the '{goPrtgFunction}' function is still present. ")
                    .Append("Please reinstate the GoPrtg start and end lines or remove all lines pertaining to GoPrtg including the GoPrtg function from your profile.");

                throw new InvalidOperationException(builder.ToString());
            }

            if (GoPrtgProfile.HeaderMissing && !GoPrtgProfile.FooterMissing)
                throw new InvalidOperationException($"GoPrtg Servers start line '{GoPrtgProfile.GoPrtgHeader}' {str}");

            if (GoPrtgProfile.FooterMissing && !GoPrtgProfile.HeaderMissing)
                throw new InvalidOperationException($"GoPrtg Servers end line '{GoPrtgProfile.GoPrtgFooter}' {str}");

            if (!GoPrtgFunctionInstalled)
            {
                throw new InvalidOperationException($"GoPrtg header and footer are present in PowerShell profile, however {goPrtgFunction} function was not loaded into the current session. Please verify the function has not been corrupted or remove the GoPrtg header and footer and re-run Install-GoPrtgServer.");
            }
        }

        internal void UpdateServerRunner(Func<List<GoPrtgServer>, GoPrtgServer, Action<GoPrtgServer>> validator)
        {
            ValidateGoPrtgBlock();

            if (GoPrtgFunctionInstalled)
            {
                var servers = GetServers();

                var activeServer = servers.Where(s => s.Server == client.Server && s.UserName == client.UserName).ToList();

                if (activeServer.Count == 0)
                {
                    var serversIgnoringUserName = servers.Where(s => s.Server == client.Server).ToList();

                    if (serversIgnoringUserName.Count == 0)
                        throw new InvalidOperationException($"Server '{client.Server}' is not a valid GoPrtg server. To install this server, run Install-GoPrtgServer [<alias>].");

                    throw new InvalidOperationException($"Server '{client.Server}' is a valid GoPrtg server, however you are not authenticated as a valid user for this server. To modify this server, first run GoPrtg [<alias>], then re-run the original command.");
                }
                else if (activeServer.Count == 1)
                {
                    Action<GoPrtgServer> rowUpdater = validator(servers, activeServer.First());

                    UpdateServerRow(activeServer.First(), rowUpdater);
                }
                else
                {
                    throw new InvalidOperationException($"A critical error has occurred: more than one server exists with the same server and username. Please remove the duplicate from your {profile} manually.");
                }
            }
            else
                throw new InvalidOperationException("GoPrtg is not installed. Run Install-GoPrtgServer <alias> to install a server with the specified alias.");
        }

        private void UpdateServerRow(GoPrtgServer activeServer, Action<GoPrtgServer> updater)
        {
            var str = activeServer.ToString();

            var newLines = ForeachServer(line =>
            {
                var compare = FormatWildcardTarget(str);

                var wildcard = new WildcardPattern(compare, WildcardOptions.IgnoreCase);

                if (wildcard.IsMatch(line))
                {
                    updater(activeServer);

                    var newLine = "    " + activeServer;

                    if (line.EndsWith(","))
                        newLine += ",";

                    return newLine;
                }

                return line;
            });

            var final = AddGoPrtgFunctionHeaderAndFooter(newLines);

            UpdateGoPrtgFunctionBody(final);

            LoadFunction(string.Join(Environment.NewLine, final));
        }

        internal List<PSObject> FormatOutput(List<GoPrtgServer> servers)
        {
            var response = servers.Select(o =>
            {
                var ps = new PSObject();

                var activeStr = o.IsActive ? "*" : " ";

                ps.Properties.Add(new PSNoteProperty("[!]", $"[{activeStr}]"));
                ps.Properties.Add(new PSNoteProperty(nameof(GoPrtgServer.Server), o.Server));
                ps.Properties.Add(new PSNoteProperty(nameof(GoPrtgServer.Alias), o.Alias));
                ps.Properties.Add(new PSNoteProperty(nameof(GoPrtgServer.UserName), o.UserName));

                return ps;
            }).ToList();

            return response;
        }

        internal List<string> AddGoPrtgFunctionHeaderAndFooter(List<string> funcBody)
        {
            var formatted = new List<string>();
            formatted.Add(GoPrtgProfile.Contents.Value[GoPrtgProfile.HeaderLine.Value + 2]);
            formatted.AddRange(funcBody);
            formatted.Add(GoPrtgProfile.Contents.Value[GoPrtgProfile.FooterLine.Value - 2]);

            return formatted;
        }

        internal void UpdateGoPrtgFunctionBody(List<string> funcBody)
        {
            var newContent = new List<string>();
            newContent.AddRange(GoPrtgProfile.AboveHeader.Value);

            if (funcBody != null && funcBody.Count > 0)
            {
                funcBody = AddGoPrtgHeaderAndFooter(funcBody);
                newContent.AddRange(funcBody);
            }

            newContent.AddRange(GoPrtgProfile.BelowFooter.Value);

            var str = string.Join(Environment.NewLine, newContent);

            if (str == string.Empty)
            {
                File.WriteAllText(profile, str);
            }
            else
                File.WriteAllText(profile, str + Environment.NewLine);
        }

        private List<string> AddGoPrtgHeaderAndFooter(List<string> funcBody)
        {
            var formatted = new List<string>();

            formatted.Add(GoPrtgProfile.Contents.Value[GoPrtgProfile.HeaderLine.Value]);
            formatted.Add(GoPrtgProfile.Contents.Value[GoPrtgProfile.HeaderLine.Value + 1]);
            formatted.AddRange(funcBody);
            formatted.Add(GoPrtgProfile.Contents.Value[GoPrtgProfile.FooterLine.Value - 1]);
            formatted.Add(GoPrtgProfile.Contents.Value[GoPrtgProfile.FooterLine.Value]);

            return formatted;
        }

        internal List<string> ForeachServer(Func<string, string> action)
        {
            var newContent = new List<string>();

            for (var i = GoPrtgProfile.HeaderLine.Value + 3; i <= GoPrtgProfile.FooterLine.Value - 3; i++)
            {
                var line = GoPrtgProfile.Contents.Value[i];

                var result = action(line);

                if (result != null)
                    newContent.Add(result);
            }

            return newContent;
        }

        internal string FormatWildcardTarget(GoPrtgServer server) => FormatWildcardTarget(server.ToString());

        private string FormatWildcardTarget(string str)
        {
            //There might be a comma. WildcardPattern requires escaped backticks to perform -like successfully
            return $"    {str}*".Replace("`\"", "```\"");
        }

        internal void Connect(string server, PSCredential credential)
        {
            var connect = new ConnectPrtgServer
            {
                Server = server,
                Credential = credential,
                PassHash = true,
                Force = true
            };

            connect.Connect(this);
        }

        internal string EncryptString(string str)
        {
            return InvokeCommand.InvokeScript($"ConvertTo-SecureString {str} -AsPlainText -Force | ConvertFrom-SecureString").First().ToString();
        }
    }
}
