namespace InventorySystem.Models
{
    // Klasse der repræsenterer én varelinje i en ordre
    public class OrderLine
    {
        // Referencerer varen (kan være UnitItem eller BulkItem)
        public Item Item { get; set; }

        // Antal af varen der bestilles
        public double Quantity { get; set; }

        // Constructor
        public OrderLine(Item item, double quantity)
        {
            Item = item;
            Quantity = quantity;
        }

        // Beregner totalpris for denne ordrelinje
        public double TotalPrice()
        {
            return Item.PricePerUnit * Quantity;
        }

        // Returnerer en tekst med information om linjen
        public override string ToString()
        {
            return $"{Item.Name} x {Quantity} → {TotalPrice()} DKK";
        }
    }
}