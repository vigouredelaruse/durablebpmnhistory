using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DurableBPMNHistoryFX
{
    public enum GatewayPath
    {
        None,
        HelloTokyo,
        HelloSeattle,
        HelloLondon
    }

    public class Orchestrator
    {
        [FunctionName("Orchestration")]
        public async Task<List<string>> Orchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var input = context.GetInput<string>();
            GatewayPath currentCase;
            Enum.TryParse(input, out currentCase);

            var outputs = new List<string>();

            switch (currentCase)
            {
                case GatewayPath.None:
                    {
                        outputs.Add("said hello to no one");
                        break;
                    }

                case GatewayPath.HelloLondon:
                    {
                        outputs.Add(await context.CallActivityAsync<string>(nameof(Orchestrator.HelloLondon), "to HelloLondon"));
                        break;
                    }

                case GatewayPath.HelloSeattle:
                    {
                        outputs.Add(await context.CallActivityAsync<string>(nameof(Orchestrator.HelloSeattle), "to HelloSeattle"));
                        break;
                    }

                case GatewayPath.HelloTokyo:
                    {
                        outputs.Add(await context.CallActivityAsync<string>(nameof(Orchestrator.HelloTokyo), "to HelloTokyo"));
                        break;
                    }
            }

            return outputs;
        }

        [FunctionName(nameof(Orchestrator.HelloTokyo))]
        public string HelloTokyo([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"said hello {name}!";
        }

        [FunctionName(nameof(Orchestrator.HelloLondon))]
        public string HelloLondon([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"said hello {name}!";
        }

        [FunctionName(nameof(Orchestrator.HelloSeattle))]
        public string HelloSeattle([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"said hello {name}!";
        }

        [FunctionName("Start")]
        public async Task Start(
            [TimerTrigger("0 */1 * * * *")]TimerInfo timer,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var gatewayInputs = new GatewayPath[]
            {
                GatewayPath.None,
                GatewayPath.HelloTokyo,
                GatewayPath.HelloLondon,
                GatewayPath.HelloSeattle
            };

            for (int i = 0; i < 5; i++  )
            {
                Random r = new Random();
                int ndx = r.Next(0, 3);
                // Function input comes from the request content.
                string instanceId = await starter.StartNewAsync<string>(nameof(Orchestration), gatewayInputs[ndx].ToString());

                log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            }


        }
    }
}