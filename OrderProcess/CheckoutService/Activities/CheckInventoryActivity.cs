using Dapr.Client;
using Dapr.Workflow;
using CheckoutService.Models;

namespace CheckoutService.Activities
{
    public class CheckInventoryActivity : WorkflowActivity<InventoryRequest, InventoryResult>
    {
        readonly ILogger _logger;
        readonly DaprClient _client;
        string storeName = Environment.GetEnvironmentVariable("INVENTORY_STORE") ?? "statestore";


        public CheckInventoryActivity(ILoggerFactory loggerFactory, DaprClient client)
        {
            _logger = loggerFactory.CreateLogger<CheckInventoryActivity>();
            _client = client;
        }

        public override async Task<InventoryResult> RunAsync(WorkflowActivityContext context, InventoryRequest req)
        {
            _logger.LogInformation(
                "Checking inventory for order '{requestId}' of {quantity} {itemName}",
                req.RequestId,
                req.Quantity,
                req.ItemName);

            // Ensure that the store has items
            var product = await _client.GetStateAsync<InventoryItem>(
                storeName,
                req.ItemName.ToLowerInvariant());

            // Catch for the case where the statestore isn't setup
            if (product == null)
            {
                // Not enough items.
                return new InventoryResult(false, null, 0);
            }

            _logger.LogInformation(
                "There are {quantity} {itemName} available for purchase",
                product.Quantity,
                product.Name);

            var totalCost = product.PerItemCost * req.Quantity;
            // See if there are enough items to purchase
            if (product.Quantity >= req.Quantity)
            {
                // Simulate slow processing
                await Task.Delay(TimeSpan.FromSeconds(3));

                return new InventoryResult(true, product, totalCost);
            }

            // Not enough items.
            return new InventoryResult(false, product, totalCost);
        }
    }
}