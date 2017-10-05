using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Reflection;
using Microsoft.PowerShell.Commands;
using PrtgAPI.PowerShell;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.Helpers
{
    static class PipelineHelpers
    {
        public static CommandInfo GetDownstreamCmdlet(this ICommandRuntime runtime)
        {
            var pipe = runtime.GetInternalProperty("OutputPipe");
            var downstreamCmdlet = pipe.GetInternalProperty("DownstreamCmdlet");

            CommandInfo commandInfo = (CommandInfo)downstreamCmdlet?.GetInternalProperty("CommandInfo");                

            return commandInfo;
        }

        public static object GetUpstreamCmdlet(this PSCmdlet cmdlet)
        {
            var commands = GetPipelineCommands(cmdlet);

            var myIndex = commands.IndexOf(cmdlet);

            if (myIndex <= 0)
                return null;

            var previousIndex = myIndex - 1;

            var previousCmdlet = commands[previousIndex];

            return previousCmdlet;
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
        /// Returns the previous PrtgCmdlet before this one. If no previous cmdlet was a PrtgCmdlet, this method returns null.
        /// </summary>
        /// <param name="cmdlet">The cmdlet to retrieve the previous cmdlet of.</param>
        /// <returns>If a previous cmdlet is a PrtgCmdlet, that cmdlet. Otherwise, null.</returns>
        public static PrtgCmdlet GetPreviousPrtgCmdlet(this PSCmdlet cmdlet)
        {
            var commands = GetPipelineCommands(cmdlet);

            var myIndex = commands.IndexOf(cmdlet);

            for (int i = myIndex - 1; i >= 0; i--)
            {
                if (commands[i] is PrtgCmdlet)
                    return (PrtgCmdlet) commands[i];
            }

            return null;
        }

        public static bool PipelineIsProgressPure(this PSCmdlet cmdlet)
        {
            var commands = GetPipelineCommands(cmdlet);

            var myIndex = commands.IndexOf(cmdlet);

            if (myIndex <= 0)
                return true;

            for (int i = 0; i < myIndex; i++)
            {
                var command = commands[i];

                if (!(command is PrtgCmdlet || command is WhereObjectCommand))
                    return false;
            }

            return true;
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

        public static bool PipelineRemainingHasCmdlet<T>(this PSCmdlet cmdlet) where T : Cmdlet
        {
            var commands = GetPipelineCommands(cmdlet);

            var myIndex = commands.IndexOf(cmdlet);

            return commands.Skip(myIndex + 1).Any(c => c is T);
        }

        /// <summary>
        /// Indicates whether the current pipeline contains progress compatible cmdlets all the way to the next <see cref="PrtgCmdlet"/>. Returns false if there are no more <see cref="PrtgCmdlet"/> objects in the pipeline.
        /// </summary>
        /// <param name="cmdlet">The currently executing cmdlet.</param>
        /// <returns></returns>
        public static bool PipelineIsProgressPureToPrtgCmdlet(this PSCmdlet cmdlet)
        {
            var commands = GetPipelineCommands(cmdlet);

            var myIndex = commands.IndexOf(cmdlet);

            for (int i = myIndex + 1; i < commands.Count; i++)
            {
                if (commands[i] is PrtgCmdlet)
                    return true;

                if (!(commands[i] is WhereObjectCommand))
                    return false;
            }

            return false;
        }

        public static bool PipelineIsProgressPureFromPrtgCmdlet(this PSCmdlet cmdlet)
        {
            var commands = GetPipelineCommands(cmdlet);

            var myIndex = commands.IndexOf(cmdlet);

            for (int i = myIndex - 1; i >= 0; i--)
            {
                if (commands[i] is PrtgCmdlet)
                    return true;

                if (!(commands[i] is WhereObjectCommand))
                    return false;
            }

            return false;
        }

        public static object GetFirstObjectInPipeline(this PSCmdlet cmdlet)
        {
            return GetPipelineCommands(cmdlet).First();
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
