# what is this

this is a proof of concept that proves the concept for rendering the execution history of azure durable functions as bpmn diagrams readable in 
* camunda bpmn modeller
* bpmn.io javascript modeller

generally speaking, azure durable functions orchestrations can be modelled usng bpmn semantics

further, the execution history of orchestrations can be used to manipulate the xml definitions used in bpmn diagrams, such as produced by the camunda bpmn modeller 

this poc examines
* how to use the camunda modeller to generate a machine readable depiction of an azure durable function, something the camunda developers are also interested in for whatever reason
* how to deliver the diagram with the orchestration and permit generation of execution paths 
* how to parse the maximal bpmn element set with the minimum of code
