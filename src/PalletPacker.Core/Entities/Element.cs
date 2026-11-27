namespace PalletPacker.Core.Entities;

public class Element
{
    public string Id { get; set; } = "";

    public decimal WeightKg { get; set; }
    public int HeightMm { get; set; }
    public int LengthMm { get; set; }
    public int WidthMm { get; set; }

    public string Series { get; set; } = "";
    public string PalletType { get; set; } = "";
    public bool CanRotate { get; set; }
    public int MaxOtherElementsOnPallet { get; set; }
}