using Dapr.Client;
using Dapr.Workflow;
using WorkflowSample.Models;

namespace WorkflowSample.Activities
{
    class UpdateInventoryActivity : WorkflowActivity<PaymentRequest, object?>
    {
        static readonly string storeName = "statestore";
        readonly ILogger _logger;
        readonly DaprClient _client;

        public UpdateInventoryActivity(ILoggerFactory loggerFactory, DaprClient client)
        {
            _logger = loggerFactory.CreateLogger<UpdateInventoryActivity>();
            _client = client;
        }

        public override async Task<object?> RunAsync(WorkflowActivityContext context, PaymentRequest req)
        {
            _logger.LogInformation(
                "Re-checking inventory for order '{requestId}' for {amount} {name}",
                req.RequestId,
                req.Amount,
                req.ItemName);

            // Simulate slow processing
            await Task.Delay(TimeSpan.FromSeconds(5));

            // Determine if there are enough Items for purchase
            InventoryItem item = await _client.GetStateAsync<InventoryItem>(
                storeName,
                req.ItemName.ToLowerInvariant());
            int newQuantity = item.Quantity - req.Amount;
            if (newQuantity < 0)
            {
                _logger.LogInformation(
                    "Inventory update for request ID '{requestId}' could not be processed. Insufficient inventory.",
                    req.RequestId);
                throw new InvalidOperationException("Insufficient inventory.");
            }

            // Update the statestore with the new amount of the item
            await _client.SaveStateAsync(
                storeName,
                req.ItemName.ToLowerInvariant(),
                new InventoryItem(Name: req.ItemName, PerItemCost: item.PerItemCost, Quantity: newQuantity));

            _logger.LogInformation(
                "There are now {quantity} {name} left in stock",
                newQuantity,
                item.Name);

            return null;
        }
    }
}