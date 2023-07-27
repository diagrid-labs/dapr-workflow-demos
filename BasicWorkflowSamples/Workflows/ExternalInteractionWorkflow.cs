using Dapr.Workflow;

namespace BasicWorkflowSamples
{
    public class ExternalInteractionWorkflow : Workflow<string, string>
    {
        public override async Task<string> RunAsync(WorkflowContext context, string input)
        {
            var message = string.Empty;
            try
            {
                var timeOut = TimeSpan.FromSeconds(20);
                var approvalEvent = await context.WaitForExternalEventAsync<ApprovalEvent>(
                    "approval-event",
                    timeOut);

                if (approvalEvent.IsApproved)
                {
                    message = await context.CallActivityAsync<string>(
                    nameof(CreateGreetingActivity),
                    input);
                }
            }
            catch (TaskCanceledException)
            {
                context.SetCustomStatus("Wait for external event is cancelled due to timeout.");
            }

            return message;
        }
    }

    public record ApprovalEvent(bool IsApproved);
}