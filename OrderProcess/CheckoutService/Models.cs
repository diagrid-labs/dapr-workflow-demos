namespace CheckoutService.Models
{
    public record OrderItem(string Name, int Quantity = 1);
    public record CheckoutResult(bool Processed);
    public record InventoryItem(int ProductId, string Name, double PerItemCost, int Quantity);
    public record InventoryRequest(string RequestId, string ItemName, int Quantity);
    public record InventoryResult(bool InStock, InventoryItem? OrderPayload, double TotalCost);
    public record PaymentRequest(string RequestId, string Name, double TotalCost);
    public record PaymentResponse(string RequestId, bool IsPaymentSuccess);
    public record Notification(string Message);
}