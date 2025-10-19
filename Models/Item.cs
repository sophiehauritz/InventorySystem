namespace InventorySystem.Models
{
    // Grundlæggende klasse for et produkt i lageret
    public class Item
    {
        // Navnet på varen
        public string Name { get; set; }

        // Pris pr. enhed (fx pr. stk. eller pr. kg)
        public double PricePerUnit { get; set; }

        // Constructor til at oprette en vare
        public Item(string name, double pricePerUnit)
        {
            Name = name;
            PricePerUnit = pricePerUnit;
        }

        // Returnerer en tekst med varens oplysninger
        public override string ToString()
        {
            return $"{Name} ({PricePerUnit} DKK/unit)";
        }
    }
}