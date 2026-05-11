using System.Windows;
using GeoDataInsight.Client.ViewModels;

namespace GeoDataInsight.Client.Views
{
    public partial class AdminWindow : Window
    {
        public AdminWindow()
        {
            InitializeComponent();

            // Isso conecta a View ao ViewModel! Sem isso, a tela fica vazia.
            this.DataContext = new AdminViewModel();
        }
    }
}