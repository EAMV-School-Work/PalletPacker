namespace PalletPacker.Application.Imports;

public sealed class ElementImportDto
{
    public required string Id { get; init; }
    public required decimal WeightKg { get; init; }
    public required int HeightMm { get; init; }
    public required int LengthMm { get; init; }
    public required int WidthMm { get; init; }
    public required string Series { get; init; }
    public required string PalletType { get; init; }
    public required bool CanRotate { get; init; }
    public required int MaxOtherElementsOnPallet { get; init; }
}