using MassTransit;
using Play.Catalog.Contracts;
using Play.Common.Interfaces;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Consumers;

public class CatalogItemUpdatedConsumer : IConsumer<CatalogItemUpdated>
{
    private readonly IRepository<CatalogItem> _repository;

    public CatalogItemUpdatedConsumer(IRepository<CatalogItem> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<CatalogItemUpdated> context)
    {
        var message = context.Message;

        var item = await _repository.GetItemAsync(message.ItemId);

        if(item is null)
        {
            item = new CatalogItem()
                    {
                        Id = message.ItemId,
                        Name = message.Name,
                        Description = message.Description
                    };

            await _repository.CreateAsync(item);
            return;
        }

        item.Name = message.Name;
        item.Description = message.Description;

        await _repository.UpdateAsync(item);       
    }
}
