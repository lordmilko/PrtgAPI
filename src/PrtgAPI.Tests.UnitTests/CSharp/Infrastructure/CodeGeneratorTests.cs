#if NETFRAMEWORK

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.CodeGenerator;
using PrtgAPI.CodeGenerator.MethodBuilder;
using PrtgAPI.CodeGenerator.Model;
using PrtgAPI.Tests.UnitTests.Support;

namespace PrtgAPI.Tests.UnitTests.Infrastructure
{
    [TestClass]
    public class CodeGeneratorTests
    {
        [TestMethod]
        [UnitTest(TestCategory.SkipCI)]
        public void CodeGen_PrtgClient_Generates_Synchronous()
        {
            var expected = @"
        /// <summary>
        /// Retrieves a sensor with a specified ID from a PRTG Server.<para/>
        /// If the sensor does not exist or an ambiguous match is found, an <see cref=""InvalidOperationException""/> is thrown.
        /// </summary>
        /// <param name=""id"">The ID of the sensor to retrieve.</param>
        /// <exception cref=""InvalidOperationException"">The specified sensor does not exist or multiple sensors were resolved with the specified ID.</exception>
        /// <returns>The sensor with the specified ID.</returns>
        public Sensor GetSensor(int id) =>
            GetSensors(Property.Id, id).SingleObject(id);
";

            VerifyObjectMethod("Single", MethodType.Synchronous, expected);
        }

        [TestMethod]
        [UnitTest(TestCategory.SkipCI)]
        public void CodeGen_PrtgClient_Generates_Asynchronous()
        {
            var expected = @"
        /// <summary>
        /// Asynchronously retrieves a sensor with a specified ID from a PRTG Server.<para/>
        /// If the sensor does not exist or an ambiguous match is found, an <see cref=""InvalidOperationException""/> is thrown.
        /// </summary>
        /// <param name=""id"">The ID of the sensor to retrieve.</param>
        /// <exception cref=""InvalidOperationException"">The specified sensor does not exist or multiple sensors were resolved with the specified ID.</exception>
        /// <returns>The sensor with the specified ID.</returns>
        public async Task<Sensor> GetSensorAsync(int id) =>
            await GetSensorAsync(id, CancellationToken.None).ConfigureAwait(false);
";

            VerifyObjectMethod("Single", MethodType.Asynchronous, expected);
        }

        [TestMethod]
        [UnitTest(TestCategory.SkipCI)]
        public void CodeGen_PrtgClient_Generates_Stream()
        {
            var expected = @"
        /// <summary>
        /// Streams all sensors from a PRTG Server.<para/>
        /// If <paramref name=""serial""/> is false, when this method's response is enumerated multiple parallel requests will be executed against the PRTG Server
        /// and yielded in the order they return.<para/>
        /// Otherwise, requests will be serially executed as the response is enumerated.
        /// </summary>
        /// <param name=""serial"">Specifies whether PrtgAPI should execute all requests one at a time rather than all at once.</param>
        /// <returns>If <paramref name=""serial""/> is false, a generator encapsulating a series of <see cref=""Task""/> objects capable of streaming a response from a PRTG Server. Otherwise, an enumeration that when iterated retrieves the specified objects.</returns>
        public IEnumerable<Sensor> StreamSensors(bool serial = false) => StreamSensors(new SensorParameters(), serial);
";

            VerifyObjectMethod("Multiple", MethodType.Stream, expected);
        }

        [TestMethod]
        [UnitTest(TestCategory.SkipCI)]
        public void CodeGen_PrtgClient_Region_WithCancellationToken_Generates_SyncAndAsync_NonTokenRegion()
        {
            var expected = @"
        /// <summary>
        /// Retrieves sensors from a PRTG Server using a custom set of parameters.
        /// </summary>
        /// <param name=""parameters"">A custom set of parameters used to retrieve PRTG Sensors.</param>
        /// <returns>A list of sensors that match the specified parameters.</returns>
        public List<Sensor> GetSensors(SensorParameters parameters) =>
            GetSensors(parameters, CancellationToken.None);

        /// <summary>
        /// Asynchronously retrieves sensors from a PRTG Server using a custom set of parameters.
        /// </summary>
        /// <param name=""parameters"">A custom set of parameters used to retrieve PRTG Sensors.</param>
        /// <returns>A list of sensors that match the specified parameters.</returns>
        public async Task<List<Sensor>> GetSensorsAsync(SensorParameters parameters) =>
            await GetSensorsAsync(parameters, CancellationToken.None).ConfigureAwait(false);

        /// <summary>
        /// Streams sensors from a PRTG Server using a custom set of parameters.<para/>
        /// If <paramref name=""serial""/> is false, when this method's response is enumerated multiple parallel requests will be executed against the PRTG Server
        /// and yielded in the order they return.<para/>
        /// Otherwise, requests will be serially executed as the response is enumerated.
        /// </summary>
        /// <param name=""parameters"">A custom set of parameters used to retrieve PRTG Sensors.</param>
        /// <param name=""serial"">Specifies whether PrtgAPI should execute all requests one at a time rather than all at once.</param>
        /// <returns>If <paramref name=""serial""/> is false, a generator encapsulating a series of <see cref=""Task""/> objects capable of streaming a response from a PRTG Server. Otherwise, an enumeration that when iterated retrieves the specified objects.</returns>
        public IEnumerable<Sensor> StreamSensors(SensorParameters parameters, bool serial = false) =>
            ObjectEngine.StreamObjects<Sensor, SensorParameters>(parameters, serial);
";

            VerifyObjectRegion("Parameters", expected);
        }

        [TestMethod]
        [UnitTest(TestCategory.SkipCI)]
        public void CodeGen_PrtgClient_Region_WithCancellationToken_Generates_SyncAndAsync_TokenRegion()
        {
            var expected = @"
        /// <summary>
        /// Retrieves sensors from a PRTG Server using a custom set of parameters with a specified cancellation token.
        /// </summary>
        /// <param name=""parameters"">A custom set of parameters used to retrieve PRTG Sensors.</param>
        /// <param name=""token"">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A list of sensors that match the specified parameters.</returns>
        public List<Sensor> GetSensors(SensorParameters parameters, CancellationToken token) =>
            ObjectEngine.GetObjects<Sensor>(parameters, token: token);

        /// <summary>
        /// Asynchronously retrieves sensors from a PRTG Server using a custom set of parameters with a specified cancellation token.
        /// </summary>
        /// <param name=""parameters"">A custom set of parameters used to retrieve PRTG Sensors.</param>
        /// <param name=""token"">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A list of sensors that match the specified parameters.</returns>
        public async Task<List<Sensor>> GetSensorsAsync(SensorParameters parameters, CancellationToken token) =>
            await ObjectEngine.GetObjectsAsync<Sensor>(parameters, token: token).ConfigureAwait(false);
";

            VerifyObjectRegion("Parameters (Cancellation Token)", expected);
        }

        [TestMethod]
        [UnitTest(TestCategory.SkipCI)]
        public void CodeGen_PrtgClient_Region_WithoutCancellationToken_Generates_Async()
        {
            var doc = GetDocument();
            var config = GetDocumentConfig(doc);

            var methodImpl = GetActionMethod(doc);

            var method = methodImpl.Serialize(methodImpl, config, null);

            var expected = @"
        /// <summary>
        /// Marks a <see cref=""Status.Down""/> sensor as <see cref=""Status.DownAcknowledged""/>. If an acknowledged sensor returns to <see cref=""Status.Up""/>, it will not be acknowledged when it goes down again.
        /// </summary>
        /// <param name=""sensorOrId"">The sensor or ID of the sensor to acknowledge.</param>
        /// <param name=""duration"">Duration (in minutes) to acknowledge the sensor for. If null, sensor will be acknowledged indefinitely.</param>
        /// <param name=""message"">Message to display on the acknowledged sensor.</param>
        public void AcknowledgeSensor(Either<Sensor, int> sensorOrId, int? duration = null, string message = null) =>
            AcknowledgeSensor(new[] {sensorOrId.GetId()}, duration, message);

        /// <summary>
        /// Asynchronously marks a <see cref=""Status.Down""/> sensor as <see cref=""Status.DownAcknowledged""/>. If an acknowledged sensor returns to <see cref=""Status.Up""/>, it will not be acknowledged when it goes down again.
        /// </summary>
        /// <param name=""sensorOrId"">The sensor or ID of the sensor to acknowledge.</param>
        /// <param name=""duration"">Duration (in minutes) to acknowledge the sensor for. If null, sensor will be acknowledged indefinitely.</param>
        /// <param name=""message"">Message to display on the acknowledged sensor.</param>
        /// <param name=""token"">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public async Task AcknowledgeSensorAsync(Either<Sensor, int> sensorOrId, int? duration = null, string message = null, CancellationToken token = default(CancellationToken)) =>
            await AcknowledgeSensorAsync(new[] {sensorOrId.GetId()}, duration, message, token).ConfigureAwait(false);
";

            var actual = string.Join(Environment.NewLine, method.Select(m => m.Definition));

            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
        }

        private void VerifyObjectMethod(string regionName, MethodType type, string expected)
        {
            var doc = GetDocument();
            var config = GetDocumentConfig(doc);

            var methodImpl = GetObjectMethod(doc);
            var template = GetResolvedTemplate(methodImpl, config);

            var region = template.Regions.First(r => r.Name == regionName);
            var methodDef = region.Elements.OfType<MethodDef>().First();

            var writer = new SourceWriter();

            var builder = new MethodRunner(new MethodConfig(methodImpl, methodDef, type, config), writer);

            builder.WriteMethod();

            expected = expected.TrimStart('\r', '\n');

            Assert.AreEqual(expected, writer.ToString());
        }

        private void VerifyObjectRegion(string regionName, string expected)
        {
            var doc = GetDocument();
            var config = GetDocumentConfig(doc);

            var methodImpl = GetObjectMethod(doc);
            var template = GetResolvedTemplate(methodImpl, config);

            var parametersRegion = template.Regions.First(r => r.Name == regionName);
            var methodDef = parametersRegion.Elements.OfType<MethodDef>().Single();

            var method = methodDef.Serialize(methodImpl, config, parametersRegion);

            var actual = string.Join(Environment.NewLine, method.Select(m => m.Definition));

            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
        }

        private MethodImpl GetObjectMethod(Document doc)
        {
            var region = doc.Methods.Regions.First(r => r.Name == "Object Data");

            return region.Elements.OfType<MethodImpl>().First(m => m.Name == "Sensor");
        }

        private InlineMethodDef GetActionMethod(Document doc)
        {
            var region = doc.Methods.Regions.First(r => r.Name == "Object Manipulation");

            return region.Elements.OfType<RegionImpl>().First(r => r.Name == "Sensor State").Elements.OfType<RegionImpl>().First(r => r.Name == "Acknowledge")
                .Elements.OfType<InlineMethodDef>().First();
        }

        private Template GetResolvedTemplate(IMethodImpl methodImpl, DocumentConfig config)
        {
            var template = config.Templates.First(t => t.Name == methodImpl.Template);

            var evaluatedTemplate = new TemplateEvaluator(methodImpl, template, config.Templates);
            var resolvedTemplate = evaluatedTemplate.ResolveAll();

            return resolvedTemplate;
        }

        private Document GetDocument()
        {
            var folder = TestHelpers.GetProjectRoot();

            var xmlFile = folder + "/Request/PrtgClient.Methods.xml";

            var doc = PrtgClientGenerator.GetDocument(xmlFile);

            return doc;
        }

        private DocumentConfig GetDocumentConfig(Document doc)
        {
            return new DocumentConfig(doc.Templates, doc.Resources, doc.CommonParameters);
        }
    }
}
#endif //NETFRAMEWORK
