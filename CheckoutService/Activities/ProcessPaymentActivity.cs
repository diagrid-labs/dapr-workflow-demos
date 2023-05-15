using Dapr.Workflow;
using CheckoutService.Models;
using Dapr.Client;

namespace CheckoutService.Activities
{
    public class ProcessPaymentActivity : WorkflowActivity<PaymentRequest, object?>
    {
        private readonly ILogger _logger;
        private readonly DaprClient _client;

        public ProcessPaymentActivity(ILoggerFactory loggerFactory, DaprClient client)
        {
            _logger = loggerFactory.CreateLogger<ProcessPaymentActivity>();
            _client = client;
            
        }

        public override async Task<object?> RunAsync(WorkflowActivityContext context, PaymentRequest req)
        {
            _logger.LogInformation(
                "Processing payment: {requestId} for {name} at ${totalCost}",
                req.RequestId,
                req.Name,
                req.TotalCost);

            var request = _client.CreateInvokeMethodRequest(HttpMethod.Post, "payment", "pay", req);
            await _client.InvokeMethodAsync(request);

            _logger.LogInformation(
                "Payment for request ID '{requestId}' processed successfully",
                req.RequestId);

            return null;
        }
    }
}