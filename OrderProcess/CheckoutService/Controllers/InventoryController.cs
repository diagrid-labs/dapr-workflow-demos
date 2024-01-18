using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using CheckoutService.Models;

namespace CheckoutService.Controllers;

[ApiController]
[Route("[controller]")]
public class InventoryController : ControllerBase
{
    private readonly ILogger<InventoryController> _logger;
    private readonly DaprClient _client;
    private static readonly string storeName = "statestore";
    private readonly static string[] itemKeys = new [] {"Paperclips", "Cars", "Computers"};

    public InventoryController(ILogger<InventoryController> logger, DaprClient client)
    {
        _logger = logger;
        _client = client;
    }

    [HttpGet]
    public async Task<IActionResult> GetInventory()
    {
        var inventory = new List<InventoryItem>();

        foreach (var itemKey in itemKeys)
        {
             var item = await _client.GetStateAsync<InventoryItem>(storeName, itemKey.ToLowerInvariant());
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
            await _client.SaveStateAsync(storeName, item.Name.ToLowerInvariant(), item);
        }

        _logger.LogInformation("Inventory Restocked!");
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
            await _client.SaveStateAsync(storeName, item.Name.ToLowerInvariant(), item);
        }

        _logger.LogInformation("Cleared inventory!");
    }
}
