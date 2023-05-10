using Dapr.Workflow;
using CheckoutService.Models;

namespace CheckoutService.Activities
{
    public class ProcessPaymentActivity : WorkflowActivity<PaymentRequest, object?>
    {
        readonly ILogger _logger;

        public ProcessPaymentActivity(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ProcessPaymentActivity>();
        }

        public override async Task<object?> RunAsync(WorkflowActivityContext context, PaymentRequest req)
        {
            _logger.LogInformation(
                "Processing payment: {requestId} for {amount} {item} at ${currency}",
                req.RequestId,
                req.Amount,
                req.ItemName,
                req.Currency);

            // Simulate slow processing
            await Task.Delay(TimeSpan.FromSeconds(5));

            _logger.LogInformation(
                "Payment for request ID '{requestId}' processed successfully",
                req.RequestId);

            return null;
        }
    }
}