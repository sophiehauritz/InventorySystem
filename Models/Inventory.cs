using System.Collections.Generic;
using System.Linq;

namespace InventorySystem.Models
{
    // Holder katalog over varer + mængder på lager
    public class Inventory
    {
        // Navn -> Item (katalog)
        public Dictionary<string, Item> Catalog { get; } = new();

        // Navn -> antal (kan være stk eller kg)
        public Dictionary<string, double> Stock { get; } = new();

        // Tilføj vare til katalog + initial lager
        public void AddCatalogItem(Item item, double initialAmount = 0)
        {
            Catalog[item.Name] = item;
            Stock[item.Name] = initialAmount;
        }

        // Læg i lager
        public void AddStock(string itemName, double amount)
        {
            if (!Stock.ContainsKey(itemName)) Stock[itemName] = 0;
            Stock[itemName] += amount;
        }

        // Træk fra lager (returnerer false hvis ikke nok)
        public bool RemoveStock(string itemName, double amount)
        {
            if (!Stock.ContainsKey(itemName) || Stock[itemName] < amount) return false;
            Stock[itemName] -= amount;
            return true;
        }

        // Find varer med lav beholdning
        public List<Item> LowStockItems(double threshold = 5)
            => Stock.Where(kv => kv.Value < threshold)
                .Select(kv => Catalog.TryGetValue(kv.Key, out var it) ? it : null)
                .Where(it => it != null)!
                .ToList()!;
    }
}