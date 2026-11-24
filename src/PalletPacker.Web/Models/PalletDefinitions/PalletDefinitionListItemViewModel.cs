namespace PalletPacker.Web.Models.PalletDefinitions;

public class PalletDefinitionListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int LengthMm { get; set; }
    public int WidthMm { get; set; }
    public int HeightMm { get; set; }
    public decimal OwnWeightKg { get; set; }
    public decimal MaxWeightKg { get; set; }
    public int MaxHeightMm { get; set; }
    public bool IsActive { get; set; }
    public bool IsArchived { get; set; }
}