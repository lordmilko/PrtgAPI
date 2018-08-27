using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using PrtgAPI.Helpers;

namespace PrtgAPI.PowerShell
{
    class FormatGenerator
    {
        internal static string PSObjectTypeName = "PrtgAPI.DynamicFormatPSObject";

        private static List<string> Formats = new List<string>();

        static string folder;

        internal static string Folder
        {
            get
            {
                if (folder == null)
                {
                    var temp = Environment.GetEnvironmentVariable("temp");

                    var prtgTemp = temp + "\\PrtgAPIFormats";

                    if (!Directory.Exists(prtgTemp))
                    {
                        Directory.CreateDirectory(prtgTemp);
                    }
                    else
                    {
                        var files = Directory.GetFiles(prtgTemp);
                        var processes = Process.GetProcesses().Select(p => p.Id).ToList();

                        var regex = new Regex("(.+DynamicFormat.+_)(.+?)(\\..+)");

                        foreach (var file in files)
                        {
                            var delete = true;

                            if (regex.IsMatch(file))
                            {
                                var str = regex.Replace(file, "$2");
                                var pid = Convert.ToInt32(str);

                                if (processes.Contains(pid))
                                    delete = false;
                            }

                            if (delete)
                                new FileInfo(file).Delete();
                        }
                    }

                    folder = prtgTemp + $"\\PrtgAPI.DynamicFormat{{0}}_{Process.GetCurrentProcess().Id}.Format.ps1xml";
                }

                return folder;
            }
        }

        public static void Generate(string typeName, List<Tuple<string, string>> columns, int index)
        {
            var xml = new XElement("Configuration",
                new XElement("ViewDefinitions",
                    new XElement("View",
                        new XElement("Name", "Default"),
                        new XElement("ViewSelectedBy",
                            new XElement("TypeName", typeName)
                        ),
                        new XElement("TableControl",
                            new XElement("TableHeaders",
                                columns.Select(c =>
                                    new XElement("TableColumnHeader",
                                        new XElement("Label", c.Item2)
                                    )
                                )
                            ),
                            new XElement("TableRowEntries",
                                new XElement("TableRowEntry",
                                    new XElement("TableColumnItems",
                                        columns.Select(c =>
                                            new XElement("TableColumnItem",
                                                new XElement("PropertyName", c.Item1)
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    ),
                    new XElement("View",
                        new XElement("Name", "ListView"),
                        new XElement("ViewSelectedBy",
                            new XElement("TypeName", typeName)
                        ),
                        new XElement("ListControl",
                            new XElement("ListEntries",
                                new XElement("ListEntry",
                                    new XElement("ListItems",
                                        columns.Select(c => 
                                            new XElement("ListItem",
                                                new XElement("PropertyName", c.Item1),
                                                new XElement("Label", c.Item2)
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
            );

            var newPath = string.Format(Folder, index);

            xml.Save(newPath);

            Formats.Add(newPath);
        }

        public static void LoadXml(Cmdlet cmdlet)
        {
            var updateFormatData = GetUpdateFormatDataCommand();

            var destinationContextInfo = updateFormatData.GetInternalPropertyInfo("Context");
            var sourceContext = cmdlet.GetInternalProperty("Context");
            var appendPath = updateFormatData.GetPublicPropertyInfo("AppendPath");

            destinationContextInfo.SetValue(updateFormatData, sourceContext);

            appendPath.SetValue(updateFormatData, Formats.ToArray());

            var processRecord = updateFormatData.GetInternalMethod("ProcessRecord");

            processRecord.Invoke(updateFormatData, null);
        }

        private static PSCmdlet GetUpdateFormatDataCommand()
        {
            var assemblyName = "Microsoft.PowerShell.Commands.Utility.dll";
            var typeName = "Microsoft.PowerShell.Commands.UpdateFormatDataCommand";

            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.ManifestModule.Name == assemblyName);

            if (assembly == null)
                throw new InvalidOperationException($"Cannot load type '{typeName}': cannot find assembly '{assemblyName}'.");

            var type = assembly.GetType(typeName);

            if (type == null)
                throw new InvalidOperationException($"Cannot find type '{typeName}' in assembly '{assemblyName}'.");

            var obj = Activator.CreateInstance(type);

            return (PSCmdlet) obj;
        }
    }

    class TypeNameRecord
    {
        internal int Index { get; set; }

        internal string TypeName { get; set; }

        internal int Impurity { get; set; }

        public TypeNameRecord(int index, int impurity)
        {
            Index = index;
            TypeName = FormatGenerator.PSObjectTypeName + index;
            Impurity = impurity;
        }
    }
}