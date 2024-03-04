using CheckoutService.Models;
using System.Net;
using Dapr.Client;
using Dapr.Workflow;

namespace CheckoutService.Activities
{
    public class NotifyActivity : WorkflowActivity<Notification, Object?>
    {
        private readonly ILogger _logger;

        private readonly DaprClient _client;

        static string PubSubName = Environment.GetEnvironmentVariable("PUBSUB_NAME") ?? "pubsub";
        static string TopicName = Environment.GetEnvironmentVariable("TOPIC_NAME") ?? "notifications";

        public NotifyActivity(ILoggerFactory loggerFactory, DaprClient client)
        {
            _logger = loggerFactory.CreateLogger<NotifyActivity>();
            _client = client;
        }

        public override async Task<object?> RunAsync(WorkflowActivityContext context, Notification notification)
        {
            _logger.LogInformation("Publishing notification: {message}", notification.Message);

            try
            {
                await _client.PublishEventAsync(PubSubName, TopicName, notification);
                _logger.LogInformation("Successfully published notification to topic {topic}", TopicName);

                return Results.Ok(notification);
            }
            catch (Exception e)
            {
                if (e.InnerException != null && e.InnerException.Message.Contains("500"))
                {
                    // Throw internal server errors up to the workflow so
                    // this activity can be retried.
                    throw;
                }

                _logger.LogError("Publishing notification failed with error: {exception}", e.Message + e.InnerException?.Message);

                return Results.Problem(e.Message, null, (int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
