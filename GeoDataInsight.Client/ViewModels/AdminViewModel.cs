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
        private List<LocationModel> _listaOriginal;
        private ObservableCollection<LocationModel> _todosRegistros;

        // Propriedades de Filtro
        private string _filtroCep;
        private string _filtroId;
        private DateTime? _filtroData;
        private bool _filtrosAtivos;
        private bool _todosSelecionados;
        private bool _mostrarAvisoVazio;

        public ObservableCollection<LocationModel> TodosRegistros
        {
            get => _todosRegistros;
            set { _todosRegistros = value; OnPropertyChanged(); }
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

        public AdminViewModel()
        {
            _firebaseService = new FirebaseService();
            TodosRegistros = new ObservableCollection<LocationModel>();
            _listaOriginal = new List<LocationModel>();

            // Inicialização dos Comandos
            DeletarSelecionadosCommand = new RelayCommand(async (obj) => await ExecutarDeletarLote());
            CarregarDadosCommand = new RelayCommand(async (obj) => await CarregarDados());
            SelecionarTodosCommand = new RelayCommand((obj) => AlternarSelecaoTodos());
            LimparFiltrosCommand = new RelayCommand((obj) => ResetarFiltros());

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

                    // A mágica da numeração vai acontecer aqui dentro!
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
            // 1. Sua lógica atual de filtragem continua igual
            // (Pode ser que você use LINQ aqui com o _listaOriginal)
            IEnumerable<LocationModel> resultadoFiltro = _listaOriginal;

            if (FiltrosAtivos)
            {
                // Exemplo de como seus filtros provavelmente estão
                if (!string.IsNullOrWhiteSpace(FiltroCep))
                    resultadoFiltro = resultadoFiltro.Where(r => r.Cep.Contains(FiltroCep));

                // ... outros filtros
            }

            var listaFinal = resultadoFiltro.ToList();

            // ---------------------------------------------------------
            // 2. NOVA LÓGICA: APLICAR NUMERAÇÃO SEQUENCIAL (DisplayId)
            // ---------------------------------------------------------
            int contador = 1;
            foreach (var item in listaFinal)
            {
                item.DisplayId = contador;
                contador++;
            }

            // 3. Atualiza a tela com a lista já numerada corretamente
            TodosRegistros = new ObservableCollection<LocationModel>(listaFinal);

            // Atualiza a visibilidade da mensagem de "Nenhum registro encontrado"
            MostrarAvisoVazio = !TodosRegistros.Any();
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

                    // Recalcula a lista e a numeração após deletar
                    AplicarFiltro();

                    MessageBox.Show("Registros removidos!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
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
            _filtroCep = string.Empty;
            _filtroId = string.Empty;
            _filtroData = null;

            // Notifica a UI que as propriedades mudaram
            OnPropertyChanged(nameof(FiltroCep));
            OnPropertyChanged(nameof(FiltroId));
            OnPropertyChanged(nameof(FiltroData));

            AplicarFiltro();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}