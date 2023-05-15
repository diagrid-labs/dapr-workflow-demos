using Dapr.Workflow;
using CheckoutService.Models;
using Dapr.Client;
using System.Net.Http.Headers;

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
                "Processing payment: {requestId} for {name} at ${totalCost}",
                req.RequestId,
                req.Name,
                req.TotalCost);

            var httpClient = DaprClient.CreateInvokeHttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:3501");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("dapr-app-id", "payment");

            var result = await httpClient.PostAsJsonAsync("/pay", req);
            _logger.LogInformation("Payment response: {status}", result.StatusCode.ToString());
            
            // Simulate slow processing
            //await Task.Delay(TimeSpan.FromSeconds(5));

            _logger.LogInformation(
                "Payment for request ID '{requestId}' processed successfully",
                req.RequestId);

            return null;
        }
    }
}