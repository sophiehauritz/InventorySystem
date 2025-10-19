using System.Collections.Generic;

namespace InventorySystem.Models
{
    // Styrer kø og bogføring
    public class OrderBook
    {
        public Queue<Order> QueuedOrders { get; } = new();
        public List<Order> ProcessedOrders { get; } = new();
        public double TotalRevenue { get; private set; }

        public void QueueOrder(Order order) => QueuedOrders.Enqueue(order);

        // Forsøger at processere næste ordre. True = lykkedes.
        public bool ProcessNextOrder(Inventory inventory)
        {
            if (QueuedOrders.Count == 0) return false;

            var order = QueuedOrders.Dequeue();

            var name = order.Item.Name;

            // Tjek at hele ordren kan leveres
            if (!inventory.Stock.TryGetValue(name, out var available) || available < order.Quantity)
                return false;

            // Bogfør lager & omsætning
            inventory.Stock[name] = available - order.Quantity;
            TotalRevenue += order.TotalPrice;

            // Flyt ordren til "processed"
            ProcessedOrders.Add(order);
            return true;
        }
    }
}