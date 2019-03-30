using System;
using System.Collections.Generic;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Progress;
using PrtgAPI.Request;

namespace PrtgAPI.PowerShell.Base
{
    class StreamableCmdletProvider<TCmdlet, TObject, TParam>
        where TCmdlet : PrtgProgressCmdlet, IStreamableCmdlet<TCmdlet, TObject, TParam>
        where TParam : PageableParameters, IXmlParameters
    {
        public bool StreamResults { get; set; }

        /// <summary>
        /// Indicates that current cmdlet should stream, regardless of whether it determines this is required.
        /// </summary>
        public bool ForceStream { get; set; }

        public int? StreamCount { get; set; }

        private int? totalExist;

        public int GetTotalExist(TParam parameters)
        {
            if (totalExist == null)
                totalExist = cmdlet.GetStreamTotalObjects(parameters);

            return totalExist.Value;
        }

        public void SetTotalExist(int value)
        {
            totalExist = value;
        }

        private TCmdlet cmdlet;

        private int? streamThreshold;

        public StreamableCmdletProvider(TCmdlet cmdlet, bool? shouldStream)
        {
            this.cmdlet = cmdlet;
            streamThreshold = PageableParameters.DefaultPageSize;

            if (shouldStream == true)
                StreamResults = true;
        }

        public IEnumerable<TRet> StreamResultsWithProgress<TRet>(TParam parameters, int? count, Func<IEnumerable<TRet>> getFiltered = null)
        {
            cmdlet.ProgressManager.Scenario = ProgressScenario.StreamProgress;

            if (!cmdlet.ProgressManager.WatchStream)
            {
                cmdlet.ProgressManager.WriteProgress($"PRTG {IObjectExtensions.GetTypeDescription(typeof(TObject))} Search", "Detecting total number of items");

                StreamCount = cmdlet.GetStreamTotalObjects(parameters);
                SetTotalExist(StreamCount.Value);
            }
            else
            {
                cmdlet.ProgressManager.WriteProgress($"PRTG {IObjectExtensions.GetTypeDescription(typeof(TObject))} Watcher", "Waiting for first event");
                StreamCount = 1000;
            }

            IEnumerable<TRet> records;

            //Normally if a filter has been specified PrtgAPI won't stream, and so the custom parameters are not necessary.
            //However, if a cmdlet has specified it wants to force a stream, we apply our filters and use our custom parameters.
            if (ForceStream)
            {
                records = getFiltered == null ? StreamRecords<TRet>(parameters, count) : getFiltered();
            }
            else
                records = StreamRecords<TRet>(parameters, count);

            if (StreamCount > streamThreshold || cmdlet.ProgressManager.WatchStream)
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

        public IEnumerable<TRet> StreamRecords<TRet>(TParam parameters, int? countToRetrieve, Func<int> totalExist = null)
        {
            if (StreamCount != null)
                SetTotalExist(StreamCount.Value);

            SetStreamCount(parameters, countToRetrieve);

            if (totalExist == null)
            {
                //If TotalExist was known, we set it upon entering StreamRecords.
                //Otherwise, we set it upon entering SetStreamCount
                totalExist = () => GetTotalExist(parameters);
            }

            var manager = new StreamManager<TObject, TParam>(
                PrtgSessionState.Client.ObjectEngine, //Object Engine
                parameters,                           //Parameters
                totalExist,                           //Get Count (Total Exist)
                true,                                 //Serial
                true,                                 //Direct Call
                cmdlet.GetStreamObjects               //Get Objects
            );

            return (IEnumerable<TRet>)PrtgSessionState.Client.ObjectEngine.SerialStreamObjectsInternal(manager);
        }

        public void SetStreamCount(TParam parameters, int? count)
        {
            //If we're forcing a stream, we may have actually specified the Count parameter. As such, override the
            //number of results to retrieve
            if (ForceStream && count != null)
            {
                //Need to know the total number of objects that exist to prevent overlapping with Start offset.
                //In a sense, this is actually unnecessary - StreamManager will request the total number of objects
                //if it needs to know it. Instead, we ensure we know the total number of objects, and then
                //give the StreamManager a Func that simply returns our cached TotalExist value
                StreamCount = Math.Min(count.Value, GetTotalExist(parameters));
            }

            //We went down an alternate code path that doesn't retrieve the total number of needed objects
            if (StreamCount == null)
            {
                if (ForceStream)
                {
                    StreamCount = cmdlet.GetStreamTotalObjects(parameters);
                    SetTotalExist(StreamCount.Value);
                }
            }
        }
    }
}