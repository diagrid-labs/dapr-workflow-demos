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
//await client.WaitForSidecarAsync();
const string CONFIG_STORE_NAME = "configstore";
const string configKey = "isPaymentSuccess";
var configItems = new List<string> { configKey };
string subscriptionId = string.Empty;

using (var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
{
    await daprClient.WaitForSidecarAsync(tokenSource.Token);
}

app.MapPost("/pay", async (PaymentRequest request) =>
{
    Console.WriteLine("PaymentRequest received : " + request.RequestId);
    bool isPaymentSuccess = await GetConfigItemAsync();
    if (isPaymentSuccess)
    {
        return Results.Accepted();
    }
    else
    {
        return Results.BadRequest();
    }
});

app.MapPost("/refund", async (RefundRequest request) =>
{
    Console.WriteLine("RefundRequest received : " + request.RequestId);
    bool isPaymentSuccess = await GetConfigItemAsync();
    if (isPaymentSuccess)
    {
        return Results.Accepted();
    }
    else
    {
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
        return true;
    }
}

public record PaymentRequest(string RequestId, string Name, double TotalCost);
public record RefundRequest(string RequestId, string Name, double TotalCost);
