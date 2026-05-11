using GeoDataInsight.Client.Helpers;
using GeoDataInsight.Client.Models;
using GeoDataInsight.Client.Services;
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

        // Usamos uma lista original para guardar os dados sem filtro
        private List<LocationModel> _listaOriginal;
        private ObservableCollection<LocationModel> _todosRegistros;

        private string _filtroCep;
        private bool _todosSelecionados;

        public ObservableCollection<LocationModel> TodosRegistros
        {
            get => _todosRegistros;
            set { _todosRegistros = value; OnPropertyChanged(); }
        }

        public string FiltroCep
        {
            get => _filtroCep;
            set
            {
                _filtroCep = value;
                OnPropertyChanged();
                AplicarFiltro(); // Filtra a lista automaticamente ao digitar
            }
        }

        public ICommand DeletarSelecionadosCommand { get; }
        public ICommand CarregarDadosCommand { get; }
        public ICommand SelecionarTodosCommand { get; } // Comando do CheckBox principal

        public AdminViewModel()
        {
            _firebaseService = new FirebaseService();

            // Inicializa a coleção apenas UMA vez no construtor
            TodosRegistros = new ObservableCollection<LocationModel>();
            _listaOriginal = new List<LocationModel>();

            DeletarSelecionadosCommand = new RelayCommand(async (obj) => await ExecutarDeletarLote());
            CarregarDadosCommand = new RelayCommand(async (obj) => await CarregarDados());
            SelecionarTodosCommand = new RelayCommand((obj) => AlternarSelecaoTodos());

            // Carrega os dados assim que abrir o painel
            _ = CarregarDados();
        }

        private async Task CarregarDados()
        {
            var dados = await _firebaseService.GetTodosRegistrosAsync();

            // Limpa a coleção atual ao invés de recriar a variável
            TodosRegistros.Clear();

            if (dados != null && dados.Any())
            {
                _listaOriginal = dados.ToList();

                // Popula a ObservableCollection item por item
                foreach (var item in _listaOriginal)
                {
                    TodosRegistros.Add(item);
                }
            }

            FiltroCep = string.Empty; // Limpa o filtro ao recarregar
        }

        private async Task ExecutarDeletarLote()
        {
            var selecionados = TodosRegistros.Where(x => x.IsSelected).ToList();

            if (!selecionados.Any())
            {
                MessageBox.Show("Nenhum item selecionado para exclusão.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Pede confirmação antes de apagar
            var confirmacao = MessageBox.Show($"Tem certeza que deseja excluir {selecionados.Count} registro(s) do Firebase?",
                                              "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmacao == MessageBoxResult.Yes)
            {
                foreach (var item in selecionados)
                {
                    await _firebaseService.DeletarDoHistoricoAsync(item.Key);
                    TodosRegistros.Remove(item);
                    _listaOriginal.Remove(item); // Remove do backup de dados também
                }
                MessageBox.Show("Itens deletados com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void AplicarFiltro()
        {
            TodosRegistros.Clear();

            if (string.IsNullOrWhiteSpace(FiltroCep))
            {
                // Se a caixa de pesquisa estiver vazia, restaura os dados originais
                foreach (var item in _listaOriginal)
                {
                    TodosRegistros.Add(item);
                }
            }
            else
            {
                // Filtra pelo CEP (ignorando o traço "-" caso o usuário digite com ou sem)
                var listaFiltrada = _listaOriginal.Where(x =>
                    !string.IsNullOrEmpty(x.Cep) &&
                    x.Cep.Replace("-", "").Contains(FiltroCep.Replace("-", ""))
                ).ToList();

                foreach (var item in listaFiltrada)
                {
                    TodosRegistros.Add(item);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}