using Dapr.Workflow;
using CheckoutService.Models;
using Dapr.Client;

namespace CheckoutService.Activities
{
    public class RefundPaymentActivity : WorkflowActivity<PaymentRequest, PaymentResponse>
    {
        private readonly ILogger _logger;
        private readonly DaprClient _client;

        public RefundPaymentActivity(ILoggerFactory loggerFactory, DaprClient client)
        {
            _client = client;
            _logger = loggerFactory.CreateLogger<RefundPaymentActivity>();
        }

        public override async Task<PaymentResponse> RunAsync(WorkflowActivityContext context, PaymentRequest request)
        {
            _logger.LogInformation(
                "Refunding payment: {requestId} for {name} at ${totalCost}",
                request.RequestId,
                request.Name,
                request.TotalCost);

            // Simulate slow processing
            await Task.Delay(TimeSpan.FromSeconds(3));

            var methodRequest = _client.CreateInvokeMethodRequest(HttpMethod.Post, "payment", "refund", request);
            try
            {
                await _client.InvokeMethodAsync(methodRequest);
                _logger.LogInformation(
                    "Refund for request ID '{requestId}' processed successfully",
                    request.RequestId);
                return new PaymentResponse(request.RequestId, true);
            }
            catch (Exception)
            {
                _logger.LogWarning(
                    "Refund for request ID '{requestId}' failed",
                    request.RequestId);
                return new PaymentResponse(request.RequestId, false);
            }
        }
    }
}