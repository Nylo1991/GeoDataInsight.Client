using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


// Dependências do seu projeto
using GeoDataInsight.Client.Helpers;
using GeoDataInsight.Client.Models;
using GeoDataInsight.Client.Services;
using GeoDataInsight.Client.Views; // Adicionado para reconhecer a AdminWindow

namespace GeoDataInsight.Client.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // ==========================================
        // 1. CAMPOS PRIVADOS (Variáveis internas)
        // ==========================================
        private string _termoBusca = string.Empty;
        private LocationModel _selecionado;
        private bool _isSugestoesAberto;

        private readonly MapService _mapService;
        private readonly SearchHistoryService _historyService;
        private readonly FirebaseService _firebaseService;
        private System.Windows.Threading.DispatcherTimer _statusTimer;

        // ==========================================
        // 2. COMANDOS (Ações da Interface)
        // ==========================================
        public ICommand BuscarCommand { get; }
        public ICommand SelecionarHistoricoCommand { get; }
        public ICommand SalvarCommand { get; }
        public ICommand AbrirHistoricoCommand { get; }
        public ICommand DeletarCommand { get; }
        public ICommand AbrirPainelAdminCommand { get; }

        // ==========================================
        // 3. PROPRIEDADES (Dados expostos para a Tela)
        // ==========================================
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

        public LocationModel Selecionado
        {
            get => _selecionado;
            set { _selecionado = value; OnPropertyChanged(); }
        }

        public ObservableCollection<LocationModel> Resultados { get; set; }
        public ObservableCollection<LocationModel> HistoricoPesquisas { get; set; }

        // ==========================================
        // 4. CONSTRUTOR (Inicialização)
        // ==========================================
        public MainViewModel()
        {
            // Instanciando Serviços
            _mapService = new MapService();
            _historyService = new SearchHistoryService();
            _firebaseService = new FirebaseService();
            _ = VerificarSaudeApi();


            // Instanciando Coleções
            Resultados = new ObservableCollection<LocationModel>();
            HistoricoPesquisas = new ObservableCollection<LocationModel>();

            // Vinculando Comandos aos seus Métodos
            BuscarCommand = new RelayCommand(async (obj) => await ExecutarBusca());
            SelecionarHistoricoCommand = new RelayCommand(ExecutarSelecionarHistorico);
            SalvarCommand = new RelayCommand(async (obj) => await ExecutarSalvar(obj));
            AbrirHistoricoCommand = new RelayCommand((obj) => ExecutarAbrirHistorico());
            DeletarCommand = new RelayCommand(async (obj) => await ExecutarDeletar(obj));
            AbrirPainelAdminCommand = new RelayCommand((obj) => AbrirPainelAdmin());

            // Carregamento Inicial
            CarregarHistorico();
        }

        // ==========================================
        // 5. MÉTODOS DE EXECUÇÃO (Lógica de Negócio)
        // ==========================================
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
            catch (Exception ex)
            {
                MessageBox.Show($"Erro na busca: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecutarSelecionarHistorico(object obj)
        {
            if (obj is LocationModel local)
            {
                TermoBusca = local.Logradouro;
                IsSugestoesAberto = false;
                _ = ExecutarBusca();
            }
        }

        private void ExecutarAbrirHistorico()
        {
            TermoBusca = string.Empty;
            Resultados.Clear();
            CarregarHistorico();

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
                    // 1. GARANTIR DATA E HORA ATUAL
                    // Isso corrige o problema da data "0001" que aparecia na sua tabela
                    local.Timestamp = DateTime.Now;

                    // 2. SALVAR NO FIREBASE
                    // O FirebaseService (com a alteração que sugerimos antes) 
                    // vai buscar o GetProximoIdAsync() internamente dentro deste método.
                    string chaveDoFirebase = await _firebaseService.SalvarNoHistoricoAsync(local);

                    // 3. ATUALIZAR A CHAVE LOCAL
                    local.Key = chaveDoFirebase;

                    // Atualiza o histórico local e a UI
                    _historyService.AddToHistory(local);
                    CarregarHistorico();

                    MessageBox.Show($"Salvo com sucesso!\nID Gerado: {local.Id}", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao salvar: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task ExecutarDeletar(object obj)
        {
            var local = obj as LocationModel ?? Selecionado;

            if (local != null && !string.IsNullOrEmpty(local.Key))
            {
                try
                {
                    await _firebaseService.DeletarDoHistoricoAsync(local.Key);
                    _historyService.RemoveFromHistory(local);

                    if (HistoricoPesquisas.Contains(local))
                        HistoricoPesquisas.Remove(local);

                    if (Resultados.Contains(local))
                        Resultados.Remove(local);

                    MessageBox.Show($"Registro ({local.Logradouro}) deletado do Firebase com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao deletar: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Não foi possível identificar a chave do registro para exclusão. Verifique se o dado veio do Firebase.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private void AbrirPainelAdmin()
        {
            var adminWindow = new AdminWindow();
            adminWindow.Show();
        }

        // ==========================================
        // 6. EVENTOS (Notificação de Mudança)
        // ==========================================
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private string _apiStatusText = "Verificando...";
        public string ApiStatusText
        {
            get => _apiStatusText;
            set { _apiStatusText = value; OnPropertyChanged(); }
        }

        private string _apiStatusColor = "#94A3B8"; // Cinza inicial
        public string ApiStatusColor
        {
            get => _apiStatusColor;
            set { _apiStatusColor = value; OnPropertyChanged(); }
        }

        private void IniciarMonitoramentoStatus()
        {
            _statusTimer = new System.Windows.Threading.DispatcherTimer();
            _statusTimer.Interval = TimeSpan.FromSeconds(30);
            _statusTimer.Tick += async (s, e) => await VerificarSaudeApi();
            _statusTimer.Start();
        }

        public async Task VerificarSaudeApi()
        {
            // Tenta conectar usando o método que refatoramos no FirebaseService
            bool estaOnline = await _firebaseService.TesteConexaoAsync();

            if (estaOnline)
            {
                ApiStatusText = "API Online";
                ApiStatusColor = "#10B981"; // Verde
            }
            else
            {
                ApiStatusText = "API Offline";
                ApiStatusColor = "#EF4444"; // Vermelho
            }
        }   
    }
}