using System;
using System.Management.Automation;
using System.Reflection;
using System.Threading.Tasks;
using PrtgAPI.PowerShell;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support.Progress;

namespace PrtgAPI.Tests.UnitTests.InfrastructureTests.Support
{
    public class MockProgressWriter : IProgressWriter
    {
        public static void Bind()
        {
            ProgressManager.CustomWriter = new MockProgressWriter();
        }

        public void WriteProgress(ProgressRecord progressRecord)
        {
            ProgressQueue.Enqueue(progressRecord);
        }

        public void WriteProgress(long sourceId, ProgressRecord progressRecord)
        {
            ProgressQueue.Enqueue(progressRecord);
        }
    }
}
