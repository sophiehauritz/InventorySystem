using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Threading.Tasks;               // for async/await

using InventorySystem.Models;              // Inventory, OrderBook, Item, UnitItem, Robot

namespace InventorySystem.ViewModels
{
    // -----------------------------
    // Simple ICommand for buttons
    // -----------------------------
    public sealed class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object? parameter) => _execute(parameter);

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    // -----------------------------
    // ViewModel for MainWindow
    // -----------------------------
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        // Domain objects
        private readonly Inventory _inventory;
        private readonly OrderBook _orderBook;

        // Robot interface
        private readonly Robot _robot;

        // Catalog for ComboBox
        public ObservableCollection<Item> CatalogItems { get; } = new();

        private Item? _selectedItem;
        public Item? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (!Equals(_selectedItem, value))
                {
                    _selectedItem = value;
                    OnPropertyChanged();
                    UpdateButtons();
                }
            }
        }

        private double _newQuantity = 1;
        public double NewQuantity
        {
            get => _newQuantity;
            set
            {
                if (Math.Abs(_newQuantity - value) > double.Epsilon)
                {
                    _newQuantity = value;
                    OnPropertyChanged();
                    UpdateButtons();
                }
            }
        }

        // Queues shown in DataGrids
        public ObservableCollection<Order> QueuedOrders { get; } = new();
        public ObservableCollection<Order> ProcessedOrders { get; } = new();

        // Revenue label
        private double _totalRevenue;
        public double TotalRevenue
        {
            get => _totalRevenue;
            private set
            {
                if (Math.Abs(_totalRevenue - value) > double.Epsilon)
                {
                    _totalRevenue = value;
                    OnPropertyChanged();
                }
            }
        }

        // Buttons
        public RelayCommand AddOrderCommand { get; }
        public RelayCommand ProcessNextCommand { get; }

        public MainWindowViewModel()
        {
            _inventory = new Inventory();
            _orderBook = new OrderBook();

            // ---- Robot config ----
            _robot = new Robot
            {
                // Use 127.0.0.1 if you reach URSim via Docker port mapping (recommended).
                // Use 172.17.0.2 only if you connect straight to the container’s IP.
                IpAddress = "127.0.0.1"
            };

            // --- Catalog & initial stock ---
            var hydraulicPump = new UnitItem("hydraulic pump", 8500, weight: 0);
            var plcModule     = new UnitItem("PLC module",     1200, weight: 0);
            var servoMotor    = new UnitItem("servo motor",    4300, weight: 0);

            CatalogItems.Clear();
            CatalogItems.Add(hydraulicPump);
            CatalogItems.Add(plcModule);
            CatalogItems.Add(servoMotor);

            _inventory.AddCatalogItem(hydraulicPump, initialAmount: 5);
            _inventory.AddCatalogItem(plcModule,     initialAmount: 10);
            _inventory.AddCatalogItem(servoMotor,    initialAmount: 3);

            // Commands
            AddOrderCommand    = new RelayCommand(_ => AddOrder(),    _ => CanAddOrder());
            ProcessNextCommand = new RelayCommand(_ => ProcessNext(), _ => CanProcessNext());

            RefreshLists();
            UpdateButtons();
        }

        // -------- Helpers --------
        private void RefreshLists()
        {
            QueuedOrders.Clear();
            foreach (var o in _orderBook.QueuedOrders)
                QueuedOrders.Add(o);

            ProcessedOrders.Clear();
            foreach (var o in _orderBook.ProcessedOrders)
                ProcessedOrders.Add(o);
        }

        private void UpdateButtons()
        {
            AddOrderCommand.RaiseCanExecuteChanged();
            ProcessNextCommand.RaiseCanExecuteChanged();
        }

        // -------- Add order --------
        private bool CanAddOrder() => SelectedItem != null && NewQuantity > 0;

        private void AddOrder()
        {
            if (SelectedItem is null || NewQuantity <= 0) return;

            var order = new Order(SelectedItem, NewQuantity);
            _orderBook.QueueOrder(order);

            NewQuantity = 1;
            RefreshLists();
            UpdateButtons();
        }

        // -------- Process next order (robot hook calls Robot.RunSequence) --------
        private bool CanProcessNext() => _orderBook.QueuedOrders.Count > 0;

        private async void ProcessNext()
        {
            if (_orderBook.QueuedOrders.Count == 0) return;

            // Peek before processing (optional: use for UI feedback)
            var next = _orderBook.QueuedOrders.Peek();

            // Try to process it (this will dequeue internally if successful)
            bool processed = _orderBook.ProcessNextOrder(_inventory);
            if (!processed) return;

            // --- Trigger robot. This wraps your working URScript internally. ---
            try
            {
                // Run on a background thread so the UI doesn't block
                await Task.Run(() => _robot.RunSequence());
            }
            catch
            {
                // Swallow robot errors for demo stability
            }

            // Update revenue and lists
            TotalRevenue = _orderBook.TotalRevenue;
            RefreshLists();
            UpdateButtons();
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
