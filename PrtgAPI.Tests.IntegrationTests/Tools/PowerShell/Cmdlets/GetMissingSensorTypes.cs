using System;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.PowerShell.Base;
using System.Xml.Serialization;
using PrtgAPI.Exceptions.Internal;

namespace PrtgAPI.Tests.IntegrationTests
{
    public class MissingSensorType
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool Missing { get; set; }

        public MissingSensorType(string id, string name, string description, bool missing)
        {
            Id = id;
            Name = name;
            Description = description;
            Missing = missing;
        }
    }

    [Cmdlet(VerbsCommon.Get, "MissingSensorTypes")]
    public class GetMissingSensorTypes : PrtgCmdlet
    {
        protected override void ProcessRecordEx()
        {
            var prtgTypes = client.GetSensorTypes(1);
            var uniquePrtgTypes = prtgTypes.GroupBy(t => t.Id).Select(g => g.First()).ToList();
            var ourTypes = Enum.GetValues(typeof(SensorTypeInternal)).Cast<Enum>().Select(e => GetEnumAttribute<XmlEnumAttribute>(e).Name).ToList();

            var missingObjs = uniquePrtgTypes.Select(t =>
            {
                var missing = !ourTypes.Contains(t.Id);

                return new MissingSensorType(t.Id, t.Name, t.Description, missing);
            }).OrderByDescending(o => o.Missing);

            foreach (var type in missingObjs)
            {
                WriteObject(type);
            }
        }

        public static TAttribute GetEnumAttribute<TAttribute>(Enum element, bool mandatory = false) where TAttribute : Attribute
        {
            var attributes = element.GetType().GetMember(element.ToString()).First().GetCustomAttributes(typeof(TAttribute), false);

            if (attributes.Any())
                return (TAttribute)attributes.First();

            if (!mandatory)
                return null;
            else
                throw new MissingAttributeException(element.GetType(), element.ToString(), typeof(TAttribute));
        }
    }
}
