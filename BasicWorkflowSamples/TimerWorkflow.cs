using Dapr.Workflow;

namespace BasicWorkflowSamples
{
    public class TimerWorkflow : Workflow<TimerWorkflowInput, string>
    {
        public override async Task<string> RunAsync(WorkflowContext context, TimerWorkflowInput input)
        { 
            if (input.DateTime > context.CurrentUtcDateTime)
            {
                await context.CreateTimer(input.DateTime, default(CancellationToken));
            }

            var message =  await context.CallActivityAsync<string>(
                nameof(CreateGreetingActivity),
                $"{input.Name} at {input.DateTime.ToString("yyyy-MM-dd HH:mm:ss")}");

            return message;
        }
    }
}