using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class TypeTests
    {
        #region Sensor Target

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

            Assert.AreEqual(code1, code2);

            var timeSpan = new TimeSpan(1, 0, 0);

            Assert.AreNotEqual(code2, timeSpan);
        }

        #endregion
        #region Device Template

        [TestMethod]
        public void DeviceTemplate_ReferenceEquals_DeviceTemplate()
        {
            DeviceTemplate template1 = new DeviceTemplate("file.odt|File||");
            DeviceTemplate template2 = template1;

            Assert.IsTrue(template1.Equals(template2));
        }

        [TestMethod]
        public void DeviceTemplate_ValueEquals_DeviceTemplate()
        {
            DeviceTemplate template1 = new DeviceTemplate("file1.odt|File 1||");
            DeviceTemplate template2 = new DeviceTemplate("file1.odt|File 1||");
            DeviceTemplate template3 = new DeviceTemplate("file2.odt|File 2||");

            Assert.IsTrue(template1.Equals(template2));
            Assert.IsFalse(template1.Equals(template3));
        }

        [TestMethod]
        public void DeviceTemplate_HashCodeEquals_DeviceTemplate()
        {
            DeviceTemplate template1 = new DeviceTemplate("file.odt|File||");
            DeviceTemplate template2 = new DeviceTemplate("file.odt|File||");

            var template1Hash = template1.GetHashCode();
            var template2Hash = template2.GetHashCode();

            Assert.AreEqual(template1Hash, template2Hash);
        }

        [TestMethod]
        public void DeviceTemplate_StringEquals_DeviceTemplate()
        {
            DeviceTemplate template1 = new DeviceTemplate("file.odt|File||");
            DeviceTemplate template2 = new DeviceTemplate("file.odt|File||");

            Assert.AreEqual(template1.ToString(), template2.ToString());
        }

        #endregion
    }
}
