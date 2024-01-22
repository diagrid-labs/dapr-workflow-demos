using CheckoutService.Activities;
using CheckoutService.Models;
using CheckoutService.Workflows;
using Dapr.Workflow;
using FluentAssertions;
using NSubstitute;

namespace CheckoutService.Tests;

public class CheckoutWorkflowTests
{
    [Fact]
    public async void GivenInventoryIsTooLow_WhenCallingCheckInventory_ThenWorkflowResultShouldBeFalse()
    {
        var workflowContext = GetContextWithInventoryResultIsNotProcessed();
        var checkoutWorkflow = new CheckoutWorkflow();
        var orderItem = new OrderItem("Test", 1);
        var workflowResult = await checkoutWorkflow.RunAsync(workflowContext, orderItem);
        workflowResult.Processed.Should().BeFalse();
    }

    [Fact]
    public async void GivenInventoryIsEnough_WhenCallingCheckInventory_ThenWorkflowResultShouldBeTrue()
    {
        var workflowContext = GetContextWithInventoryResultIsProcessedAndSuccessfulPayment();
        var checkoutWorkflow = new CheckoutWorkflow();
        var orderItem = new OrderItem("Test", 1);
        var workflowResult = await checkoutWorkflow.RunAsync(workflowContext, orderItem);
        workflowResult.Processed.Should().BeTrue();
    }

    [Fact]
    public async void GivenPaymentProcessIsUnsuccessful_WhenCallingProcessPaymentActivity_ThenWorkflowResultShouldBeFalse()
    {
        var workflowContext = GetContextWithInventoryResultIsProcessedAndUnsuccessfulPayment();
        var checkoutWorkflow = new CheckoutWorkflow();
        var orderItem = new OrderItem("Test", 1);
        var workflowResult = await checkoutWorkflow.RunAsync(workflowContext, orderItem);
        workflowResult.Processed.Should().BeFalse();
    }

    private static WorkflowContext GetContextWithInventoryResultIsProcessedAndSuccessfulPayment()
    {
        var workflowContext = Substitute.For<WorkflowContext>();
        var inventoryItem = new InventoryItem(1, "TestItem", 100, 1);
        workflowContext.CallActivityAsync<InventoryResult>(
            Arg.Is("CheckInventoryActivity"),
            Arg.Any<InventoryRequest>(),
            Arg.Any<WorkflowTaskOptions>())
            .Returns(Task.FromResult(new InventoryResult(
                InStock: true,
                inventoryItem,
                TotalCost: 100)));

        workflowContext.CallActivityAsync<PaymentResponse>(
            Arg.Is(nameof(ProcessPaymentActivity)),
            Arg.Any<PaymentRequest>(),
            Arg.Any<WorkflowTaskOptions>())
            .Returns(Task.FromResult(new PaymentResponse(
                "",
                IsPaymentSuccess: true)));

        return workflowContext;
    }

    private static WorkflowContext GetContextWithInventoryResultIsProcessedAndUnsuccessfulPayment()
    {
        var workflowContext = Substitute.For<WorkflowContext>();
        var inventoryItem = new InventoryItem(1, "TestItem", 100, 1);
        workflowContext.CallActivityAsync<InventoryResult>(
            Arg.Is("CheckInventoryActivity"),
            Arg.Any<InventoryRequest>(),
            Arg.Any<WorkflowTaskOptions>())
            .Returns(Task.FromResult(new InventoryResult(
                InStock: true,
                inventoryItem,
                TotalCost: 100)));

        workflowContext.CallActivityAsync<PaymentResponse>(
            Arg.Is(nameof(ProcessPaymentActivity)),
            Arg.Any<PaymentRequest>(),
            Arg.Any<WorkflowTaskOptions>())
            .Returns(Task.FromResult(new PaymentResponse(
                "",
                IsPaymentSuccess: false)));

        return workflowContext;
    }

    private static WorkflowContext GetContextWithInventoryResultIsNotProcessed()
    {
        var workflowContext = Substitute.For<WorkflowContext>();
        var inventoryItem = new InventoryItem(1, "TestItem", 100, 1);
        workflowContext.CallActivityAsync<InventoryResult>(
            Arg.Is(nameof(CheckInventoryActivity)),
            Arg.Any<InventoryRequest>(),
            Arg.Any<WorkflowTaskOptions>())
            .Returns(Task.FromResult(new InventoryResult(
                InStock: false,
                inventoryItem,
                TotalCost: 100)));

        return workflowContext;
    }
}