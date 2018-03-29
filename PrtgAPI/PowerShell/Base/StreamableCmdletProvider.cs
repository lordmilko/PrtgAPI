using System;
using System.Collections.Generic;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Progress;

namespace PrtgAPI.PowerShell.Base
{
    class StreamableCmdletProvider<TCmdlet, TObject, TParam>
        where TCmdlet : PrtgProgressCmdlet, IStreamableCmdlet<TCmdlet, TObject, TParam>
        where TParam : PageableParameters
    {
        public bool StreamResults { get; set; }

        public bool StreamSerial { get; set; }

        /// <summary>
        /// Indicates that current cmdlet should stream, regardless of whether it determines this is required.
        /// </summary>
        public bool ForceStream { get; set; }

        public int? StreamCount { get; set; }

        private TCmdlet cmdlet;

        private int? streamThreshold;

        public StreamableCmdletProvider(TCmdlet cmdlet, int? streamThreshold, bool streamSerial)
        {
            this.cmdlet = cmdlet;
            this.streamThreshold = streamThreshold;

            if (streamThreshold != null)
            {
                StreamResults = true;
                StreamSerial = streamSerial;
            }
        }

        public IEnumerable<TRet> StreamResultsWithProgress<TRet>(TParam parameters, int? count, Func<IEnumerable<TRet>> getFiltered = null)
        {
            cmdlet.ProgressManager.Scenario = ProgressScenario.StreamProgress;

            cmdlet.ProgressManager.WriteProgress($"PRTG {PrtgProgressCmdlet.GetTypeDescription(typeof(TObject))} Search", "Detecting total number of items");

            StreamCount = cmdlet.GetStreamTotalObjects(parameters);

            IEnumerable<TRet> records;

            //Normally if a filter has been specified PrtgAPI won't stream, and so the custom parameters are not necessary.
            //However, if a cmdlet has specified it wants to force a stream, we apply our filters and use our custom parameters.
            if (ForceStream)
            {
                records = getFiltered == null ? StreamRecords<TRet>(parameters, count) : getFiltered();
            }
            else
                records = StreamRecords<TRet>(parameters, count);

            if (StreamCount > streamThreshold)
            {
                //We'll be replacing this progress record, so just null it out via a call to CompleteProgress()
                //We strategically set the TotalRecords AFTER calling this method, to avoid CompleteProgress truly completing the record
                cmdlet.ProgressManager.CompleteProgress();
                cmdlet.SetObjectSearchProgress(ProcessingOperation.Retrieving);
                cmdlet.ProgressManager.TotalRecords = StreamCount;
            }
            else //We won't be showing progress, so complete this record
            {
                cmdlet.ProgressManager.TotalRecords = StreamCount;
                cmdlet.ProgressManager.CompleteProgress();
            }

            return records;
        }

        public IEnumerable<TRet> StreamRecords<TRet>(TParam parameters, int? count)
        {
            //Depending on the number of items we're streaming, we may have made so many requests to PRTG that it can't possibly
            //respond to any cmdlets further downstream until all of the streaming requests have been completed

            //If we're forcing a stream, we may have actually specified the Count parameter. As such, override the
            //number of results to retrieve
            if (ForceStream && count != null)
                StreamCount = count.Value;

            //We went down an alternate code path that doesn't retrieve the total number of needed objects
            if (StreamCount == null)
            {
                if (ForceStream)
                    StreamCount = cmdlet.GetStreamTotalObjects(parameters);
                else
                    throw new NotImplementedException("Attempted to stream without specifying a stream count. This indicates a bug");
            }

            //As such, if there are no other PRTG cmdlets after us, stream as normal. Otherwise, only request a couple at a time
            //so the PRTG will be able to handle the next cmdlet's request
            if (!StreamSerial && cmdlet.ProgressManager.Scenario == ProgressScenario.StreamProgress && !cmdlet.ProgressManager.CacheManager.PipelineRemainingHasCmdlet<PrtgCmdlet>() && cmdlet.ProgressManager.ProgressPipelinesCount == 1) //There are no other cmdlets after us
                return (IEnumerable<TRet>)PrtgSessionState.Client.StreamObjectsInternal(parameters, StreamCount.Value, true, cmdlet.GetStreamObjectsAsync);

            return (IEnumerable<TRet>)PrtgSessionState.Client.SerialStreamObjectsInternal(parameters, StreamCount.Value, true, cmdlet.GetStreamObjects); //There are other cmdlets after us; do one request at a time
        }
    }
}