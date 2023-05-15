var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

app.MapPost("/pay", (PaymentRequest request) =>
{
    Console.WriteLine("PaymentRequest received : " + request.RequestId);
    return request.RequestId;
});

app.MapPost("/refund", (RefundRequest request) =>
{
    Console.WriteLine("RefundRequest received : " + request.RequestId);
    return request.RequestId;
});

app.Run();

public record PaymentRequest(string RequestId, string Name, double TotalCost);
public record RefundRequest(string RequestId, string Name, double TotalCost);
