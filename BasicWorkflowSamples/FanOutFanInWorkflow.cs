using Dapr.Workflow;

namespace BasicWorkflowSamples
{
    public class FanOutFanInWorkflow : Workflow<string, string>
    {
        public override async Task<string> RunAsync(WorkflowContext context, string input)
        {
            string orderId = context.InstanceId;

            var tasks = new List<Task<string>>();

            tasks.Add(context.CallActivityAsync<string>(
                nameof(CreateGreetingActivity),
                input));
            
            tasks.Add(context.CallActivityAsync<string>(
                nameof(CreateGreetingActivity),
                input));
            
            tasks.Add(context.CallActivityAsync<string>(
                nameof(CreateGreetingActivity),
                input));

            var messages = await Task.WhenAll(tasks);

            return string.Join(',', messages);
        }
    }
}