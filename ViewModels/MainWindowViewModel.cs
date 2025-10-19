using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Linq;

using InventorySystem.Models;

namespace InventorySystem.ViewModels
{
    // ----- Simpel ICommand-implementering til knapper -----
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

    // ----- ViewModel til MainWindow -----
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        // ----- Felter -----
        private readonly Inventory _inventory;
        private readonly OrderBook _orderBook;

        // ----- Katalog + valgt vare + mængde -----
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

        // ----- Lister til DataGrids -----
        public ObservableCollection<Order> QueuedOrders { get; } = new();
        public ObservableCollection<Order> ProcessedOrders { get; } = new();

        // ----- Indtægt i alt -----
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

        // ----- Commands til knapper -----
        public RelayCommand AddOrderCommand { get; }
        public RelayCommand ProcessNextCommand { get; }

        // ----- Constructor -----
        public MainWindowViewModel()
        {
            _inventory = new Inventory();
            _orderBook = new OrderBook();

            // --- KATALOG & STARTLAGER ---
            var hydraulicPump = new UnitItem(name: "hydraulic pump", pricePerUnit: 8500, weight: 0);
            var plcModule     = new UnitItem(name: "PLC module",     pricePerUnit: 1200, weight: 0);
            var servoMotor    = new UnitItem(name: "servo motor",    pricePerUnit: 4300, weight: 0);

            // Læg i katalog (dropdown viser disse)
            CatalogItems.Clear();
            CatalogItems.Add(hydraulicPump);
            CatalogItems.Add(plcModule);
            CatalogItems.Add(servoMotor);

            // Startlager
            _inventory.AddCatalogItem(hydraulicPump, initialAmount: 5);
            _inventory.AddCatalogItem(plcModule,     initialAmount: 10);
            _inventory.AddCatalogItem(servoMotor,    initialAmount: 3);

            // Commands
            AddOrderCommand    = new RelayCommand(_ => AddOrder(), _ => CanAddOrder());
            ProcessNextCommand = new RelayCommand(_ => ProcessNext(), _ => CanProcessNext());

            RefreshLists();
            UpdateButtons();
        }

        // ----- UI-hjælpere -----
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

        // ----- Tilføj ordre -----
        private bool CanAddOrder() => SelectedItem != null && NewQuantity > 0;

        private void AddOrder()
        {
            if (SelectedItem is null || NewQuantity <= 0) return;

            var order = new Order(SelectedItem, NewQuantity);
            _orderBook.QueueOrder(order);

            // nulstil mængde og opdater visning
            NewQuantity = 1;
            RefreshLists();
            UpdateButtons();
        }

        // ----- Processér næste ordre -----
        private bool CanProcessNext() => _orderBook.QueuedOrders.Count > 0;

        private void ProcessNext()
        {
            // ProcessNextOrder returnerer bool
            bool processed = _orderBook.ProcessNextOrder(_inventory);
            if (!processed) return;

            // Opdatér indtægt + visning
            TotalRevenue = _orderBook.TotalRevenue;
            RefreshLists();
            UpdateButtons();
        }

        // ----- INotifyPropertyChanged -----
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
