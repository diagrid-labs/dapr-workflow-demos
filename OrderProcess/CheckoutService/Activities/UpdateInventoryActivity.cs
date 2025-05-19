using Dapr.Client;
using Dapr.Workflow;
using CheckoutService.Models;

namespace CheckoutService.Activities
{
    class UpdateInventoryActivity(ILoggerFactory loggerFactory, DaprClient client) : WorkflowActivity<InventoryRequest, object?>
    {
        static readonly string storeName = "statestore";

        public override async Task<object?> RunAsync(WorkflowActivityContext context, InventoryRequest req)
        {
            var logger = loggerFactory.CreateLogger<UpdateInventoryActivity>();
            logger.LogInformation(
                "Re-checking inventory for order '{requestId}' for {quantity} {itemName}",
                req.RequestId,
                req.Quantity,
                req.ItemName);

            // Simulate slow processing
            await Task.Delay(TimeSpan.FromSeconds(5));

            // Determine if there are enough Items for purchase
            var product = await client.GetStateAsync<InventoryItem>(
                storeName,
                req.ItemName.ToLowerInvariant());
            int newQuantity = product.Quantity - req.Quantity;
            if (newQuantity < 0)
            {
                logger.LogInformation(
                    "Inventory update for request ID '{requestId}' could not be processed. Insufficient inventory.",
                    req.RequestId);
                throw new InvalidOperationException("Insufficient inventory.");
            }

            // Update the statestore with the new amount of the item
            await client.SaveStateAsync(
                storeName,
                req.ItemName.ToLowerInvariant(),
                new InventoryItem(ProductId: product.ProductId, Name: req.ItemName, PerItemCost: product.PerItemCost, Quantity: newQuantity));

            logger.LogInformation(
                "There are now {quantity} {name} left in stock",
                newQuantity,
                product.Name);

            return null;
        }
    }
}