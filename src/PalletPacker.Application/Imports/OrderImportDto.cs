namespace PalletPacker.Application.Imports;

public sealed class OrderImportDto
{
    public required string Id { get; init; }
    public required List<string> ElementIds { get; init; }
}