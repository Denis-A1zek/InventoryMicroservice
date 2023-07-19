using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;
using Play.Catalog.Service.Extensions;
using Play.Common.Interfaces;
using MassTransit;
using Play.Catalog.Contracts;

namespace Play.Catalog.Service.Controllers;

[ApiController]
[Route("items")]
public class ItemsController : ControllerBase
{
    private readonly IRepository<Item> _itemsRepository;
    private readonly IPublishEndpoint _publishEndpoint; 

    public ItemsController(IRepository<Item> itemsRepository, IPublishEndpoint publishEndpoint)
        => (_itemsRepository, _publishEndpoint) = (itemsRepository, publishEndpoint);

    [HttpGet]
    public async Task<IEnumerable<ItemDto>> GetAllAsync()
    {
        var items = (await _itemsRepository.GetAllAsync())
            .Select(item => item.AsDto());
        return items;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
    {
        var item = await _itemsRepository.GetItemAsync(id);
        if (item is null) return NotFound();
        return Ok(item.AsDto());
    }

    [HttpPost]
    public async Task<ActionResult<ItemDto>> PostAsync(CreateItemDto createItemDto)
    {
        if (createItemDto is null)
            return BadRequest();

        var item = new Item()
        {
            Name = createItemDto.Name,
            Description = createItemDto.Description,
            Price = createItemDto.Price,
            CreatedDate = DateTimeOffset.UtcNow
        };
        await _itemsRepository.CreateAsync(item);

        await _publishEndpoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description));

        return CreatedAtAction(nameof(GetByIdAsync), new { id = item.Id }, item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutAsync(Guid id, UpdateItemDto updateItemDto)
    {
        var exsistingItem = await _itemsRepository.GetItemAsync(id);
        if (exsistingItem is null) return NotFound();

        exsistingItem.Name = updateItemDto.Name;
        exsistingItem.Description = updateItemDto.Description;
        exsistingItem.Price = updateItemDto.Price;

        await _itemsRepository.UpdateAsync(exsistingItem);

        await _publishEndpoint.Publish(new CatalogItemUpdated(exsistingItem.Id, exsistingItem.Name, exsistingItem.Description));

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var exsistingItem = await _itemsRepository.GetItemAsync(id);
        if (exsistingItem is null) return NotFound();

        await _itemsRepository.RemoveAsync(exsistingItem.Id);

        await _publishEndpoint.Publish(new CatalogItemDeleted(id));

        return NoContent();
    }
}