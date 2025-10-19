namespace InventorySystem.Models
{
    // Klasse for varer, der sælges i styk (fx "1 stk", "5 stk")
    public class UnitItem : Item
    {
        // Vægten af varen (valgfrit felt, bruges kun hvis relevant)
        public double Weight { get; set; }

        // Constructor som bruger base-klassen (Item)
        public UnitItem(string name, double pricePerUnit, double weight)
            : base(name, pricePerUnit)
        {
            Weight = weight;
        }

        // Overskriver ToString for at inkludere vægt
        public override string ToString()
        {
            return $"{Name} ({PricePerUnit} DKK/stk, {Weight} kg)";
        }
    }
}