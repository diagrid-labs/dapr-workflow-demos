using Dapr.Workflow;

namespace BasicWorkflowSamples
{
    public class TimerWorkflow : Workflow<TimerWorkflowInput, string>
    {
        public override async Task<string> RunAsync(WorkflowContext context, TimerWorkflowInput input)
        { 
            if (input.DateTime.ToUniversalTime() > context.CurrentUtcDateTime)
            {
                context.SetCustomStatus($"Waiting for timer: {input.DateTime:yyyy-MM-dd HH:mm:ss}");
                await context.CreateTimer(input.DateTime, default);
            }

            context.SetCustomStatus(null);

            var message =  await context.CallActivityAsync<string>(
                nameof(CreateGreetingActivity),
                $"{input.Name} at {input.DateTime:yyyy-MM-dd HH:mm:ss}");

            return message;
        }
    }
}