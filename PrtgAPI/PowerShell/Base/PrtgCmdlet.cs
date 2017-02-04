using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using PrtgAPI.Helpers;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for all cmdlets requiring authenticated access to a PRTG Server.
    /// </summary>
    public abstract class PrtgCmdlet : PSCmdlet
    {
        /// <summary>
        /// Provides access to the <see cref="PrtgClient"/> stored in the current PowerShell Session State.
        /// </summary>
        protected PrtgClient client => PrtgSessionState.Client;

        /// <summary>
        /// Provides a one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessing()
        {
            if (PrtgSessionState.Client == null)
                throw new Exception("You are not connected to a PRTG Server. Please connect first using Connect-PrtgServer.");

            client.RetryRequest += OnRetryRequest;
        }

        /// <summary>
        /// Provides a one-time, post-processing functionality for the cmdlet.
        /// </summary>
        protected override void EndProcessing()
        {
            client.RetryRequest -= OnRetryRequest;
        }

        /// <summary>
        /// Writes a list to the output pipeline.
        /// </summary>
        /// <typeparam name="T">The type of the list that will be output.</typeparam>
        /// <param name="sendToPipeline">The list that will be output to the pipeline.</param>
        /// <param name="settings">How progress should be displayed while the list is being enumerated.</param>
        internal void WriteList<T>(IEnumerable<T> sendToPipeline, ProgressSettings settings)
        {
            //var visibleMembers = typeof(T).GetPSVisibleMembers().ToList();

            //visibleMembers.Sort();

            ProgressRecord progress = null;

            var recordsProcessed = -1;

            if (settings != null)
            {
                recordsProcessed = 0;

                progress = new ProgressRecord(1, settings.ActivityName, settings.InitialDescription)
                {
                    PercentComplete = 0,
                    StatusDescription = $"{settings.InitialDescription} {recordsProcessed}/{settings.TotalRecords}"
                };

                WriteProgress(progress);
            }

            foreach (var item in sendToPipeline)
            {
                WriteObject(item);

                if (settings != null)
                {
                    recordsProcessed++;

                    progress.PercentComplete = (int)(recordsProcessed / Convert.ToDouble(settings.TotalRecords) * 100);
                    progress.StatusDescription = $"{settings.InitialDescription} ({recordsProcessed}/{settings.TotalRecords})";
                    WriteProgress(progress);
                }
            }
        }

        private void OnRetryRequest(object sender, RetryRequestEventArgs args)
        {
            var msg = args.Exception.Message.TrimEnd('.');

            WriteWarning($"'{MyInvocation.MyCommand}' timed out: {args.Exception.Message}. Retries remaining: {args.RetriesRemaining}");
        }
    }
}
