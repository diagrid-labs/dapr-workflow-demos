using Dapr.Workflow;
using CheckoutService.Models;
using Dapr.Client;

namespace CheckoutService.Activities
{
    public class RefundPaymentActivity : WorkflowActivity<PaymentRequest, object?>
    {
        private readonly ILogger _logger;
        private readonly DaprClient _client;

        public RefundPaymentActivity(ILoggerFactory loggerFactory, DaprClient client)
        {
            _client = client;
            _logger = loggerFactory.CreateLogger<RefundPaymentActivity>();
        }

        public override async Task<object?> RunAsync(WorkflowActivityContext context, PaymentRequest req)
        {
            _logger.LogInformation(
                "Refunding payment: {requestId} for {name} at ${totalCost}",
                req.RequestId,
                req.Name,
                req.TotalCost);

            var request = _client.CreateInvokeMethodRequest(HttpMethod.Post, "payment", "refund", req);
            await _client.InvokeMethodAsync(request);

            _logger.LogInformation(
                "Payment for request ID '{requestId}' refunded successfully",
                req.RequestId);

            return null;
        }
    }
}