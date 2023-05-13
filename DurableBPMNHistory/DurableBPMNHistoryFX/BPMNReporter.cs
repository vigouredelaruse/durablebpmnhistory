using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;

namespace DurableBPMNHistoryFX
{
 
    public class BPMNReporter
    {
        /// <summary>
        /// expand history query to include activity
        /// </summary>
        /// <param name="client"></param>
        /// <param name="orchestrationId"></param>
        /// <returns></returns>
        private async Task<DurableOrchestrationStatus> GetHistory(IDurableOrchestrationClient client, string orchestrationId)
        {

            var result = await client
                .GetStatusAsync(orchestrationId, showHistory: true, showHistoryOutput: true, showInput: true);

            return result;
        }

        /// <summary>
        /// query orchestration state
        /// </summary>
        /// <param name="client"></param>
        /// <param name="top"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private async Task<IEnumerable<DurableOrchestrationStatus>> GetHistories(IDurableOrchestrationClient client, int top, OrchestrationRuntimeStatus[] filter)
        {
            var filterQuery = new OrchestrationStatusQueryCondition()
            {
                RuntimeStatus = filter,
                PageSize = top,
                ShowInput = true,
                
            };

            var result = await client.ListInstancesAsync(filterQuery, CancellationToken.None);
            return result.DurableOrchestrationState;
        }


        /// <summary>
        /// render collection of bpmn diagrams
        /// showing execution paths
        /// </summary>
        /// <param name="req"></param>
        /// <param name="client"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(nameof(BPMNReporter.BPMNReport))]
        public async Task<ActionResult<IEnumerable<XElement>>> BPMNReport(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient client,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var camundaDriver = new CamundaBPMNDriver();
            var result = new List<XElement>();
            //string name = req.Query["name"];

            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            //name = name ?? data?.name;

            //string responseMessage = string.IsNullOrEmpty(name)
            //    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
            //    : $"Hello, {name}. This HTTP triggered function executed successfully.";

            OrchestrationRuntimeStatus[] filter = new[]
            {
                OrchestrationRuntimeStatus.Completed
            };

            var history = await this.GetHistories(client, 3, filter);
            foreach (var item in history)
            {
                // item.input = HelloLondon
                // item.output = hello from london
                var state = await this.GetHistory(client, item.InstanceId);
                var stateHistory = state.History;
                /*
                 * 
                 * {
                  "EventType": "ExecutionStarted",
                  "Correlation": null,
                  "ScheduledStartTime": null,
                  "Timestamp": "2023-05-12T21:20:00.9598119Z",
                  "FunctionName": "Orchestration"
                }	Newtonsoft.Json.Linq.JToken+LineInfoAnnotation	Object	True	[0]
                {
                  "EventType": "TaskCompleted",
                  "Result": "Hello to HelloLondon!",
                  "Timestamp": "2023-05-12T21:20:14.1661503Z",
                  "ScheduledTime": "2023-05-12T17:20:10.1905954-04:00",
                  "FunctionName": "HelloLondon"
                }	Newtonsoft.Json.Linq.JToken+LineInfoAnnotation	Object	True	[1]
                {
                  "EventType": "ExecutionCompleted",
                  "OrchestrationStatus": "Completed",
                  "Result": [
                    "Hello to HelloLondon!"
                  ],
                  "Timestamp": "2023-05-12T21:20:24.1778151Z"
                }	Newtonsoft.Json.Linq.JToken+LineInfoAnnotation	Object	True	[2]

                */

                // modify the diagram

                var driverRoot = camundaDriver.LoadDiagram();
                var itemResult = camundaDriver.GetDiagramFor(driverRoot, item, stateHistory);
                result.Add(itemResult);
            }

            return new OkObjectResult(result); // { ContentType = "application/xml", Content = result.ToString() };
        }
    }
}
