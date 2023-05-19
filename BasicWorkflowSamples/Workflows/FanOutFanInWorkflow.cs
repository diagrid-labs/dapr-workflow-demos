using Dapr.Workflow;

namespace BasicWorkflowSamples
{
    public class FanOutFanInWorkflow : Workflow<string[], string[]>
    {
        public override async Task<string[]> RunAsync(WorkflowContext context, string[] input)
        {
            var tasks = new List<Task<string>>();

            foreach (var name in input)
            {
                tasks.Add(context.CallActivityAsync<string>(
                    nameof(CreateGreetingActivity),
                    name));
            }

            var messages = await Task.WhenAll(tasks);

            return messages;
        }
    }
}