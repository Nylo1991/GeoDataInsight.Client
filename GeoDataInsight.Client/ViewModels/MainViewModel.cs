using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using GeoDataInsight.Client.Models;

namespace GeoDataInsight.Client.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // Propriedades Públicas para a Tela enxergar
        private string _termoBusca = "";
        public string TermoBusca { get => _termoBusca; set { _termoBusca = value; OnPropertyChanged(); } }

        private string _statusMensagem = "Pronto";
        public string StatusMensagem { get => _statusMensagem; set { _statusMensagem = value; OnPropertyChanged(); } }

        public ObservableCollection<LocationModel> Resultados { get; set; } = new ObservableCollection<LocationModel>();

        private LocationModel? _selecionado;
        public LocationModel? Selecionado { get => _selecionado; set { _selecionado = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null!) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}