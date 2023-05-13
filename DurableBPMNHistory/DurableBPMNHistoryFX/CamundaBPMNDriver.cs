using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DurableBPMNHistoryFX
{
    public class CamundaBPMNDriver
    {

        public XElement LoadDiagram()
        {
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(".", "bpmndiagram.bpmn");
            var root = XElement.Load(path);
            return root;
        }

        public XElement GetDiagramFor(XElement root, DurableOrchestrationStatus status, JArray history)
        {
            var diagramMarkup = string.Empty;
            XNamespace bpmnNamespace = "http://www.omg.org/spec/BPMN/20100524/MODEL";
            XNamespace bpmnDiNamespace = "http://www.omg.org/spec/BPMN/20100524/DI";


            // deserialize shape nodes
            XNamespace digramNamespace = "http://www.omg.org/spec/BPMN/20100524/DI";
            XNamespace bpmnbiOColorNS = "http://bpmn.io/schema/bpmn/biocolor/1.0";
            XNamespace bpmnColor = "http://ww.omg.org/spec/BPMN/non-normative/color/1.0";
            IEnumerable<XElement> result = from nodes in root.Elements(bpmnNamespace + "process")
                                               // where nodes.Name == "sequenceflow"
                                           select nodes;
            var processNode = result.First();
            var planeNodes = root.Element(digramNamespace + "BPMNDiagram");
            var diagramNodes = planeNodes.Elements(); // planeNodes.Element(digramNamespace + "BPMNDiagram");
            var diagramElements = diagramNodes
                 .First().Descendants()
                 .Where(w => w.Name.LocalName.Equals("BPMNShape"));
            var historyNodes = history.Where(w => w["FunctionName"] != null);
            // handle execution graph item.functionname
            foreach (var item in historyNodes)
            {
                var eventType = item["EventType"].ToString();
                var functionName = item["FunctionName"].ToString();

                var targetNodes = diagramElements
                    .Where(w => w.Attributes()
                    .Where(w => w.Value.Equals(functionName)).Any());

                if(targetNodes != null && targetNodes.Count() == 1)
                {
                    var targetNode = targetNodes.First();
                    var targetAttribute = targetNode.Attributes().Where(w => w.Name.LocalName.Equals("background-color"));
                    if (targetAttribute != null && targetAttribute.Any()) {
                        targetAttribute.First().Value = "#FF0000";
                    }
                }
            }

            return root;
        }
    }
}
