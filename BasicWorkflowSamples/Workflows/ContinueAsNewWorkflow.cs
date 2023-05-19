using Dapr.Workflow;

namespace BasicWorkflowSamples
{
    public class ContinueAsNewWorkflow : Workflow<int, string>
    {
        public override async Task<string> RunAsync(WorkflowContext context, int counter)
        {
            var message =  await context.CallActivityAsync<string>(
                nameof(CreateGreetingActivity),
                counter.ToString());

            if (counter < 10 )
            {
                counter += 1;
                await context.CreateTimer(TimeSpan.FromSeconds(1));
                context.ContinueAsNew(counter);
            }

            return message;
        }
    }
}