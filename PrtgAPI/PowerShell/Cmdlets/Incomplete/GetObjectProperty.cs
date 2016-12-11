using System;
using System.Management.Automation;
using PrtgAPI.Attributes;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Helpers;
using System.Linq;
using System.Text;

namespace PrtgAPI.PowerShell.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "ObjectProperty")]
    public class GetObjectProperty : PrtgCmdlet
    {
        public int? ObjectId { get; set; }

        [Parameter(ValueFromPipeline = true)]
        public ObjectTable Object { get; set; }

        [Parameter(Mandatory = true, Position = 0)]
        public PropertyType Setting { get; set; } //maybe we could have a "type" parameter that asks for one of the many enum types (and we'll have an enum of those enums) and then we validate the setting in the processrecord

        [Parameter(Mandatory = true, Position = 1)]
        public object Value { get; set; }

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            if (ObjectId == null && Object == null)
                throw new ArgumentException("Please specify either an Object or an ObjectId");

            var enumType = Setting.GetEnumAttribute<PropertyTypeAttribute>(); //todo: need to make the attribute mandatory

            var enumValues = Enum.GetNames(enumType.Type);

            if (enumValues.All(v => v.ToLower() != Value.ToString().ToLower()))
            {
                var builder = new StringBuilder();

                builder.Append($"'{Value}' is not a valid value for property type '{Setting}'. Please enter one of the following values: ");

                for (int i = 0; i < enumValues.Length; i++)
                {
                    builder.Append(enumValues[i]);

                    if (i < enumValues.Length - 1)
                    {
                        builder.Append(", ");
                    }
                }

                throw new ArgumentException(builder.ToString());
            }
            else
            {
                Value = Enum.Parse(enumType.Type, Value.ToString(), true);
            }

            var result = client.GetObjectProperty(Object?.Id ?? ObjectId.Value, (dynamic)Value);

            var obj = new PSObject();
            obj.Members.Add(new PSNoteProperty(Setting.ToString(), result));

            WriteObject(obj);
        }
    }
}
