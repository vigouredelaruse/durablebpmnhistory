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

namespace DurableBPMNHistoryFX
{
 
    public class BPMNReporter
    {
        /// <summary>
        /// query orchestration state
        /// </summary>
        /// <param name="client"></param>
        /// <param name="top"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private async Task<IEnumerable<DurableOrchestrationStatus>> GetHistory(IDurableOrchestrationClient client, int top, OrchestrationRuntimeStatus[] filter)
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
        public async Task<IActionResult> BPMNReport(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient client,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            OrchestrationRuntimeStatus[] filter = new[]
            {
                OrchestrationRuntimeStatus.Completed
            };

            var history = await this.GetHistory(client, 100, filter);

            return new OkObjectResult(responseMessage);
        }
    }
}
