using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class TypeTests
    {
        [TestMethod]
        public void SensorTarget_ReferenceEquals_SensorTarget()
        {
            ExeFileTarget target1 = "test.ps1";
            ExeFileTarget target2 = target1;

            Assert.IsTrue(target1.Equals(target2));
        }

        [TestMethod]
        public void SensorTarget_ValueEquals_SensorTarget()
        {
            ExeFileTarget target1 = "test.ps1";
            ExeFileTarget target2 = "test.ps1";
            ExeFileTarget target3 = "test.ps2";

            Assert.IsTrue(target1.Equals(target2));
            Assert.IsFalse(target1.Equals(target3));
        }

        [TestMethod]
        public void SensorTarget_HashCodeEquals_SensorTarget()
        {
            ExeFileTarget target1 = "test.ps1";
            ExeFileTarget target2 = "test.ps1";

            var target1Hash = target1.GetHashCode();
            var target2Hash = target2.GetHashCode();

            Assert.AreEqual(target1Hash, target2Hash);
        }
        
        [TestMethod]
        public void ScanningInterval_HasCorrectHashCode()
        {
            ScanningInterval interval1 = ScanningInterval.OneHour;
            ScanningInterval interval2 = new TimeSpan(1, 0, 0);

            var code1 = interval1.GetHashCode();
            var code2 = interval2.GetHashCode();

            Assert.AreEqual(interval1, interval2);

            var timeSpan = new TimeSpan(1, 0, 0);

            Assert.AreNotEqual(code2, timeSpan);
        }
    }
}
