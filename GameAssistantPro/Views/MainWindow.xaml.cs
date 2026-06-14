using System.Windows;
using GameAssistantPro.ViewModels;

namespace GameAssistantPro.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
