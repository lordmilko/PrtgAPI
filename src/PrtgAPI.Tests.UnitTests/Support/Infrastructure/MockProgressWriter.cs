using System;
using System.Linq;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.PowerShell.Progress;
using PrtgAPI.Tests.UnitTests.Support.Progress;

namespace PrtgAPI.Tests.UnitTests
{
    public class MockProgressWriter : IProgressWriter
    {
        private ProgressWriter realWriter;

        public MockProgressWriter(PrtgCmdlet cmdlet)
        {
            realWriter = new ProgressWriter(cmdlet);
        }

        public static void Bind()
        {
            ProgressManager.CustomWriter = cmdlet => new MockProgressWriter(cmdlet);
        }

        public static void Unbind()
        {
            ProgressManager.CustomWriter = null;
        }

        public void WriteProgress(ProgressRecordEx progressRecord)
        {
            ProgressQueue.Enqueue(progressRecord);

            WriteProtectedProgress(() => realWriter.WriteProgress(progressRecord));
        }

        public void WriteProgress(long sourceId, ProgressRecordEx progressRecord)
        {
            ProgressQueue.Enqueue(progressRecord);

            WriteProtectedProgress(() => realWriter.WriteProgress(sourceId, progressRecord));
        }

        private void WriteProtectedProgress(Action action)
        {
            //PowerShell ISE can be naughty and modify the CurrentOperation and ParentActivityId properties
            //of our records, messing up our expected output predictions. To counteract this, we clone
            //all progress records before writing the progress, then revert any changes WriteProgress
            //may have caused

            if (ProgressManager.progressPipelines.Count > 0)
            {
                var pipeline = ProgressManager.progressPipelines.currentPipeline;

                var preRecords = pipeline.Records.Select(ProgressManager.CloneRecord).ToList();

                action();

                var postRecords = pipeline.Records;

                for (int i = 0; i < postRecords.Count; i++)
                {
                    if (postRecords[i].Activity != preRecords[i].Activity)
                        postRecords[i].Activity = preRecords[i].Activity;

                    if (postRecords[i].StatusDescription != preRecords[i].StatusDescription)
                        postRecords[i].StatusDescription = preRecords[i].StatusDescription;

                    if (postRecords[i].CurrentOperation != preRecords[i].CurrentOperation)
                        postRecords[i].CurrentOperation = preRecords[i].CurrentOperation;

                    if (postRecords[i].ParentActivityId != preRecords[i].ParentActivityId)
                        postRecords[i].ParentActivityId = preRecords[i].ParentActivityId;
                }
            }
            else
                action();
        }
    }
}
