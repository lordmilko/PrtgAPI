using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace PrtgAPI.PowerShell
{
    class FormatGenerator
    {
        internal static string PSObjectTypeName = "PrtgAPI.DynamicFormatPSObject";

        private static List<Tuple<string, XElement>> Formats = new List<Tuple<string, XElement>>();

        static string folder;

        internal static string Folder
        {
            get
            {
                if (folder == null)
                {
                    var prtgTemp = Path.Combine(Path.GetTempPath(), "PrtgAPIFormats");

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

                    folder = Path.Combine(prtgTemp, $"PrtgAPI.DynamicFormat{{0}}_{Process.GetCurrentProcess().Id}.Format.ps1xml");
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

            Formats.Add(Tuple.Create(newPath, xml));
        }

        public static void RepairMissing()
        {
            var missingFormats = Formats.Where(f => !File.Exists(f.Item1)).ToList();

            foreach (var f in missingFormats)
                f.Item2.Save(f.Item1);
        }

        public static void LoadXml(PSCmdlet cmdlet)
        {
            RepairMissing();

            var command = $"Update-FormatData -AppendPath {string.Join(",", Formats.Select(f => $"'{f.Item1}'"))}";

            cmdlet.InvokeCommand.InvokeScript(command);
        }
    }

    class TypeNameRecord
    {
        internal int Index { get; set; }

        internal string TypeName { get; set; }

        internal int Impurity { get; set; }

        internal bool XmlGenerated { get; set; }

        public TypeNameRecord(int index, int impurity)
        {
            Index = index;
            TypeName = FormatGenerator.PSObjectTypeName + index;
            Impurity = impurity;
        }
    }
}
