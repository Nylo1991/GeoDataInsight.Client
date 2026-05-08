using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Threading.Tasks;
using GeoDataInsight.Client.Models;
using GeoDataInsight.Client.Helpers;
using GeoDataInsight.Client.Services; // Certifique-se de que este using existe

namespace GeoDataInsight.Client.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _termoBusca = string.Empty;
        private LocationModel _selecionado;
        private readonly MapService _mapService; // Instância do serviço real

        public MainViewModel()
        {
            _mapService = new MapService();
            // Transformamos o comando em assíncrono para não travar a interface
            BuscarCommand = new RelayCommand(async (obj) => await ExecutarBusca());
        }

        public ICommand BuscarCommand { get; }

        public string TermoBusca
        {
            get => _termoBusca;
            set
            {
                _termoBusca = value;
                OnPropertyChanged();
                // Opcional: Poderia disparar uma pesquisa automática aqui para "sugestões"
            }
        }

        public ObservableCollection<LocationModel> Resultados { get; set; } = new ObservableCollection<LocationModel>();

        public LocationModel Selecionado
        {
            get => _selecionado;
            set
            {
                _selecionado = value;
                OnPropertyChanged();
            }
        }

        // Método agora é assíncrono e utiliza o serviço real
        private async Task ExecutarBusca()
        {
            if (string.IsNullOrWhiteSpace(TermoBusca)) return;

            Resultados.Clear();

            // Chama a API do OpenStreetMap através do seu MapService
            var locaisEncontrados = await _mapService.SearchLocationAsync(TermoBusca);

            foreach (var local in locaisEncontrados)
            {
                Resultados.Add(local);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}