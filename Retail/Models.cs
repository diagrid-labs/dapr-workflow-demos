namespace WorkflowSample.Models
{
    public record OrderPayload(string Name, int Quantity = 1);
    public record InventoryRequest(string RequestId, string ItemName, int Quantity);
    public record InventoryResult(bool Success, InventoryItem? OrderPayload, double TotalCost);
    public record PaymentRequest(string RequestId, string ItemName, int Amount, double Currency);
    public record OrderResult(bool Processed);
    public record InventoryItem(string Name, double PerItemCost, int Quantity);
}