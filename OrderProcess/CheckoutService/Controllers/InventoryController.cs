using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using CheckoutService.Models;

namespace CheckoutService.Controllers;

[ApiController]
[Route("[controller]")]
public class InventoryController(ILogger<InventoryController> logger, DaprClient client) : ControllerBase
{
    private static readonly string storeName = "statestore";
    private readonly static string[] itemKeys = new [] {"Paperclips", "Cars", "Computers"};

    [HttpGet]
    public async Task<IActionResult> GetInventory()
    {
        var inventory = new List<InventoryItem>();

        foreach (var itemKey in itemKeys)
        {
             var item = await client.GetStateAsync<InventoryItem>(storeName, itemKey.ToLowerInvariant());
             inventory.Add(item);
        }

        return new OkObjectResult(inventory);
    }

    [HttpPost("restock")]
    public async void RestockInventory()
    {
        var baseInventory = new List<InventoryItem>
        {
            new InventoryItem(ProductId: 1, Name: itemKeys[0], PerItemCost: 5, Quantity: 100),
            new InventoryItem(ProductId: 2, Name: itemKeys[1], PerItemCost: 15000, Quantity: 100),
            new InventoryItem(ProductId: 3, Name: itemKeys[2], PerItemCost: 500, Quantity: 100),
        };

        foreach (var item in baseInventory)
        {
            await client.SaveStateAsync(storeName, item.Name.ToLowerInvariant(), item);
        }

        logger.LogInformation("Inventory Restocked!");
    }

    [HttpDelete]
    public async void ClearInventory()
    {
        var baseInventory = new List<InventoryItem>
        {
            new InventoryItem(ProductId: 1, Name: itemKeys[0], PerItemCost: 5, Quantity: 0),
            new InventoryItem(ProductId: 2, Name: itemKeys[1], PerItemCost: 15000, Quantity: 0),
            new InventoryItem(ProductId: 3, Name: itemKeys[2], PerItemCost: 500, Quantity: 0),
        };

        foreach (var item in baseInventory)
        {
            await client.SaveStateAsync(storeName, item.Name.ToLowerInvariant(), item);
        }

        logger.LogInformation("Cleared inventory!");
    }
}
