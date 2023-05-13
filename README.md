# what is this

this is a proof of concept that proves the concept for rendering the execution history of azure durable functions as annotated bpmn diagrams readable in 
* camunda bpmn modeller
* bpmn.io javascript modeller

this is the camunda modelling part of the workflow
![image](https://github.com/vigouredelaruse/durablebpmnhistory/assets/31052867/095f2308-87c3-48b5-a868-658dd69e60ac)

the names and ids of the bpmn elements match this orchestrator function's names
![image](https://github.com/vigouredelaruse/durablebpmnhistory/assets/31052867/c8de19fd-dd04-47ba-aa31-9b11ac4c1dd0)

given this testset durable functions history graph
```
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
```

the poc renders this annotated diagrasm
![image](https://github.com/vigouredelaruse/durablebpmnhistory/assets/31052867/4b33e707-913d-40e4-884d-9fcd27f16944)

this is the postman part of the workflow
![image](https://github.com/vigouredelaruse/durablebpmnhistory/assets/31052867/ca41118a-a77f-4d12-bfb3-bda3d92165a8)

generally speaking, azure durable functions orchestrations can be modelled usng bpmn semantics

further, the execution history of orchestrations can be used to manipulate the xml definitions used in bpmn diagrams, such as produced by the camunda bpmn modeller 

this poc examines
* how to use the camunda modeller to generate a machine readable depiction of an azure durable function, something the camunda developers are also interested in for whatever reason
* how to deliver the diagram with the orchestration and permit generation of execution paths 
* how to parse the maximal bpmn element set with the minimum of code
