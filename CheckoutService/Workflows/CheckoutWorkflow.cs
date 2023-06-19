using CheckoutService.Activities;
using CheckoutService.Models;
using Dapr.Workflow;
using Microsoft.DurableTask;

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

            // Determine if there is enough of the item available for purchase by checking the inventory.
            var inventoryRequest = new InventoryRequest(RequestId: orderId, order.Name, order.Quantity);
            var inventoryResult = await context.CallActivityAsync<InventoryResult>(
                nameof(CheckInventoryActivity),
                inventoryRequest);

            // If there is insufficient inventory, inform the user and stop the workflow.
            if (!inventoryResult.InStock)
            {
                // End the workflow here since we don't have sufficient inventory.
                await context.CallActivityAsync(
                    nameof(NotifyActivity),
                    new Notification($"Insufficient inventory for {order.Name}"));

                context.SetCustomStatus("Stopped order process due to insufficient inventory.");

                return new CheckoutResult(Processed: false);
            }

            // Create a RetryPolicy to retry calling the ProcessPaymentActivity in case it fails.
            var taskOptions = new WorkflowTaskOptions(
                new WorkflowRetryPolicy(10, TimeSpan.FromSeconds(1), 2));

            // Process the payment (calls the PaymentService).
            var paymentResponse = await context.CallActivityAsync<PaymentResponse>(
                nameof(ProcessPaymentActivity),
                new PaymentRequest(orderId, order.Name, inventoryResult.TotalCost),
                taskOptions);

            // In case the payment fails, notify the user and stop the workflow.
            if (!paymentResponse.IsPaymentSuccess)
            {
                await context.CallActivityAsync(
                    nameof(NotifyActivity),
                    new Notification($"Payment failed for order {orderId}"));

                context.SetCustomStatus("Stopped order process due to payment issue.");

                return new CheckoutResult(Processed: false);
            }

            try
            {
                // Check and update the inventory.
                await context.CallActivityAsync(
                    nameof(UpdateInventoryActivity),
                    inventoryRequest);
            }
            catch (TaskFailedException)
            {
                // The inventory is insufficient, notify the user,
                // perform the compensation action (refund), and end the workflow.
                await context.CallActivityAsync(
                    nameof(NotifyActivity),
                    new Notification($"Order {orderId} Failed! You are now getting a refund"));

                await context.CallActivityAsync(
                    nameof(RefundPaymentActivity),
                    new PaymentRequest(RequestId: orderId, order.Name, inventoryResult.TotalCost));

                context.SetCustomStatus("Stopped order process due to error in inventory update.");

                return new CheckoutResult(Processed: false);
            }

            await context.CallActivityAsync(
                nameof(NotifyActivity),
                new Notification($"Order {orderId} has completed!"));

            return new CheckoutResult(Processed: true);
        }
    }
}