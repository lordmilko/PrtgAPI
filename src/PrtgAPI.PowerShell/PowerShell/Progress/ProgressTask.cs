using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Progress
{
    /// <summary>
    /// Represents a sequence of tasks in a process containing two or more operations that yields objects of a specified type.
    /// </summary>
    /// <typeparam name="TResult">The type of objects returned from this task's operation.</typeparam>
    public class ProgressTask<TResult>
    {
        private readonly Lazy<List<TResult>> task;
        private readonly PrtgProgressCmdlet cmdlet;

        private ProgressTask(Func<List<TResult>> function, PrtgProgressCmdlet cmdlet)
        {
            task = new Lazy<List<TResult>>(function);
            this.cmdlet = cmdlet;
        }

        private ProgressTask(Lazy<List<TResult>> task, PrtgProgressCmdlet cmdlet)
        {
            this.task = task;
            this.cmdlet = cmdlet;
        }

        /// <summary>
        /// Create a sequence of progress tasks for processing a process containing two or more operations.
        /// </summary>
        /// <typeparam name="TNewResult">The type of object returned by the first operation.</typeparam>
        /// <param name="func">The first operation to execute.</param>
        /// <param name="cmdlet">The cmdlet that should display progress.</param>
        /// <param name="typeDescription">The type description use for the progress.</param>
        /// <param name="operationDescription">The progress description to use for the first operation.</param>
        /// <returns>A new <see cref="ProgressTask{TResult}"/> that encapsulates the specified parameters.</returns>
        public static ProgressTask<TNewResult> Create<TNewResult>(Func<List<TNewResult>> func, PrtgProgressCmdlet cmdlet, string typeDescription, string operationDescription)
        {
            return new ProgressTask<TNewResult>(() =>
            {
                //The actions to actually perform at the time the lazy task is actually executed.
                cmdlet.TypeDescription = typeDescription;
                cmdlet.OperationTypeDescription = operationDescription;
                cmdlet.DisplayProgress(false);
                return func();
            }, cmdlet);
        }

        /// <summary>
        /// Execute an intermediate progress task for a process containing three or more operations.
        /// </summary>
        /// <typeparam name="TNewResult">The type of object returned by this operation.</typeparam>
        /// <param name="func">The operation to execute.</param>
        /// <param name="operationDescription">The progress description to use for this operation.</param>
        /// <returns>A new <see cref="ProgressTask{TResult}"/> specifying the next task to perform in the progress sequence.</returns>
        [ExcludeFromCodeCoverage]
        public ProgressTask<TNewResult> Then<TNewResult>(Func<List<TResult>, List<TNewResult>> func, string operationDescription)
        {
            if (ValueEmpty())
                return Empty<TNewResult>();

            return new ProgressTask<TNewResult>(() =>
            {
                /* The actions to actually perform at the time the lazy task is actually executed.
                 * This method functions much the same as ProgressTask.Create, except it operates on an existing ProgressTask object,
                 * doesn't set the initial cmdlet.TypeDescription and the func that is supplied takes the output of the previous task as input. */
                cmdlet.OperationTypeDescription = operationDescription;
                cmdlet.DisplayProgress(false);
                return func(task.Value);
            }, cmdlet);
        }

        /// <summary>
        /// Execute the last progress task of a process containing two or more operations.
        /// </summary>
        /// <typeparam name="TNewResult">The type of object returned by this operation.</typeparam>
        /// <param name="func">The operation to execute.</param>
        /// <param name="operationDescription">The progress description to use for this operation.</param>
        /// <returns>A new <see cref="ProgressTask{TResult}"/> specifying the last task to perform in the progress sequence.</returns>
        public ProgressTask<TNewResult> Finally<TNewResult>(Func<List<TResult>, List<TNewResult>> func, string operationDescription)
        {
            if (ValueEmpty())
                return Empty<TNewResult>();

            var newTask = new Lazy<List<TNewResult>>(() =>
            {
                /* The actions to actually perform at the time the lazy task is actually executed
                 * This method functions much the same as ProgressTask.Then, except it _does_ permit writing the resulting object to the pipeline */
                cmdlet.OperationTypeDescription = operationDescription;
                cmdlet.DisplayProgress();

                return func(task.Value);
            });

            return new ProgressTask<TNewResult>(newTask, cmdlet);
        }

        /// <summary>
        /// Write the result of the task sequence to the pipeline.
        /// </summary>
        public void Write()
        {
            if (ValueEmpty())
                return;

            cmdlet.UpdatePreviousProgressAndSetCount(task.Value.Count);

            cmdlet.WriteList(task.Value);
        }

        private bool ValueEmpty()
        {
            if (task.Value == null || task.Value.Count == 0)
                return true;

            return false;
        }

        private ProgressTask<TNewResult> Empty<TNewResult>()
        {
            return new ProgressTask<TNewResult>(() => new List<TNewResult>(), cmdlet);
        }
    }
}
