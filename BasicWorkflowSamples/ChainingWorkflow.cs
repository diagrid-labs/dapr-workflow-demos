using Dapr.Workflow;

namespace BasicWorkflowSamples
{
    public class ChainingWorkflow : Workflow<string, string>
    {
        public override async Task<string> RunAsync(WorkflowContext context, string input)
        {
            string orderId = context.InstanceId;

            var message1 =  await context.CallActivityAsync<string>(
                nameof(CreateGreetingActivity),
                input);
            
            var message2 = await context.CallActivityAsync<string>(
                nameof(CreateGreetingActivity),
                message1);
            
            var message3 = await context.CallActivityAsync<string>(
                nameof(CreateGreetingActivity),
                message2);

            return message3;
        }
    }
}