using Dapr.Workflow;

namespace BasicWorkflowSamples
{
    public class ChildWorkflows : Workflow<string, string[]>
    {
        public override async Task<string[]> RunAsync(WorkflowContext context, string input)
        {
            var helloWorld =  await context.CallChildWorkflowAsync<string>(
                nameof(HelloWorldWorkflow),
                input);

            var chaining = await context.CallChildWorkflowAsync<string>(
                nameof(ChainingWorkflow),
                helloWorld);

            var fanOutFanIn = await context.CallChildWorkflowAsync<string[]>(
                nameof(FanOutFanInWorkflow),
                new [] { helloWorld, chaining });

            return fanOutFanIn;
        }
    }
}