using Avalonia.Controls;                 // for Window
using InventorySystem.ViewModels;        // for MainWindowViewModel

namespace InventorySystem;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();                          // bygger XAML
        DataContext = new MainWindowViewModel();        // binder ViewModel til vinduet
    }
}

