using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using PrtgAPI.PowerShell;

namespace PrtgAPI.Helpers
{
    static class PipelineHelpers
    {
        public static CommandInfo GetDownstreamCmdlet(this ICommandRuntime runtime)
        {
            CommandInfo commandInfo = null;

            var pipe = runtime.GetInternalProperty("OutputPipe");
            var downstreamCmdlet = pipe.GetInternalProperty("DownstreamCmdlet");

            commandInfo = (CommandInfo)downstreamCmdlet?.GetInternalProperty("CommandInfo");                

            return commandInfo;
        }

        public static object GetInternalProperty(this object obj, string name)
        {
            var internalFlags = BindingFlags.Instance | BindingFlags.NonPublic;

            var prop = obj.GetType().GetProperty(name, internalFlags).GetValue(obj);

            return prop;
        }

        public static Pipeline GetPipelineInput(this ICommandRuntime commandRuntime, Cmdlet cmdlet)
        {
            var inputPipe = commandRuntime.GetInternalProperty("InputPipe");
            var enumerator = inputPipe.GetType().GetField("_enumeratorToProcess", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(inputPipe);

            var currentPS = (PSObject)cmdlet.GetInternalProperty("CurrentPipelineObject");
            var current = currentPS.ToString() == string.Empty ? null : currentPS.BaseObject;

            if (enumerator == null) //Piping from a cmdlet
            {
                if (current == null)
                    return null;

                return new Pipeline(current, new List<object> {current});
            }
            else //Piping from a variable
            {
                var array = ((object[])enumerator.GetType().GetField("_array", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(enumerator)).Cast<PSObject>();

                return new Pipeline(current, array.Select(e => e.BaseObject).ToList());
            }
        }
    }
}
