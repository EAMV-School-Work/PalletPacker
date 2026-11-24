using System.ComponentModel.DataAnnotations;

namespace PalletPacker.Web.Models.PalletDefinitions;

public class PalletDefinitionFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Angiv et navn.")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Angiv en palletype.")]
    public string PalletType { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "Målet skal være mindst 1 mm.")]
    public int LengthMm { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Målet skal være mindst 1 mm.")]
    public int WidthMm { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Målet skal være mindst 1 mm.")]
    public int HeightMm { get; set; }

    [Range(typeof(decimal), "0", "99999999.99",
        ErrorMessage = "Egenvægten skal være mellem 0 og 99.999.999,99 kg.")]
    public decimal OwnWeightKg { get; set; }

    [Range(typeof(decimal), "0.01", "99999999.99",
        ErrorMessage = "Maksimal vægt skal være større end 0 kg.")]
    public decimal MaxWeightKg { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Maks. højde skal være mindst 1 mm.")]
    public int MaxHeightMm { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Overmål må ikke være negativt.")]
    public int AllowedOverhangMm { get; set; }

    public bool IsActive { get; set; }
}