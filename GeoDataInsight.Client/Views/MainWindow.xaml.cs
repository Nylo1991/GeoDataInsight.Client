using System.Windows;
using GeoDataInsight.Client.ViewModels;

namespace GeoDataInsight.Client.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }
    }
}