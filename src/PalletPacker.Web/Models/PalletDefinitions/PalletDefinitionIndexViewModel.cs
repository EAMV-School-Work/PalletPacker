namespace PalletPacker.Web.Models.PalletDefinitions;

public class PalletDefinitionIndexViewModel
{
    public bool ShowArchived { get; set; }
    public List<PalletDefinitionListItemViewModel> Definitions { get; set; } = new();
}