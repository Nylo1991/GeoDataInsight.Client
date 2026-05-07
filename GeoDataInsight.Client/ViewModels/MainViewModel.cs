using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using GeoDataInsight.Client.Models;

namespace GeoDataInsight.Client.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _termoBusca = string.Empty;
        private string _statusMensagem = "Pronto";
        private LocationModel? _selecionado;

        public string TermoBusca
        {
            get => _termoBusca;
            set { _termoBusca = value; OnPropertyChanged(); }
        }

        public string StatusMensagem
        {
            get => _statusMensagem;
            set { _statusMensagem = value; OnPropertyChanged(); }
        }

        // Lista que o MapService vai preencher
        public ObservableCollection<LocationModel> Resultados { get; set; } = new ObservableCollection<LocationModel>();

        // O item que o usuário seleciona na lista para ver os detalhes
        public LocationModel? Selecionado
        {
            get => _selecionado;
            set { _selecionado = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}