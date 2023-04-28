using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using WorkflowSample.Models;

namespace WorkflowSample.Controllers;

[ApiController]
[Route("[controller]")]
public class StockController : ControllerBase
{
    private readonly ILogger<StockController> _logger;
    private readonly DaprClient _client;
    static readonly string storeName = "statestore";

    public StockController(ILogger<StockController> logger, DaprClient client)
    {
        _logger = logger;
        _client = client;
    }

    [HttpGet("restock")]
    public async void RestockInventory()
    {
        var baseInventory = new List<InventoryItem>
        {
            new InventoryItem(Name: "Paperclips", PerItemCost: 5, Quantity: 100),
            new InventoryItem(Name: "Cars", PerItemCost: 15000, Quantity: 100),
            new InventoryItem(Name: "Computers", PerItemCost: 500, Quantity: 100),
        };

        foreach (var item in baseInventory)
        {
            await this._client.SaveStateAsync(storeName, item.Name.ToLowerInvariant(), item);
        }

        _logger.LogInformation("Inventory Restocked!");
    }
}
