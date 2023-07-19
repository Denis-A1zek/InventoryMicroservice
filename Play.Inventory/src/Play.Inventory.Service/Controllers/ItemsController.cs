using Microsoft.AspNetCore.Mvc;
using Play.Common.Interfaces;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;
using Play.Inventory.Service.Extensions;

namespace Play.Inventory.Service.Controllers;

[ApiController]
[Route("items")]
public class ItemsController : ControllerBase
{
    private readonly IRepository<InventoryItem> _inventoryItemRepository;
    private readonly IRepository<CatalogItem> _catalogItemRepository;

    public ItemsController(IRepository<InventoryItem> inventoryItemRepository, IRepository<CatalogItem> catalogItems)
        => (_inventoryItemRepository, _catalogItemRepository) = (inventoryItemRepository, catalogItems);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
    {
        if(userId == Guid.Empty) return BadRequest();

        var inventoryItemEntities = await _inventoryItemRepository
                .GetAllAsync(item => item.UserId == userId);
        var itemIds = inventoryItemEntities.Select(item => item.CatalogItemId);
        var catalogItemEntities = await _catalogItemRepository.GetAllAsync(item => itemIds.Contains(item.Id));

        var inventoryItemDto = inventoryItemEntities.Select(inventoryItem => 
        {
            var catalogItem = catalogItemEntities.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);
            return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
        });

        return Ok(inventoryItemDto);   
    }

    [HttpPost]
    public async Task<ActionResult> PostAsync(GrantItemsDto grantItemDto)
    {
        var inventoryItem = await _inventoryItemRepository
                    .GetItemAsync(item => item.UserId == grantItemDto.UserId 
                    && item.CatalogItemId == grantItemDto.CatalogItemId);
        
        if(inventoryItem is null)
        {
            inventoryItem = new InventoryItem() 
            { 
                CatalogItemId=grantItemDto.CatalogItemId,
                UserId= grantItemDto.UserId,
                Quantity = grantItemDto.Quantity,
                AcquiredDate = DateTimeOffset.UtcNow
            };

            await _inventoryItemRepository.CreateAsync(inventoryItem);
        }
        else
        {
            inventoryItem.Quantity += grantItemDto.Quantity;
            await _inventoryItemRepository.UpdateAsync(inventoryItem);
        }

        return Ok();
    }
}