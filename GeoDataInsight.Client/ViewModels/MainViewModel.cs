using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using GeoDataInsight.Client.Models;
using GeoDataInsight.Client.Helpers; // Necessário para o RelayCommand

namespace GeoDataInsight.Client.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _termoBusca = string.Empty;
        private string _statusMensagem = "Pronto";
        private LocationModel? _selecionado;
        private bool _isPanelVisible;

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

        public ObservableCollection<LocationModel> Resultados { get; set; } = new ObservableCollection<LocationModel>();
        public LocationModel? Selecionado
        {
            get => _selecionado;
            set { _selecionado = value; OnPropertyChanged(); }
        }

        public bool IsPanelVisible
        {
            get => _isPanelVisible;
            set { _isPanelVisible = value; OnPropertyChanged(); }
        }
        public ICommand MarkerClickCommand { get; }
        public ICommand SalvarCommand { get; }

        public MainViewModel()
        {
            IsPanelVisible = false;

            MarkerClickCommand = new RelayCommand(p => {
                if (p is LocationModel loc)
                {
                    Selecionado = loc;
                    IsPanelVisible = true;
                }
            });

            SalvarCommand = new RelayCommand(p => {

                StatusMensagem = "Localização salva no histórico com sucesso!";
                IsPanelVisible = false; 
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}