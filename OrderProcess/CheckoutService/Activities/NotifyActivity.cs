using CheckoutService.Models;
using Dapr.Workflow;

namespace CheckoutService.Activities
{
    public class NotifyActivity(ILoggerFactory loggerFactory) : WorkflowActivity<Notification, object?>
    {
        public override Task<object?> RunAsync(WorkflowActivityContext context, Notification notification)
        {
            var logger = loggerFactory.CreateLogger<NotifyActivity>();
            logger.LogInformation(notification.Message);

            return Task.FromResult<object?>(null);
        }
    }
}