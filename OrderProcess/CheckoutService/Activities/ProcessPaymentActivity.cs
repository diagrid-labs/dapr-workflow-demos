using CheckoutService.Models;
using Dapr.Client;
using Dapr.Workflow;

namespace CheckoutService.Activities
{
    public class ProcessPaymentActivity(HttpClient httpClient, ILoggerFactory loggerFactory, DaprClient client) : WorkflowActivity<PaymentRequest, PaymentResponse>
    {
        public override async Task<PaymentResponse> RunAsync(WorkflowActivityContext context, PaymentRequest request)
        {
            var logger = loggerFactory.CreateLogger<ProcessPaymentActivity>();
            logger.LogInformation(
                "Calling PaymentService for: {requestId} {name} at ${totalCost}",
                request.RequestId,
                request.Name,
                request.TotalCost);

            // Simulate slow processing
            await Task.Delay(TimeSpan.FromSeconds(3));

            try
            {
                const string INVOKE_APP = "payment-service";
                var httpClient = DaprClient.CreateInvokeHttpClient(appId: INVOKE_APP);
                var httpResponse = await httpClient.PostAsJsonAsync("/pay", request);
                logger.LogInformation(
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
                logger.LogWarning(
                    "Payment for request ID '{requestId}' failed",
                    request.RequestId);

                return new PaymentResponse(request.RequestId, false);

            }
        }
    }
}