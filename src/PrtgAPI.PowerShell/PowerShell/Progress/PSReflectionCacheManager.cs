using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Internal;
using Microsoft.PowerShell.Commands;
using PrtgAPI.Linq;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Reflection;
using PrtgAPI.Utilities;

namespace PrtgAPI.PowerShell.Progress
{
    [ExcludeFromCodeCoverage]
    class PSReflectionCacheManager
    {
        private PSCmdlet cmdlet;

        #region Reflection Members

        private Lazy<object> runtimeOutputPipe;
        private Lazy<object> runtimeOutputPipeDownstreamCmdletProcessor;
        private Lazy<CommandInfo> runtimeOutputPipeDownstreamCmdletCommandInfo;
        private Lazy<CommandInfo> upstreamCmdletCommandInfo;
        private Lazy<object> runtimeOutputPipeDownstreamCmdletCommand;

        private Lazy<object> runtimePipelineProcessor;
        private Lazy<List<object>> runtimePipelineProcessorCommandProcessors;
        private Lazy<List<object>> runtimePipelineProcessorCommandProcessorCommands;

        #endregion
        #region Method Results

        private Lazy<object> upstreamCmdlet;
        private Lazy<PrtgCmdlet> nextPrtgCmdlet;
        private Lazy<PrtgCmdlet> previousPrtgCmdlet;

        #endregion

        public PSReflectionCacheManager(PSCmdlet cmdlet)
        {
            this.cmdlet = cmdlet;

            runtimeOutputPipe                            = new Lazy<object>      (() => cmdlet.CommandRuntime.GetInternalProperty("OutputPipe"));
            runtimeOutputPipeDownstreamCmdletProcessor   = new Lazy<object>      (() => runtimeOutputPipe.Value.GetInternalProperty("DownstreamCmdlet"));
            runtimeOutputPipeDownstreamCmdletCommandInfo = new Lazy<CommandInfo> (() => (CommandInfo)runtimeOutputPipeDownstreamCmdletProcessor.Value?.GetInternalProperty("CommandInfo"));
            runtimeOutputPipeDownstreamCmdletCommand     = new Lazy<object>      (() => runtimeOutputPipeDownstreamCmdletProcessor.Value?.GetInternalProperty("Command"));

            runtimePipelineProcessor                     = new Lazy<object>      (() => cmdlet.CommandRuntime.GetInternalProperty("PipelineProcessor"));
            runtimePipelineProcessorCommandProcessors    = new Lazy<List<object>>(() =>
            {
                var commands = runtimePipelineProcessor.Value.GetInternalProperty("Commands");

                return commands.ToIEnumerable().ToList();
            });
            runtimePipelineProcessorCommandProcessorCommands = new Lazy<List<object>>(() => runtimePipelineProcessorCommandProcessors.Value.Select(c => c.GetInternalProperty("Command")).ToList());

            upstreamCmdlet = new Lazy<object>(GetUpstreamCmdletInternal);
            nextPrtgCmdlet = new Lazy<PrtgCmdlet>(GetNextPrtgCmdletInternal);
            previousPrtgCmdlet = new Lazy<PrtgCmdlet>(GetPreviousPrtgCmdletInternal);

            upstreamCmdletCommandInfo = new Lazy<CommandInfo>(() => (CommandInfo)GetUpstreamCmdlet()?.GetInternalProperty("CommandInfo"));
        }

        #region Get Cmdlet From Position

        public object GetFirstCmdletInPipeline() => GetPipelineCommands().First();

        public object GetDownstreamCmdlet() => runtimeOutputPipeDownstreamCmdletCommand.Value;

        public object GetDownstreamCmdletNotOfType<T>()
        {
            var commands = GetPipelineCommands();

            var myIndex = commands.IndexOf(cmdlet);

            if (myIndex == commands.Count - 1)
                return null;

            var nextIndex = myIndex + 1;

            for (int i = nextIndex; i < commands.Count; i++)
            {
                if (!(commands[i] is T))
                    return commands[i];
            }

            return null;
        }

        private object GetUpstreamCmdletInternal()
        {
            var commands = GetPipelineCommands();

            var myIndex = commands.IndexOf(cmdlet);

            if (myIndex <= 0)
                return null;

            var previousIndex = myIndex - 1;

            var previousCmdlet = commands[previousIndex];

            return previousCmdlet;
        }

        public object GetUpstreamCmdlet() => upstreamCmdlet.Value;

        public object GetUpstreamCmdletNotOfType<T>()
        {
            var commands = GetPipelineCommands();

            var myIndex = commands.IndexOf(cmdlet);

            if (myIndex <= 0)
                return null;

            var previousIndex = myIndex - 1;

            for (int i = previousIndex; i >= 0; i--)
            {
                if (!(commands[i] is T))
                    return commands[i];
            }

            return null;
        }

        private PrtgCmdlet GetNextPrtgCmdletInternal()
        {
            var commands = GetPipelineCommands();

            var myIndex = commands.IndexOf(cmdlet);

            for (int i = myIndex + 1; i < commands.Count; i++)
            {
                if (commands[i] is PrtgCmdlet)
                    return (PrtgCmdlet) commands[i];
            }

            return null;
        }

        /// <summary>
        /// Retrieves the next <see cref="PrtgCmdlet"/> in the pipeline. If there are no more PrtgAPI cmdlets in the pipeline, this method returns null. 
        /// </summary>
        /// <returns></returns>
        public PrtgCmdlet GetNextPrtgCmdlet() => nextPrtgCmdlet.Value;

        /// <summary>
        /// Returns the previous PrtgCmdlet before this one. If no previous cmdlet was a PrtgCmdlet, this method returns null.
        /// </summary>
        /// <returns>If a previous cmdlet is a PrtgCmdlet, that cmdlet. Otherwise, null.</returns>
        private PrtgCmdlet GetPreviousPrtgCmdletInternal()
        {
            var commands = GetPipelineCommands();

            var myIndex = commands.IndexOf(cmdlet);

            for (int i = myIndex - 1; i >= 0; i--)
            {
                if (commands[i] is PrtgCmdlet)
                    return (PrtgCmdlet) commands[i];
            }

            if (cmdlet.CommandRuntime is DummyRuntime)
                return ((DummyRuntime) cmdlet.CommandRuntime).Owner;

            return null;
        }

        public PrtgCmdlet GetPreviousPrtgCmdlet() => previousPrtgCmdlet.Value;

        /// <summary>
        /// Returns the last <see cref="PrtgCmdlet"/> not of type T. If the first cmdlet in the pipeline is of type T,
        /// that cmdlet will be returned. If there are no previous cmdlets in the pipeline, this method will return null.
        /// </summary>
        /// <typeparam name="T">The type of cmdlet to try and avoid.</typeparam>
        /// <returns>The previous PrtgCmdlet not of type T, unless a cmdlet of type T is the first cmdlet in the pipeline. If there is no previous PrtgCmdlet, this method returns null.</returns>
        public PrtgCmdlet TryGetPreviousPrtgCmdletOfNotType<T>()
        {
            var commands = GetPipelineCommands();

            var myIndex = commands.IndexOf(cmdlet);

            for (int i = myIndex - 1; i >= 0; i--)
            {
                if (commands[i] is PrtgCmdlet)
                {
                    if (!(commands[i] is T) || i == 0)
                    {
                        return (PrtgCmdlet)commands[i];
                    }
                }
            }

            return null;
        }

        public PrtgCmdlet GetPreviousPrtgCmdletOfType<T>()
        {
            var commands = GetPipelineCommands();

            var myIndex = commands.IndexOf(cmdlet);

            for (int i = myIndex - 1; i >= 0; i--)
            {
                if (commands[i] is PrtgCmdlet)
                {
                    if (commands[i] is T)
                    {
                        return (PrtgCmdlet)commands[i];
                    }
                }
            }

            return null;
        }

        public PrtgOperationCmdlet TryGetFirstOperationCmdletAfterSelectObject()
        {
            var commands = GetPipelineCommands();

            var myIndex = commands.IndexOf(cmdlet);

            for (int i = myIndex; i >= 1; i--)
            {
                if (SelectObjectDescriptor.IsSelectObjectCommand(commands[i - 1]))
                {
                    if (commands[i] is PrtgOperationCmdlet)
                        return commands[i] as PrtgOperationCmdlet;

                    return null;
                }
            }

            return null;
        }

        #endregion
        #region Pipeline Has Cmdlet

        /// <summary>
        /// Indicates whether the current pipeline contains a cmdlet of a specified type
        /// </summary>
        /// <typeparam name="T">The type of cmdlet to check for.</typeparam>
        /// <returns>True if the pipeline history contains a cmdlet of the specified type. Otherwise, false.</returns>
        public bool PipelineHasCmdlet<T>() where T : Cmdlet
        {
            if (cmdlet is T)
                return true;

            var commands = GetPipelineCommands();

            return commands.Any(c => c is T);
        }

        public bool PipelineSoFarHasCmdlet<T>() where T : Cmdlet
        {
            if (cmdlet is T)
                return true;

            return PipelineBeforeMeHasCmdlet<T>();
        }

        public bool PipelineBeforeMeHasCmdlet<T>() where T : Cmdlet
        {
            var commands = GetPipelineCommands();

            var myIndex = commands.IndexOf(cmdlet);

            return commands.Take(myIndex).Any(c => c is T);
        }

        public bool PipelineAfterMeIsCmdlet<T>() where T : Cmdlet
        {
            var commands = GetPipelineCommands();

            var myIndex = commands.IndexOf(cmdlet) + 1;

            return commands.Skip(myIndex).FirstOrDefault(c => c is T) != null;
        }

        public bool PipelineRemainingHasCmdlet<T>() where T : Cmdlet
        {
            var commands = GetPipelineCommands();

            var myIndex = commands.IndexOf(cmdlet);

            return commands.Skip(myIndex + 1).Any(c => c is T);
        }

        #endregion
        #region Pipeline Purity

        //whats the difference between this and PipelineIsProgressPureFromPrtgCmdlet

        public bool PipelineIsProgressPure()
        {
            var commands = GetPipelineCommands();

            var myIndex = commands.IndexOf(cmdlet);

            if (myIndex <= 0)
                return true;

            for (int i = 0; i < myIndex; i++)
            {
                var command = commands[i];

                if (!(command is PrtgCmdlet || ProgressManager.IsPureThirdPartyCmdlet(command.GetType())))
                    return false;
            }

            return true;
        }

        public bool PipelineIsProgressPureFromLastPrtgCmdlet()
        {
            var commands = GetPipelineCommands();

            var myIndex = commands.IndexOf(cmdlet);

            for (var i = myIndex - 1; i >= 0; i--)
            {
                if (commands[i] is PrtgCmdlet)
                    return true;

                if (!ProgressManager.IsPureThirdPartyCmdlet(commands[i].GetType()))
                    return false;
            }

            return false;
        }

        /// <summary>
        /// Indicates whether the current pipeline contains progress compatible cmdlets all the way to the next <see cref="PrtgCmdlet"/>. Returns false if there are no more <see cref="PrtgCmdlet"/> objects in the pipeline.
        /// </summary>
        /// <returns></returns>
        public bool PipelineIsProgressPureToNextPrtgCmdlet()
        {
            var commands = GetPipelineCommands();

            var myIndex = commands.IndexOf(cmdlet);

            for (var i = myIndex + 1; i < commands.Count; i++)
            {
                if (commands[i] is PrtgProgressCmdlet || commands[i] is PrtgOperationCmdlet)
                    return true;

                if (!ProgressManager.IsPureThirdPartyCmdlet(commands[i].GetType()))
                    return false;
            }

            return false;
        }

        public bool PipelineContainsBlockingCmdletToNextPrtgCmdletOrEnd()
        {
            var commands = GetPipelineCommands();

            var myIndex = commands.IndexOf(cmdlet);

            for (var i = myIndex + 1; i < commands.Count; i++)
            {
                if (commands[i] is PrtgCmdlet)
                    return false;

                if (SelectObjectDescriptor.IsSelectObjectCommand(commands[i]))
                {
                    var selectObject = (PSCmdlet) commands[i];
                    var boundParameters = selectObject.MyInvocation.BoundParameters;

                    if (boundParameters.ContainsKey("Last"))
                        return true;

                    if (boundParameters.ContainsKey("SkipLast"))
                        return true;
                }
            }

            return false;
        }

        #endregion
        #region Pipeline Input

        /// <summary>
        /// Retrieve the input to the entire pipeline.
        /// </summary>
        /// <returns></returns>
        public Pipeline GetPipelineInput()
        {
            var processor = runtimePipelineProcessorCommandProcessors.Value.First();

            var command = (InternalCommand) runtimePipelineProcessorCommandProcessorCommands.Value.First();

            var runtime = (ICommandRuntime)processor.GetInternalProperty("CommandRuntime");

            return GetCmdletPipelineInput(runtime, command);
        }

        /// <summary>
        /// Retrieve the input to the current cmdlet.
        /// </summary>
        /// <returns></returns>
        public Pipeline GetCmdletPipelineInput()
        {
            return GetCmdletPipelineInput(cmdlet.CommandRuntime, cmdlet);
        }

        public Pipeline GetSelectPipelineOutput()
        {
            var command = (PSCmdlet)GetUpstreamCmdletNotOfType<WhereObjectCommand>();

            var queue = (Queue<PSObject>) command.PSGetInternalField("_selectObjectQueue", "selectObjectQueue");

            var cmdletPipeline = GetCmdletPipelineInput();

            if (cmdletPipeline == null)
                return null;

            cmdletPipeline.List.AddRange(queue.Cast<object>().ToList());

            var list = cmdletPipeline.List;

            if (command.MyInvocation.BoundParameters.ContainsKey("SkipLast"))
            {
                list = cmdletPipeline.List.Take(cmdletPipeline.List.Count - 1).ToList();
            }

            return new Pipeline(cmdletPipeline.List.First(), list);
        }

        /// <summary>
        /// Retrieve the pipeline input of a specified cmdlet.
        /// </summary>
        /// <param name="commandRuntime">The runtime of the cmdlet whose pipeline should be retrieved.</param>
        /// <param name="cmdlet">The cmdlet whose pipeline should be retrieved.</param>
        /// <returns></returns>
        private static Pipeline GetCmdletPipelineInput(ICommandRuntime commandRuntime, InternalCommand cmdlet)
        {
            var inputPipe = commandRuntime.GetInternalProperty("InputPipe");
            var enumerator = inputPipe.GetInternalField("_enumeratorToProcess");

            var currentPS = (PSObject)cmdlet.GetInternalProperty("CurrentPipelineObject");

            var current = PSObjectToString(currentPS) == string.Empty || currentPS == null ? null : currentPS.BaseObject;

            if (enumerator == null) //Piping from a cmdlet
            {
                if (current == null)
                    return null;

                return new Pipeline(current, new List<object> { current });
            }
            else //Piping from a variable
            {
                var declaringType = enumerator.GetType().DeclaringType;
                var enumeratorType = enumerator.GetType();

                IEnumerable<object> list;

                if (declaringType == typeof(Array) || enumeratorType.Name == "SZArrayEnumerator") //It's a SZArrayEnumerator (piping straight from a variable). In .NET Core 3.1 SZArrayEnumerator is no longer nested
                    list = ((object[]) enumerator.GetInternalField("_array"));
                else if (declaringType == typeof(List<>)) //It's a List<T>.Enumerator (piping from $groups[0].Group)
                    list = enumerator.PSGetInternalField("_list", "list").ToIEnumerable();
                else if (enumeratorType.IsGenericType && enumeratorType.GetGenericTypeDefinition() == typeof(ReadOnlyListEnumerator<>))
                    list = enumerator.GetInternalField("list").ToIEnumerable();
                else
                    throw new NotImplementedException($"Don't know how to extract the pipeline input from a '{enumeratorType}' enumerator from type '{declaringType}'.");

                list = list.Select(o =>
                {
                    var pso = o as PSObject;

                    if (pso != null)
                        return pso.BaseObject;

                    return o;
                });

                return new Pipeline(current, list.ToList());
            }
        }

        private static string PSObjectToString(PSObject obj)
        {
            try
            {
                return obj?.ToString();
            }
            catch(ExtendedTypeSystemException)
            {
                //Calling ToString on the upstream object (such as an illegal SearchFilter) may throw
                return string.Empty;
            }
        }

        #endregion

        /// <summary>
        /// Retrieves the CommandProcessor of the next cmdlet.
        /// </summary>
        /// <returns></returns>
        public object GetDownstreamCmdletProcessor() => runtimeOutputPipeDownstreamCmdletProcessor.Value;

        public CommandInfo GetDownstreamCmdletInfo() => runtimeOutputPipeDownstreamCmdletCommandInfo.Value;

        public CommandInfo GetUpstreamCmdletInfo() => upstreamCmdletCommandInfo.Value;

        public List<object> GetPipelineCommands() => runtimePipelineProcessorCommandProcessorCommands.Value;
    }
}
