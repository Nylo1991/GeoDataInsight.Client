using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using GeoDataInsight.Client.Models;
using GeoDataInsight.Client.Services;
using GeoDataInsight.Client.Helpers;

namespace GeoDataInsight.Client.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly MapService _mapService = new MapService();
        private readonly FirebaseService _firebaseService = new FirebaseService();

        private string _termoBusca;
        public string TermoBusca
        {
            get => _termoBusca;
            set { _termoBusca = value; OnPropertyChanged(); }
        }

        private LocationModel _resultadoAtual;
        public LocationModel ResultadoAtual
        {
            get => _resultadoAtual;
            set { _resultadoAtual = value; OnPropertyChanged(); }
        }

        private string _statusMensagem = "Pronto";
        public string StatusMensagem
        {
            get => _statusMensagem;
            set { _statusMensagem = value; OnPropertyChanged(); }
        }

        private string _mapImageUrl;
        public string MapImageUrl
        {
            get => _mapImageUrl;
            set { _mapImageUrl = value; OnPropertyChanged(); }
        }

        public ICommand BuscarCommand { get; }

        public MainViewModel()
        {
            BuscarCommand = new RelayCommand(async (param) => await BuscarLocal());
        }

        private async Task BuscarLocal()
        {
            if (string.IsNullOrWhiteSpace(TermoBusca)) return;

            StatusMensagem = "Buscando...";
            var resultado = await _mapService.BuscarLocalAsync(TermoBusca);

            if (resultado != null)
            {
                ResultadoAtual = resultado;

                string latStr = resultado.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                string lonStr = resultado.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);

                MapImageUrl = $"https://staticmap.openstreetmap.de/staticmap.php?center={latStr},{lonStr}&zoom=15&size=400x300&markers={latStr},{lonStr},red-pushpin";

                StatusMensagem = "Local encontrado! Salvando no banco...";

                await _firebaseService.SalvarNoHistoricoAsync(resultado);

                StatusMensagem = "Local salvo com sucesso no Firebase!";
            }
            else
            {
                StatusMensagem = "Local não encontrado ou erro na API.";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}