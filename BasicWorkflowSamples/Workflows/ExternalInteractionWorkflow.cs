using Dapr.Workflow;

namespace BasicWorkflowSamples
{
    public class ExternalInteractionWorkflow : Workflow<string, string>
    {
        public override async Task<string> RunAsync(WorkflowContext context, string input)
        {
            var timeOut = TimeSpan.FromSeconds(15);
            var approvalEvent = await context.WaitForExternalEventAsync<ApprovalEvent>("approval-event", timeOut);
            var message = string.Empty;

            if (approvalEvent.isApproved)
            {
                message = await context.CallActivityAsync<string>(
                nameof(CreateGreetingActivity),
                input);
             }

            return message;
        }
    }

    public record ApprovalEvent(bool isApproved);
}