using DurableBPMNHistoryFX;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Smoketests
{
    public class BPMNParserTests
    {
        XElement root;
        CamundaBPMNDriver bpmnDriver;
        JArray executionGraph;

        [SetUp]
        public void Setup()
        {
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(".", "bpmndiagram.bpmn");
            root = XElement.Load(path);


            executionGraph = JArray.Parse(@"[{
                  ""EventType"": ""ExecutionStarted"",
                  ""Correlation"": null,
                  ""ScheduledStartTime"": null,
                  ""Timestamp"": ""2023-05-12T21:20:00.9598119Z"",
                  ""FunctionName"": ""Orchestration""
                },
                {
                  ""EventType"": ""TaskCompleted"",
                  ""Result"": ""Hello to HelloLondon!"",
                  ""Timestamp"": ""2023-05-12T21:20:14.1661503Z"",
                  ""ScheduledTime"": ""2023-05-12T17:20:10.1905954-04:00"",
                  ""FunctionName"": ""HelloLondon""
                },
                {
                  ""EventType"": ""ExecutionCompleted"",
                  ""OrchestrationStatus"": ""Completed"",
                  ""Result"": [
                    ""Hello to HelloLondon!""
                  ],
                  ""Timestamp"": ""2023-05-12T21:20:24.1778151Z""
                }]");

            bpmnDriver = new CamundaBPMNDriver();
        }

        [Test]
        public void CanParseBPMN()
        {
            XNamespace bpmnNamespace = "http://www.omg.org/spec/BPMN/20100524/MODEL";
            XNamespace bpmnDiNamespace = "http://www.omg.org/spec/BPMN/20100524/DI";
            var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var path = System.IO.Path.Combine(".", "bpmndiagram.bpmn");
            var root = XElement.Load(path);
            IEnumerable<XElement> result = from nodes in root.Elements(bpmnNamespace + "process")
                                               // where nodes.Name == "sequenceflow"
                                           select nodes;
            Assert.True(result != null);

            Assert.True(result.Any(), "could not find process node in process definition");

            var processNode = result.First();
            Assert.True(processNode.Attribute("id") != null, "could not find loanRequest process");

            // deserialize shape nodes
            XNamespace digramNamespace = "http://www.omg.org/spec/BPMN/20100524/DI";
            XNamespace bpmnbiOColorNS = "http://bpmn.io/schema/bpmn/biocolor/1.0";
            XNamespace bpmnColor = "http://ww.omg.org/spec/BPMN/non-normative/color/1.0";

            var planeNodes = root.Element(digramNamespace + "BPMNDiagram");
            var diagramNodes = planeNodes.Elements(); // planeNodes.Element(digramNamespace + "BPMNDiagram");
            var diagramElements = diagramNodes
                 .First().Descendants()
                 .Where(w => w.Name.LocalName.Equals("BPMNShape"));

            var attributeSeek = diagramElements.First();

            var colorAttributes = attributeSeek.Attributes()
                .Where(w => w.Name.LocalName.Equals("background-color"));

            // prove can select <bpmndi:BPMNShape>
            foreach (var item in diagramElements)
            {
                var itemName = item.Name;
                Assert.True(itemName.LocalName.Equals("BPMNShape"));
            }

        }

        [Test]
        public void EnsureCamundaBPMNDriver()
        {

            var driverRoot = bpmnDriver.LoadDiagram();
            var output = bpmnDriver.GetDiagramFor(driverRoot, new Microsoft.Azure.WebJobs.Extensions.DurableTask.DurableOrchestrationStatus()
            {
                Input = "HelloLondon"
            },
            this.executionGraph);
            Assert.IsTrue(driverRoot != null);

        }
    }
}