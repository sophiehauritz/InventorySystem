namespace InventorySystem.Models
{
    // Klasse for varer der måles i vægt eller volumen (f.eks. kg, liter)
    public class BulkItem : Item
    {
        // Måleenhed (fx "kg", "liter")
        public string MeasurementUnit { get; set; }

        // Constructor som bruger base-klassen (Item)
        public BulkItem(string name, double pricePerUnit, string measurementUnit)
            : base(name, pricePerUnit)
        {
            MeasurementUnit = measurementUnit;
        }

        // Overskriver ToString for at inkludere måleenheden
        public override string ToString()
        {
            return $"{Name} ({PricePerUnit} DKK/{MeasurementUnit})";
        }
    }
}