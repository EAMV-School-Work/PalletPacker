namespace PalletPacker.Core.Entities;

public class Order
{
    public string Id { get; set; } = "";

    public ICollection<Element> Elements { get; set; } = new List<Element>();
}