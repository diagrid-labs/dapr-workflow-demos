using Dapr.Client;
using Dapr.Workflow;
using CheckoutService.Models;

namespace CheckoutService.Activities
{
    public class CheckInventoryActivity(ILoggerFactory loggerFactory, DaprClient client) : WorkflowActivity<InventoryRequest, InventoryResult>
    {
        static readonly string storeName = "statestore";

        public override async Task<InventoryResult> RunAsync(WorkflowActivityContext context, InventoryRequest req)
        {
            var logger = loggerFactory.CreateLogger<CheckInventoryActivity>();
            logger.LogInformation(
                "Checking inventory for order '{requestId}' of {quantity} {name}",
                req.RequestId,
                req.Quantity,
                req.ItemName);

            // Ensure that the store has items
            var product = await client.GetStateAsync<InventoryItem>(
                storeName,
                req.ItemName.ToLowerInvariant());

            // Catch for the case where the statestore isn't setup
            if (product == null)
            {
                // Not enough items.
                return new InventoryResult(false, null, 0);
            }

            logger.LogInformation(
                "There are {quantity} {name} available for purchase",
                product.Quantity,
                product.Name);

            var totalCost = product.PerItemCost * req.Quantity;
            // See if there're enough items to purchase
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