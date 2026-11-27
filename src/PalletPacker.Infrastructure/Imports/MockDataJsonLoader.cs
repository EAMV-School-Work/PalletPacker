using System.Text.Json;
using PalletPacker.Application.Imports;

namespace PalletPacker.Infrastructure.Imports;

public sealed class MockDataJsonLoader
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public async Task<(
        List<ElementImportDto> Elements,
        List<OrderImportDto> Orders)> LoadAsync(
            CancellationToken cancellationToken = default)
    {
        var elementsPath = Path.Combine(
            AppContext.BaseDirectory,
            "mock-elements.json");

        var ordersPath = Path.Combine(
            AppContext.BaseDirectory,
            "mock-orders.json");

        var elementsJson = await File.ReadAllTextAsync(
            elementsPath,
            cancellationToken);

        var ordersJson = await File.ReadAllTextAsync(
            ordersPath,
            cancellationToken);

        var elements =
            JsonSerializer.Deserialize<List<ElementImportDto>>(
                elementsJson,
                JsonOptions)
            ?? throw new InvalidDataException(
                "mock-elements.json must contain a JSON array.");

        var orders =
            JsonSerializer.Deserialize<List<OrderImportDto>>(
                ordersJson,
                JsonOptions)
            ?? throw new InvalidDataException(
                "mock-orders.json must contain a JSON array.");

        ValidateData(elements, orders);
        return (elements, orders);
    }

    private static void ValidateData(
    List<ElementImportDto> elements,
    List<OrderImportDto> orders)
    {
        const int maxIdLength = 450;
        const decimal maxWeightKg = 99_999_999.99m;

        var elementIds = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var element in elements)
        {
            if (element is null ||
                string.IsNullOrWhiteSpace(element.Id) ||
                element.Id.Length > maxIdLength)
            {
                throw new InvalidDataException(
                    "Every element must have an ID of 1 to 450 characters.");
            }

            if (!elementIds.Add(element.Id))
            {
                throw new InvalidDataException(
                    $"Duplicate element ID: {element.Id}");
            }

            if (element.WeightKg <= 0 ||
                element.WeightKg > maxWeightKg ||
                decimal.Round(element.WeightKg, 2) != element.WeightKg)
            {
                throw new InvalidDataException(
                    $"Element {element.Id} must have a positive weight with at most two decimals.");
            }

            if (element.HeightMm <= 0 ||
                element.LengthMm <= 0 ||
                element.WidthMm <= 0)
            {
                throw new InvalidDataException(
                    $"Element {element.Id} must have positive dimensions in millimetres.");
            }

            if (element.MaxOtherElementsOnPallet < 0)
            {
                throw new InvalidDataException(
                    $"Element {element.Id} cannot have a negative maximum number of other elements.");
            }

            if (string.IsNullOrWhiteSpace(element.Series))
            {
                throw new InvalidDataException(
                    $"Element {element.Id} must have a series value.");
            }

            if (string.IsNullOrWhiteSpace(element.PalletType))
            {
                throw new InvalidDataException(
                    $"Element {element.Id} must have a pallet type.");
            }
        }

        var orderIds = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        var assignedElementIds = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var order in orders)
        {
            if (order is null ||
                string.IsNullOrWhiteSpace(order.Id) ||
                order.Id.Length > maxIdLength)
            {
                throw new InvalidDataException(
                    "Every order must have an ID of 1 to 450 characters.");
            }

            if (!orderIds.Add(order.Id))
            {
                throw new InvalidDataException(
                    $"Duplicate order ID: {order.Id}");
            }

            if (order.ElementIds is null)
            {
                throw new InvalidDataException(
                    $"Order {order.Id} must have an elementIds list.");
            }

            var referencesInOrder = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var elementId in order.ElementIds)
            {
                if (string.IsNullOrWhiteSpace(elementId))
                {
                    throw new InvalidDataException(
                        $"Order {order.Id} contains an empty element ID.");
                }

                if (!referencesInOrder.Add(elementId))
                {
                    throw new InvalidDataException(
                        $"Order {order.Id} references element {elementId} more than once.");
                }

                if (!elementIds.Contains(elementId))
                {
                    throw new InvalidDataException(
                        $"Order {order.Id} references unknown element {elementId}.");
                }

                if (!assignedElementIds.Add(elementId))
                {
                    throw new InvalidDataException(
                        $"Element {elementId} is referenced by more than one order.");
                }
            }
        }
    }
}