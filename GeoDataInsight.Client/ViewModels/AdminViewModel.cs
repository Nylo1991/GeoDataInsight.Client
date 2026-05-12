using GeoDataInsight.Client.DTO;
using GeoDataInsight.Client.Helpers;
using GeoDataInsight.Client.Models;
using GeoDataInsight.Client.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GeoDataInsight.Client.ViewModels
{
    public class AdminViewModel : INotifyPropertyChanged
    {
        private readonly FirebaseService _firebaseService;
        private readonly Squad2IntegrationService _squad2Service;
        private List<LocationModel> _listaOriginal;
        private ObservableCollection<LocationModel> _todosRegistros;

        private string _filtroCep;
        private string _filtroId;
        private DateTime? _filtroData;
        private bool _filtrosAtivos;
        private bool _todosSelecionados;
        private bool _mostrarAvisoVazio;
        private LocationModel _selectedLocation;

        public ObservableCollection<LocationModel> TodosRegistros
        {
            get => _todosRegistros;
            set { _todosRegistros = value; OnPropertyChanged(); }
        }

        public LocationModel SelectedLocation
        {
            get => _selectedLocation;
            set { _selectedLocation = value; OnPropertyChanged(); }
        }

        public bool MostrarAvisoVazio
        {
            get => _mostrarAvisoVazio;
            set { _mostrarAvisoVazio = value; OnPropertyChanged(); }
        }

        public bool FiltrosAtivos
        {
            get => _filtrosAtivos;
            set { _filtrosAtivos = value; OnPropertyChanged(); AplicarFiltro(); }
        }

        public string FiltroCep
        {
            get => _filtroCep;
            set { _filtroCep = value; OnPropertyChanged(); AplicarFiltro(); }
        }

        public string FiltroId
        {
            get => _filtroId;
            set { _filtroId = value; OnPropertyChanged(); AplicarFiltro(); }
        }

        public DateTime? FiltroData
        {
            get => _filtroData;
            set { _filtroData = value; OnPropertyChanged(); AplicarFiltro(); }
        }

        // Comandos
        public ICommand DeletarSelecionadosCommand { get; }
        public ICommand CarregarDadosCommand { get; }
        public ICommand SelecionarTodosCommand { get; }
        public ICommand LimparFiltrosCommand { get; }
        public ICommand SincronizarComSquad2Command { get; }
        public ICommand LimparSelecaoCommand { get; }

        public AdminViewModel()
        {
            _firebaseService = new FirebaseService();
            _squad2Service = new Squad2IntegrationService();
            TodosRegistros = new ObservableCollection<LocationModel>();
            _listaOriginal = new List<LocationModel>();

            // Inicialização dos Comandos (Corrigido parênteses e vírgulas)
            DeletarSelecionadosCommand = new RelayCommand(async (obj) => await ExecutarDeletarLote());
            CarregarDadosCommand = new RelayCommand(async (obj) => await CarregarDados());
            SelecionarTodosCommand = new RelayCommand((obj) => AlternarSelecaoTodos());
            LimparFiltrosCommand = new RelayCommand((obj) => ResetarFiltros());
            SincronizarComSquad2Command = new RelayCommand(async (obj) => await ExecutarSincronizacaoSquad2());
            LimparSelecaoCommand = new RelayCommand((obj) => SelectedLocation = null);

            _ = CarregarDados();
        }

        private async Task CarregarDados()
        {
            try
            {
                var dados = await _firebaseService.GetTodosRegistrosAsync();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _listaOriginal = dados ?? new List<LocationModel>();
                    AplicarFiltro();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar dados: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AplicarFiltro()
        {
            IEnumerable<LocationModel> resultadoFiltro = _listaOriginal;

            if (FiltrosAtivos)
            {
                if (!string.IsNullOrWhiteSpace(FiltroCep))
                    resultadoFiltro = resultadoFiltro.Where(r => r.Cep != null && r.Cep.Contains(FiltroCep));

                if (!string.IsNullOrWhiteSpace(FiltroId))
                    resultadoFiltro = resultadoFiltro.Where(r => r.Id != null && r.Id.ToString().Contains(FiltroId));
            }

            var listaFinal = resultadoFiltro.ToList();

            // Numeração Sequencial
            int contador = 1;
            foreach (var item in listaFinal)
            {
                item.DisplayId = contador;
                contador++;
            }

            TodosRegistros = new ObservableCollection<LocationModel>(listaFinal);
            MostrarAvisoVazio = !TodosRegistros.Any();
        }

        private async Task ExecutarSincronizacaoSquad2()
        {
            var selecionados = TodosRegistros.Where(x => x.IsSelected).ToList();
            if (!selecionados.Any()) return;

            int sucessos = 0;
            int falhas = 0;

            foreach (var local in selecionados)
            {
                // 1. Tenta extrair apenas números do CEP original
                string cepFinal = new string(local.Cep?.Where(char.IsDigit).ToArray() ?? Array.Empty<char>());

                // 2. LÓGICA DE MASCARAMENTO:
                // Se não tem 8 dígitos, vamos "mascarar" para a API aceitar
                if (cepFinal.Length != 8)
                {
                    // Criamos um CEP fictício de 8 dígitos: 
                    // Os 3 primeiros dígitos identificam que é internacional (ex: 999)
                    // Os outros 5 tentamos manter do código original ou preenchemos com 0
                    string prefixoInternacional = "999";
                    string sufixoLimpo = cepFinal.Length > 5 ? cepFinal.Substring(0, 5) : cepFinal.PadLeft(5, '0');

                    cepFinal = prefixoInternacional + sufixoLimpo;

                    // Opcional: Adicionar no Logradouro que era um dado internacional original
                    // local.Logradouro = "[INTL] " + local.Logradouro;
                }

                var request = new MapasApiRequest
                {
                    Id = local.Id,
                    Logradouro = local.Logradouro,
                    Numero = local.Numero,
                    Bairro = local.Bairro,
                    Cep = cepFinal, // Agora sempre terá 8 dígitos
                    Latitude = local.Latitude,
                    Longitude = local.Longitude,
                    Timestamp = local.Timestamp.ToUniversalTime()
                };

                // Certifique-se de que _squad2Service foi instanciado no construtor!
                if (_squad2Service == null)
                {
                    MessageBox.Show("Erro: Serviço Squad 2 não inicializado.");
                    return;
                }

                bool ok = await _squad2Service.EnviarLocalizacaoAsync(request);
                if (ok) sucessos++; else falhas++;
            }

            MessageBox.Show($"Sincronização concluída!\nSucessos: {sucessos}\nFalhas: {falhas}", "Resultado Squad 2");
        }

        private async Task ExecutarDeletarLote()
        {
            var selecionados = TodosRegistros.Where(x => x.IsSelected).ToList();
            if (!selecionados.Any()) return;

            var confirmacao = MessageBox.Show($"Excluir {selecionados.Count} registro(s) permanentemente?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirmacao == MessageBoxResult.Yes)
            {
                try
                {
                    foreach (var item in selecionados)
                    {
                        await _firebaseService.DeletarDoHistoricoAsync(item.Key);
                        _listaOriginal.Remove(item);
                    }
                    AplicarFiltro();
                    MessageBox.Show("Registros removidos!", "Sucesso");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro: {ex.Message}", "Erro");
                }
            }
        }

        private void AlternarSelecaoTodos()
        {
            _todosSelecionados = !_todosSelecionados;
            foreach (var item in TodosRegistros) item.IsSelected = _todosSelecionados;
        }

        private void ResetarFiltros()
        {
            FiltroCep = string.Empty;
            FiltroId = string.Empty;
            FiltroData = null;
            AplicarFiltro();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}