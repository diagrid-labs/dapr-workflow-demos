using Dapr.Client;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

var client = new DaprClientBuilder().Build();
const string CONFIG_STORE_NAME = "configstore";
var configItems = new List<string> { "isPaymentSuccess" };
string subscriptionId = string.Empty;
bool isPaymentSuccess = true;

await SubscribeConfiguration();

app.MapPost("/pay", (PaymentRequest request) =>
{
    Console.WriteLine("PaymentRequest received : " + request.RequestId);
    //return request.RequestId;
    // return a 202 Accepted response
    return Results.Accepted();
});

app.MapPost("/refund", (RefundRequest request) =>
{
    Console.WriteLine("RefundRequest received : " + request.RequestId);
    return request.RequestId;
});

app.Run();


async Task SubscribeConfiguration()
{
    // Get config from configuration store
    GetConfigurationResponse config = await client.GetConfiguration(CONFIG_STORE_NAME, configItems);
    if (config.Items.TryGetValue("isPaymentSuccess", out var isPaymentSuccessItem))
    {
        if (!bool.TryParse(isPaymentSuccessItem.Value, out isPaymentSuccess))
        {
            Console.WriteLine("Can't parse isPaymentSuccessItem to boolean.");
        }
    }
    else
    {
        Console.WriteLine("isPaymentSuccessItem not found");
    }

    // Subscribe for configuration changes
    var subscribe = await client.SubscribeConfiguration(CONFIG_STORE_NAME, configItems);

    // Print configuration changes
    await foreach (var configItem in subscribe.Source)
    {
        // First invocation when app subscribes to config changes only returns subscription id
        if (configItem.Keys.Count == 0)
        {
            Console.WriteLine("App subscribed to config changes with subscription id: " + subscribe.Id);
            subscriptionId = subscribe.Id;
            continue;
        }
        var cfg = System.Text.Json.JsonSerializer.Serialize(configItem);
        Console.WriteLine("Configuration update " + cfg);
    }
}

// Unsubscribe to config updates and exit the app
async Task Unsubscribe(string subscriptionId)
{
    try
    {
        await client.UnsubscribeConfiguration(CONFIG_STORE_NAME, subscriptionId);
        Console.WriteLine("App unsubscribed from config changes");
        Environment.Exit(0);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error unsubscribing from config updates: " + ex.Message);
    }
}
public record PaymentRequest(string RequestId, string Name, double TotalCost);
public record RefundRequest(string RequestId, string Name, double TotalCost);
