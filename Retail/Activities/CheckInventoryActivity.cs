using Dapr.Client;
using Dapr.Workflow;
using WorkflowSample.Models;

namespace WorkflowSample.Activities
{
    public class CheckInventoryActivity : WorkflowActivity<InventoryRequest, InventoryResult>
    {
        readonly ILogger _logger;
        readonly DaprClient _client;
        static readonly string storeName = "statestore";

        public CheckInventoryActivity(ILoggerFactory loggerFactory, DaprClient client)
        {
            _logger = loggerFactory.CreateLogger<CheckInventoryActivity>();
            _client = client;
        }

        public override async Task<InventoryResult> RunAsync(WorkflowActivityContext context, InventoryRequest req)
        {
            _logger.LogInformation(
                "Checking inventory for order '{requestId}' of {quantity} {name}",
                req.RequestId,
                req.Quantity,
                req.ItemName);

            // Ensure that the store has items
            InventoryItem item = await _client.GetStateAsync<InventoryItem>(
                storeName,
                req.ItemName.ToLowerInvariant());

            // Catch for the case where the statestore isn't setup
            if (item == null)
            {
                // Not enough items.
                return new InventoryResult(false, null, 0);
            }

            _logger.LogInformation(
                "There are {quantity} {name} available for purchase",
                item.Quantity,
                item.Name);

            // See if there're enough items to purchase
            if (item.Quantity >= req.Quantity)
            {
                // Simulate slow processing
                await Task.Delay(TimeSpan.FromSeconds(2));
                var totalCost = item.PerItemCost * req.Quantity;
                return new InventoryResult(true, item, totalCost);
            }

            // Not enough items.
            return new InventoryResult(false, item, 0);

        }
    }
}