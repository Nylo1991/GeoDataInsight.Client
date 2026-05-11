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

        public ObservableCollection<LocationModel> TodosRegistros
        {
            get => _todosRegistros;
            set { _todosRegistros = value; OnPropertyChanged(); }
        }

        #region Propriedades dos Filtros Avançados

        private bool _mostrarAvisoVazio;
        public bool MostrarAvisoVazio
        {
            get => _mostrarAvisoVazio;
            set { _mostrarAvisoVazio = value; OnPropertyChanged(); }
        }

        public bool FiltrosAtivos
        {
            get => _filtrosAtivos;
            set
            {
                _filtrosAtivos = value;
                OnPropertyChanged();
                AplicarFiltro(); // Reaplica ou limpa filtros ao alternar o switch
            }
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

        #endregion

        public ICommand DeletarSelecionadosCommand { get; }
        public ICommand CarregarDadosCommand { get; }
        public ICommand SelecionarTodosCommand { get; }
        public ICommand LimparFiltrosCommand { get; }

        public AdminViewModel()
        {
            _firebaseService = new FirebaseService();
            TodosRegistros = new ObservableCollection<LocationModel>();
            _listaOriginal = new List<LocationModel>();

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
                    AplicarFiltro(); // Preenche a tela respeitando filtros atuais
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar dados: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AplicarFiltro()
        {
            if (TodosRegistros == null || _listaOriginal == null) return;

            TodosRegistros.Clear();

            // 1. Se os filtros estiverem desligados, mostra a lista completa
            if (!FiltrosAtivos)
            {
                foreach (var item in _listaOriginal) TodosRegistros.Add(item);
                MostrarAvisoVazio = !TodosRegistros.Any(); // Avisa se o banco estiver 100% vazio
                return;
            }

            // 2. Lógica de Filtros Combinados (LINQ)
            var consulta = _listaOriginal.AsQueryable();

            // Filtro por CEP (Removendo traços para busca flexível)
            if (!string.IsNullOrWhiteSpace(FiltroCep))
            {
                string cepBusca = FiltroCep.Replace("-", "");
                consulta = consulta.Where(x => x.Cep != null && x.Cep.Replace("-", "").Contains(cepBusca));
            }

            // Filtro por ID
            if (!string.IsNullOrWhiteSpace(FiltroId))
            {
                consulta = consulta.Where(x => x.Id.ToString().Contains(FiltroId));
            }

            // Filtro por Data
            if (FiltroData.HasValue)
            {
                consulta = consulta.Where(x => x.Timestamp.Date == FiltroData.Value.Date);
            }

            // 3. Executa a filtragem e preenche a lista da interface
            var resultado = consulta.ToList();
            foreach (var item in resultado)
            {
                TodosRegistros.Add(item);
            }

            // 4. A VALIDAÇÃO QUE FALTAVA: 
            // Se a consulta não retornou nada, mas o usuário digitou algo, ativa o aviso.
            MostrarAvisoVazio = !TodosRegistros.Any();
        }

        private async Task ExecutarDeletarLote()
        {
            var selecionados = TodosRegistros.Where(x => x.IsSelected).ToList();

            if (!selecionados.Any())
            {
                MessageBox.Show("Selecione pelo menos um registro para excluir.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmacao = MessageBox.Show(
                $"OPERAÇÃO IRREVERSÍVEL!\n\nDeseja excluir {selecionados.Count} registro(s) permanentemente?",
                "Confirmação de Segurança", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirmacao == MessageBoxResult.Yes)
            {
                try
                {
                    // Mostra que está processando (opcional: adicionar uma flag IsBusy)
                    foreach (var item in selecionados)
                    {
                        await _firebaseService.DeletarDoHistoricoAsync(item.Key);
                        _listaOriginal.Remove(item);
                        TodosRegistros.Remove(item);
                    }

                    MessageBox.Show("Registros removidos com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro durante a exclusão: {ex.Message}", "Falha Crítica", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AlternarSelecaoTodos()
        {
            _todosSelecionados = !_todosSelecionados;
            foreach (var item in TodosRegistros)
            {
                item.IsSelected = _todosSelecionados;
            }
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