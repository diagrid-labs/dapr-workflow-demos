using Microsoft.DurableTask;
using Dapr.Workflow;
using CheckoutService.Activities;
using CheckoutService.Models;

namespace CheckoutService.Workflows
{
    public class CheckoutWorkflow : Workflow<OrderItem, CheckoutResult>
    {
        public override async Task<CheckoutResult> RunAsync(WorkflowContext context, OrderItem order)
        {
            string orderId = context.InstanceId;

            // Notify the user that an order has come through
            await context.CallActivityAsync(
                nameof(NotifyActivity),
                new Notification($"Received order {orderId} for {order.Quantity} {order.Name}"));

            // Determine if there is enough of the item available for purchase by checking the inventory
            var inventoryRequest = new InventoryRequest(RequestId: orderId, order.Name, order.Quantity);
            var inventoryResult = await context.CallActivityAsync<InventoryResult>(
                nameof(CheckInventoryActivity),
                inventoryRequest);

            // If there is insufficient inventory, fail and let the user know 
            if (!inventoryResult.InStock)
            {
                // End the workflow here since we don't have sufficient inventory
                await context.CallActivityAsync(
                    nameof(NotifyActivity),
                    new Notification($"Insufficient inventory for {order.Name}"));
                context.SetCustomStatus("Stopped order process due to insufficient inventory.");

                return new CheckoutResult(Processed: false);
            }

            // Create a RetryPolicy to retry calling the ProcessPaymentActivity in case it fails. 
            var taskOptions = new TaskOptions(
                new TaskRetryOptions(
                    new RetryPolicy(10, TimeSpan.FromSeconds(1), 2)));
            var paymentResponse = await context.CallActivityAsync<PaymentResponse>(
                nameof(ProcessPaymentActivity),
                new PaymentRequest(orderId, order.Name, inventoryResult.TotalCost), taskOptions);

            if (!paymentResponse.IsPaymentSuccess)
            {
                context.SetCustomStatus("Stopped order process due to payment issue.");

                return new CheckoutResult(Processed: false);
            }

            try
            {
                await context.CallActivityAsync(
                    nameof(UpdateInventoryActivity),
                    inventoryRequest);
            }
            catch (Exception ex)
            {
                // Catching this inner exception is a temp workaround when using Dapr 1.10.
                // A different outer exception will be used in Dapr v1.11 that makes it cleaner to work with.
                if (ex.InnerException is DurableTask.Core.Exceptions.TaskFailedException)
                {
                    await context.CallActivityAsync(
                        nameof(NotifyActivity),
                        new Notification($"Order {orderId} Failed! You are now getting a refund"));
                    await context.CallActivityAsync(
                        nameof(RefundPaymentActivity),
                        new PaymentRequest(RequestId: orderId, order.Name, inventoryResult.TotalCost));
                    context.SetCustomStatus("Stopped order process due to error in inventory update.");

                    return new CheckoutResult(Processed: false);
                }
            }

            await context.CallActivityAsync(
                nameof(NotifyActivity),
                new Notification($"Order {orderId} has completed!"));

            return new CheckoutResult(Processed: true);
        }
    }
}