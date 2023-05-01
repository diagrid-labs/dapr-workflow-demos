using Dapr.Workflow;
using WorkflowSample.Activities;

namespace WorkflowSample.Workflows
{
    public class HelloWorldWorkflow : Workflow<string, string>
    {
        public override async Task<string> RunAsync(WorkflowContext context, string input)
        {
            string orderId = context.InstanceId;

            var message = await context.CallActivityAsync<string>(
                nameof(CreateGreetingActivity),
                input);

            return message;
        }
    }
}