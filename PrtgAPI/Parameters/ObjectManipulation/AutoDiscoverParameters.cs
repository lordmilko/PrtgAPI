using System.Linq;
using PrtgAPI.Linq;

namespace PrtgAPI.Parameters
{
    class AutoDiscoverParameters : BaseActionParameters, ICommandParameters
    {
        CommandFunction ICommandParameters.Function => CommandFunction.DiscoverNow;

        public AutoDiscoverParameters(int objectId, DeviceTemplate[] templates) : base(objectId)
        {
            if (templates != null && templates.Length > 0)
                DeviceTemplates = templates.WithoutNull();
        }

        public DeviceTemplate[] DeviceTemplates
        {
            get { return (DeviceTemplate[]) this[Parameter.Template]; }
            set
            {
                this[Parameter.Template] = value?.Select(
                    s => new DeviceTemplate(s.raw, t => $"\"{t.Value}\"")
                ).ToArray();
            }
        }
    }
}
