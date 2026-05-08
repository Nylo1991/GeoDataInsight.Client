using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Linq;
using GeoDataInsight.Client.Models;
using GeoDataInsight.Client.Helpers;
using GeoDataInsight.Client.Services;

namespace GeoDataInsight.Client.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _termoBusca = string.Empty;
        private LocationModel _selecionado;
        private bool _isSugestoesAberto;
        private readonly MapService _mapService;

        public MainViewModel()
        {
            _mapService = new MapService();
            BuscarCommand = new RelayCommand(async (obj) => await ExecutarBusca());

            // Comando para quando o usuário clicar em um item do histórico
            SelecionarHistoricoCommand = new RelayCommand((obj) =>
            {
                if (obj is string termo)
                {
                    TermoBusca = termo;
                    IsSugestoesAberto = false;
                    _ = ExecutarBusca(); // Dispara a busca
                }
            });
        }

        public ICommand BuscarCommand { get; }
        public ICommand SelecionarHistoricoCommand { get; }

        public string TermoBusca
        {
            get => _termoBusca;
            set
            {
                _termoBusca = value;
                OnPropertyChanged();

                // Abre as sugestões se o campo não estiver vazio e houver histórico
                if (HistoricoPesquisas.Any())
                    IsSugestoesAberto = true;
            }
        }

        public bool IsSugestoesAberto
        {
            get => _isSugestoesAberto;
            set { _isSugestoesAberto = value; OnPropertyChanged(); }
        }

        public ObservableCollection<LocationModel> Resultados { get; set; } = new ObservableCollection<LocationModel>();

        // Guarda os termos já pesquisados
        public ObservableCollection<string> HistoricoPesquisas { get; set; } = new ObservableCollection<string>();

        public LocationModel Selecionado
        {
            get => _selecionado;
            set { _selecionado = value; OnPropertyChanged(); }
        }

        private async Task ExecutarBusca()
        {
            if (string.IsNullOrWhiteSpace(TermoBusca)) return;

            IsSugestoesAberto = false; // Fecha o popup
            Resultados.Clear();

            // Adiciona ao histórico se não existir (limita aos 5 últimos)
            if (!HistoricoPesquisas.Contains(TermoBusca))
            {
                HistoricoPesquisas.Insert(0, TermoBusca);
                if (HistoricoPesquisas.Count > 5) HistoricoPesquisas.RemoveAt(5);
            }

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