using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Reflection;
using PrtgAPI.PowerShell;
using PrtgAPI.PowerShell.Base;

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

        public static Pipeline GetCmdletPipelineInput(this ICommandRuntime commandRuntime, InternalCommand cmdlet)
        {
            var inputPipe = commandRuntime.GetInternalProperty("InputPipe");
            var enumerator = inputPipe.GetType().GetField("_enumeratorToProcess", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(inputPipe);

            var currentPS = (PSObject)cmdlet.GetInternalProperty("CurrentPipelineObject");

            var current = currentPS?.ToString() == string.Empty || currentPS == null ? null : currentPS.BaseObject;

            if (enumerator == null) //Piping from a cmdlet
            {
                if (current == null)
                    return null;

                return new Pipeline(current, new List<object> {current});
            }
            else //Piping from a variable
            {
                var array = ((object[]) enumerator.GetType().GetField("_array", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(enumerator)).Select(o =>
                {
                    if (o is PSObject)
                        return o;
                    else
                        return new PSObject(o);
                }).Cast<PSObject>();

                return new Pipeline(current, array.Select(e => e.BaseObject).ToList());
            }
        }

        public static Pipeline GetPipelineInput(this ICommandRuntime commandRuntime)
        {
            var processor = commandRuntime.GetInternalProperty("PipelineProcessor");
            var commands = processor.GetInternalProperty("Commands");

            var list = ((IEnumerable) commands).Cast<object>().ToList();
            var first = list.First();
            var command = (InternalCommand) first.GetInternalProperty("Command");

            var runtime = (ICommandRuntime)first.GetInternalProperty("CommandRuntime");

            return runtime.GetCmdletPipelineInput(command);
        }

        /// <summary>
        /// Returns the previous PrtgCmdlet directly before this one. If the previous cmdlet was not a PrtgCmdlet, this method returns null.
        /// </summary>
        /// <param name="cmdlet">The cmdlet to retrieve the previous cmdlet of.</param>
        /// <returns>If the previous cmdlet if that cmdlet is part of PrtgAPI. Otherwise, null.</returns>
        public static PrtgCmdlet GetPreviousCmdlet(this PSCmdlet cmdlet)
        {
            var commands = GetPipelineCommands(cmdlet);

            var myIndex = commands.IndexOf(cmdlet);

            if (myIndex <= 0)
                return null;

            var previousIndex = myIndex - 1;

            var previousCmdlet = commands[previousIndex];

            if (previousCmdlet is PrtgCmdlet)
            {
                return (PrtgCmdlet) previousCmdlet;
            }

            return null;
        }

        /// <summary>
        /// Indicates whether the current pipeline contains a cmdlet of a specified type
        /// </summary>
        /// <typeparam name="T">The type of cmdlet to check for.</typeparam>
        /// <param name="cmdlet">The cmdlet whose pipeline should be inspected.</param>
        /// <returns>True if the pipeline history contains a cmdlet of the specified type. Otherwise, false.</returns>
        public static bool PipelineHasCmdlet<T>(this PSCmdlet cmdlet) where T : Cmdlet
        {
            if (cmdlet is T)
                return true;

            var commands = GetPipelineCommands(cmdlet);

            return commands.Any(c => c is T);
        }

        public static bool PipelineSoFarHasCmdlet<T>(this PSCmdlet cmdlet) where T : Cmdlet
        {
            if (cmdlet is T)
                return true;

            var commands = GetPipelineCommands(cmdlet);

            var myIndex = commands.IndexOf(cmdlet);

            return commands.Take(myIndex + 1).Any(c => c is T);
        }

        private static List<object> GetPipelineCommands(PSCmdlet cmdlet)
        {
            var processor = cmdlet.CommandRuntime.GetInternalProperty("PipelineProcessor");
            var commandProcessors = processor.GetInternalProperty("Commands");

            var commandProcessorsList = ((IEnumerable)commandProcessors).Cast<object>().ToList();

            var commands = commandProcessorsList.Select(c => c.GetInternalProperty("Command")).ToList();

            return commands;
        }
    }
}
