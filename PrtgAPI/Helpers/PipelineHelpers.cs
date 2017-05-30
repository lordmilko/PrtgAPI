using System.Management.Automation;
using System.Reflection;

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

        public static object[] GetPipelineInput(this ICommandRuntime commandRuntime, Cmdlet cmdlet)
        {
            var inputPipe = commandRuntime.GetInternalProperty("InputPipe");
            var enumerator = inputPipe.GetType().GetField("_enumeratorToProcess", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(inputPipe);

            if (enumerator == null) //Piping from a cmdlet
            {
                var obj = cmdlet.GetInternalProperty("CurrentPipelineObject");

                if (obj.ToString() == string.Empty)
                    return null;

                return new[] {obj};
            }
            else //Piping from a variable
            {
                return (object[])enumerator.GetType().GetField("_array", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(enumerator);
            }
        }
    }
}
