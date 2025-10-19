using System;

namespace InventorySystem.Models
{
    // En ordre med én varelinje (Item + Quantity).
    public class Order
    {
        // Varen der bestilles
        public Item Item { get; }

        // Antal (kan være stk eller kg afhængigt af varen)
        public double Quantity { get; }

        // Sæt altid Item og Quantity via konstruktøren (løser CS8618 + "0 parameter(s)"-fejlen)
        public Order(Item item, double quantity)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
            Quantity = quantity;
        }

        // Hjælpe-egenskab: totalpris for ordren
        public double TotalPrice => Item.PricePerUnit * Quantity;

        public override string ToString() => $"{Item.Name} x {Quantity}";
    }
}