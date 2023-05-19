using CheckoutService.Models;
using Dapr.Client;
using Dapr.Workflow;

namespace CheckoutService.Activities
{
    public class ProcessPaymentActivity : WorkflowActivity<PaymentRequest, PaymentResponse>
    {
        private readonly ILogger _logger;
        private readonly DaprClient _client;

        public ProcessPaymentActivity(ILoggerFactory loggerFactory, DaprClient client)
        {
            _logger = loggerFactory.CreateLogger<ProcessPaymentActivity>();
            _client = client;
        }

        public override async Task<PaymentResponse> RunAsync(WorkflowActivityContext context, PaymentRequest request)
        {
            _logger.LogInformation(
                "Calling PaymentService for: {requestId} {name} at ${totalCost}",
                request.RequestId,
                request.Name,
                request.TotalCost);

            // Simulate slow processing
            await Task.Delay(TimeSpan.FromSeconds(3));

            var methodRequest = _client.CreateInvokeMethodRequest(HttpMethod.Post, "payment", "pay", request);
            try
            {
                await _client.InvokeMethodAsync(methodRequest);
                _logger.LogInformation(
                    "Payment for request ID '{requestId}' processed successfully",
                    request.RequestId);

                return new PaymentResponse(request.RequestId, true);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("500"))
                {
                    // Throw internal server errors up to the workflow so
                    // this activity can be retried.
                    throw;
                }

                // Any other exception is treated as a failed payment.
                _logger.LogWarning(
                    "Payment for request ID '{requestId}' failed",
                    request.RequestId);

                return new PaymentResponse(request.RequestId, false);

            }
        }
    }
}