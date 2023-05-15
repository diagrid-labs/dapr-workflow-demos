using Dapr.Workflow;
using BasicWorkflowSamples;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDaprWorkflow(options =>
{
    // Note that it's also possible to register a lambda function as the workflow
    // or activity implementation instead of a class.
    options.RegisterWorkflow<HelloWorldWorkflow>();
    options.RegisterWorkflow<ChainingWorkflow>();
    options.RegisterWorkflow<FanOutFanInWorkflow>();
    options.RegisterWorkflow<ContinueAsNewWorkflow>();
    options.RegisterWorkflow<TimerWorkflow>();

    // These are the activities that get invoked by the workflow(s).
    options.RegisterActivity<CreateGreetingActivity>();
});

// Dapr uses a random port for gRPC by default. If we don't know what that port
// is (because this app was started separate from dapr), then assume 50001.
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DAPR_GRPC_PORT")))
{
    Environment.SetEnvironmentVariable("DAPR_GRPC_PORT", "50001");
}

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
