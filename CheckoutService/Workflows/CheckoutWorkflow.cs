using Dapr.Workflow;
using DurableTask.Core.Exceptions;

using CheckoutService.Activities;
using CheckoutService.Models;

namespace CheckoutService.Workflows
{
    public class CheckoutWorkflow : Workflow<OrderPayload, OrderResult>
    {
        public override async Task<OrderResult> RunAsync(WorkflowContext context, OrderPayload order)
        {
            string orderId = context.InstanceId;

            // Notify the user that an order has come through
            await context.CallActivityAsync(
                nameof(NotifyActivity),
                new Notification($"Received order {orderId} for {order.Quantity} {order.Name}"));

            // Determine if there is enough of the item available for purchase by checking the inventory
            InventoryResult inventoryResult = await context.CallActivityAsync<InventoryResult>(
                nameof(CheckInventoryActivity),
                new InventoryRequest(RequestId: orderId, order.Name, order.Quantity));

            // If there is insufficient inventory, fail and let the user know 
            if (!inventoryResult.Success)
            {
                // End the workflow here since we don't have sufficient inventory
                await context.CallActivityAsync(
                    nameof(NotifyActivity),
                    new Notification($"Insufficient inventory for {order.Name}"));
                context.SetCustomStatus("Stopped order process due to insufficient inventory.");

                return new OrderResult(Processed: false);
            }

            await context.CallActivityAsync(
                nameof(ProcessPaymentActivity),
                new PaymentRequest(RequestId: orderId, order.Name, order.Quantity, inventoryResult.TotalCost));

            try
            {
                await context.CallActivityAsync(
                    nameof(UpdateInventoryActivity),
                    new PaymentRequest(RequestId: orderId, order.Name, order.Quantity, inventoryResult.TotalCost));
            }
            catch (Exception ex)
            {
                if (ex.InnerException is TaskFailedException)
                {
                    await context.CallActivityAsync(
                        nameof(NotifyActivity),
                        new Notification($"Order {orderId} Failed! You are now getting a refund"));
                    await context.CallActivityAsync(
                        nameof(RefundPaymentActivity),
                        new PaymentRequest(RequestId: orderId, order.Name, order.Quantity, inventoryResult.TotalCost));
                    context.SetCustomStatus("Stopped order process due to error in inventory update.");

                    return new OrderResult(Processed: false);
                }
            }

            await context.CallActivityAsync(
                nameof(NotifyActivity),
                new Notification($"Order {orderId} has completed!"));

            return new OrderResult(Processed: true);
        }
    }
}