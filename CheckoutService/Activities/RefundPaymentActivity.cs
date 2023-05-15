using Dapr.Workflow;
using CheckoutService.Models;

namespace CheckoutService.Activities
{
    public class RefundPaymentActivity : WorkflowActivity<PaymentRequest, object?>
    {
        readonly ILogger _logger;

        public RefundPaymentActivity(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<RefundPaymentActivity>();
        }

        public override async Task<object?> RunAsync(WorkflowActivityContext context, PaymentRequest req)
        {
            _logger.LogInformation(
                "Refunding payment: {requestId} for {name} at ${totalCost}",
                req.RequestId,
                req.Name,
                req.TotalCost);

            // Simulate slow processing
            await Task.Delay(TimeSpan.FromSeconds(2));

            _logger.LogInformation(
                "Payment for request ID '{requestId}' refunded successfully",
                req.RequestId);

            return null;
        }
    }
}