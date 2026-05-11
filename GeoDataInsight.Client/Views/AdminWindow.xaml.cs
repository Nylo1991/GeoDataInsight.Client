using System.Windows;
using GeoDataInsight.Client.ViewModels;

namespace GeoDataInsight.Client.Views
{
    public partial class AdminWindow : Window
    {
        public AdminWindow()
        {
            InitializeComponent();
            // 👇 ESSA LINHA É OBRIGATÓRIA PARA OS DADOS APARECEREM 👇
            this.DataContext = new AdminViewModel();
        }
    }
}