using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Linq;
using System.Windows;
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
        private readonly SearchHistoryService _historyService;

        // Comandos da Interface
        public ICommand BuscarCommand { get; }
        public ICommand SelecionarHistoricoCommand { get; }
        public ICommand SalvarCommand { get; }
        public ICommand AbrirHistoricoCommand { get; } // NOVO: Comando do botão do relógio

        public MainViewModel()
        {
            _mapService = new MapService();
            _historyService = new SearchHistoryService();

            BuscarCommand = new RelayCommand(async (obj) => await ExecutarBusca());

            SelecionarHistoricoCommand = new RelayCommand((obj) =>
            {
                if (obj is LocationModel local)
                {
                    TermoBusca = local.Logradouro;
                    IsSugestoesAberto = false;
                    _ = ExecutarBusca();
                }
            });

            SalvarCommand = new RelayCommand(async (obj) => await ExecutarSalvar(obj));

            // Inicializa o comando do histórico
            AbrirHistoricoCommand = new RelayCommand((obj) => ExecutarAbrirHistorico());

            CarregarHistorico();
        }

        public string TermoBusca
        {
            get => _termoBusca;
            set
            {
                _termoBusca = value;
                OnPropertyChanged();

                // Abre sugestões apenas se houver texto e algo no histórico
                IsSugestoesAberto = !string.IsNullOrWhiteSpace(_termoBusca) && HistoricoPesquisas.Any();
            }
        }

        public bool IsSugestoesAberto
        {
            get => _isSugestoesAberto;
            set { _isSugestoesAberto = value; OnPropertyChanged(); }
        }

        public ObservableCollection<LocationModel> Resultados { get; set; } = new ObservableCollection<LocationModel>();
        public ObservableCollection<LocationModel> HistoricoPesquisas { get; set; } = new ObservableCollection<LocationModel>();

        public LocationModel Selecionado
        {
            get => _selecionado;
            set
            {
                _selecionado = value;
                OnPropertyChanged();
            }
        }

        private async Task ExecutarBusca()
        {
            if (string.IsNullOrWhiteSpace(TermoBusca)) return;

            IsSugestoesAberto = false;
            Resultados.Clear();

            try
            {
                var locaisEncontrados = await _mapService.SearchLocationAsync(TermoBusca);

                foreach (var local in locaisEncontrados)
                {
                    Resultados.Add(local);
                }

                // Salva o primeiro resultado da busca no histórico automaticamente
                if (locaisEncontrados.Any())
                {
                    _historyService.AddToHistory(locaisEncontrados.First());
                    CarregarHistorico();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Erro na busca: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecutarAbrirHistorico()
        {
            // 1. Limpa a barra de pesquisa
            TermoBusca = string.Empty;

            // 2. Limpa a lista de resultados atual da tela
            Resultados.Clear();

            // 3. Garante que o histórico na memória está atualizado
            CarregarHistorico();

            // 4. Joga o histórico na lista de resultados para que apareça na interface do usuário
            foreach (var item in HistoricoPesquisas)
            {
                Resultados.Add(item);
            }
        }

        private async Task ExecutarSalvar(object obj)
        {
            var local = obj as LocationModel ?? Selecionado;

            if (local != null)
            {
                try
                {
                    var firebase = new FirebaseService();
                    await firebase.SalvarNoHistoricoAsync(local);
                    _historyService.AddToHistory(local);
                    CarregarHistorico();

                    MessageBox.Show($"Registro ID {local.Id} ({local.Logradouro}) salvo no Firebase!",
                                    "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Erro ao salvar: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CarregarHistorico()
        {
            HistoricoPesquisas.Clear();

            var historico = _historyService.GetHistory();

            foreach (var item in historico)
            {
                HistoricoPesquisas.Add(item);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}