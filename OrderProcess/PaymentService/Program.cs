using Dapr.Client;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Dapr uses a random port for gRPC by default. If we don't know what that port
// is (because this app was started separate from dapr), then assume 50001.
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DAPR_GRPC_PORT")))
{
    Environment.SetEnvironmentVariable("DAPR_GRPC_PORT", "50001");
}

var daprClient = new DaprClientBuilder().Build();
const string CONFIG_STORE_NAME = "configstore";
const string configKey = "isPaymentSuccess";
var configItems = new List<string> { configKey };

app.MapPost("/pay", async (PaymentRequest request) =>
{
    Console.WriteLine("PaymentRequest received : " + request.RequestId);
    bool isPaymentSuccess = await GetConfigItemAsync();
    if (isPaymentSuccess)
    {
        Console.WriteLine("Payment successful : " + request.RequestId);

        return Results.Accepted();
    }
    else
    {
        Console.WriteLine("Payment failed : " + request.RequestId);

        return Results.BadRequest();
    }
});

app.MapPost("/refund", async (RefundRequest request) =>
{
    Console.WriteLine("RefundRequest received : " + request.RequestId);
    bool isPaymentSuccess = await GetConfigItemAsync();
    if (isPaymentSuccess)
    {
        Console.WriteLine("Refund successful : " + request.RequestId);

        return Results.Accepted();
    }
    else
    {
        Console.WriteLine("Refund failed : " + request.RequestId);

        return Results.BadRequest();
    }
});

await app.RunAsync();


async Task<bool> GetConfigItemAsync()
{
    var config = await daprClient.GetConfiguration(CONFIG_STORE_NAME, configItems);
    if (config.Items.TryGetValue(configKey, out var isPaymentSuccessItem))
    {
        if (!bool.TryParse(isPaymentSuccessItem.Value, out bool isPaymentSuccess))
        {
            Console.WriteLine("Can't parse isPaymentSuccessItem to boolean.");
        }

        return isPaymentSuccess;
    }
    else
    {
        Console.WriteLine("isPaymentSuccessItem not found");

        //default to true so the happy path of the CheckoutWorkflow always works.
        return true;
    }
}

public record PaymentRequest(string RequestId, string Name, double TotalCost);
public record RefundRequest(string RequestId, string Name, double TotalCost);
