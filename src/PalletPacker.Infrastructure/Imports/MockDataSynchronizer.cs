using Microsoft.EntityFrameworkCore;
using PalletPacker.Application.Imports;
using PalletPacker.Core.Entities;
using PalletPacker.Infrastructure.Data;

namespace PalletPacker.Infrastructure.Imports;

public sealed class MockDataSynchronizer
{
    private readonly AppDbContext _dbContext;
    private readonly MockDataJsonLoader _jsonLoader;

    public MockDataSynchronizer(
        AppDbContext dbContext,
        MockDataJsonLoader jsonLoader)
    {
        _dbContext = dbContext;
        _jsonLoader = jsonLoader;
    }

    public async Task<bool> SynchronizeAsync(
        CancellationToken cancellationToken = default)
    {
        var source = await _jsonLoader.LoadAsync(cancellationToken);

        var existingElements = await _dbContext.Elements
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var existingOrders = await _dbContext.Orders
            .Include(order => order.Elements)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (Matches(
                source.Elements,
                source.Orders,
                existingElements,
                existingOrders))
        {
            return false;
        }

        var elements = source.Elements
            .Select(item => new Element
            {
                Id = item.Id,
                WeightKg = item.WeightKg,
                HeightMm = item.HeightMm,
                LengthMm = item.LengthMm,
                WidthMm = item.WidthMm,
                Series = item.Series,
                PalletType = item.PalletType,
                CanRotate = item.CanRotate,
                MaxOtherElementsOnPallet = item.MaxOtherElementsOnPallet
            })
            .ToList();

        var elementsById = elements.ToDictionary(
            element => element.Id,
            StringComparer.OrdinalIgnoreCase);

        var orders = source.Orders
            .Select(item => new Order
            {
                Id = item.Id,
                Elements = item.ElementIds
                    .Select(elementId => elementsById[elementId])
                    .ToList()
            })
            .ToList();

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        await _dbContext.Elements.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Orders.ExecuteDeleteAsync(cancellationToken);

        _dbContext.Elements.AddRange(elements);
        _dbContext.Orders.AddRange(orders);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return true;
    }

    private static bool Matches(
        List<ElementImportDto> sourceElements,
        List<OrderImportDto> sourceOrders,
        List<Element> existingElements,
        List<Order> existingOrders)
    {
        if (sourceElements.Count != existingElements.Count ||
            sourceOrders.Count != existingOrders.Count)
        {
            return false;
        }

        var elementsById = existingElements.ToDictionary(
            element => element.Id,
            StringComparer.OrdinalIgnoreCase);

        foreach (var source in sourceElements)
        {
            if (!elementsById.TryGetValue(source.Id, out var existing))
            {
                return false;
            }

            if (!string.Equals(existing.Id, source.Id, StringComparison.Ordinal) ||
                existing.WeightKg != source.WeightKg ||
                existing.HeightMm != source.HeightMm ||
                existing.LengthMm != source.LengthMm ||
                existing.WidthMm != source.WidthMm ||
                !string.Equals(existing.Series, source.Series, StringComparison.Ordinal) ||
                !string.Equals(existing.PalletType, source.PalletType, StringComparison.Ordinal) ||
                existing.CanRotate != source.CanRotate ||
                existing.MaxOtherElementsOnPallet != source.MaxOtherElementsOnPallet)
            {
                return false;
            }
        }

        var ordersById = existingOrders.ToDictionary(
            order => order.Id,
            StringComparer.OrdinalIgnoreCase);

        foreach (var source in sourceOrders)
        {
            if (!ordersById.TryGetValue(source.Id, out var existing))
            {
                return false;
            }

            if (!string.Equals(existing.Id, source.Id, StringComparison.Ordinal))
            {
                return false;
            }

            var sourceElementIds = source.ElementIds.ToHashSet(
                StringComparer.OrdinalIgnoreCase);

            var existingElementIds = existing.Elements
                .Select(element => element.Id)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!sourceElementIds.SetEquals(existingElementIds))
            {
                return false;
            }
        }

        return true;
    }
}