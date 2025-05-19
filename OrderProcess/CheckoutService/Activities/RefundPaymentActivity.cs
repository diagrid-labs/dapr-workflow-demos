using Dapr.Client;
using Dapr.Workflow;
using CheckoutService.Models;

namespace CheckoutService.Activities
{
    public class RefundPaymentActivity(ILoggerFactory loggerFactory) : WorkflowActivity<PaymentRequest, PaymentResponse>
    {

        public override async Task<PaymentResponse> RunAsync(WorkflowActivityContext context, PaymentRequest request)
        {
            var logger = loggerFactory.CreateLogger<RefundPaymentActivity>();
            logger.LogInformation(
                "Refunding payment: {requestId} for {name} at ${totalCost}",
                request.RequestId,
                request.Name,
                request.TotalCost);

            // Simulate slow processing
            await Task.Delay(TimeSpan.FromSeconds(3));

            try
            {
                const string INVOKE_APP = "payment-service";
                var httpClient = DaprClient.CreateInvokeHttpClient(appId: INVOKE_APP);
                var httpResponse = await httpClient.PostAsJsonAsync("/refund", request);
                if (httpResponse.IsSuccessStatusCode)
                {
                    logger.LogInformation(
                    "Refund for request ID '{requestId}' processed successfully",
                    request.RequestId);
                    return new PaymentResponse(request.RequestId, true);
                }
                return new PaymentResponse(request.RequestId, false);
            }
            catch (Exception)
            {
                logger.LogWarning(
                    "Refund for request ID '{requestId}' failed",
                    request.RequestId);
                return new PaymentResponse(request.RequestId, false);
            }
        }
    }
}