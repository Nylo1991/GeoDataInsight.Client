using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using GeoDataInsight.Client.Models;
using GeoDataInsight.Client.Helpers;

namespace GeoDataInsight.Client.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _termoBusca = string.Empty;
        private LocationModel _selecionado;

        public MainViewModel()
        {
            BuscarCommand = new RelayCommand(ExecutarBusca);
        }

        public ICommand BuscarCommand { get; }

        public string TermoBusca
        {
            get => _termoBusca;
            set { _termoBusca = value; OnPropertyChanged(); }
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

        private void ExecutarBusca(object obj)
        {
            Resultados.Clear();
            if (!string.IsNullOrWhiteSpace(TermoBusca))
            {
                Resultados.Add(new LocationModel
                {
                    Logradouro = "Sede AngloGold Ashanti",
                    Bairro = "Nova Lima",
                    Latitude = -19.9850,
                    Longitude = -43.8450,
                    Cep = "34000-000"
                });

                Resultados.Add(new LocationModel
                {
                    Logradouro = "Rio das Velhas - Área de Pesca",
                    Bairro = "Rio Acima",
                    Latitude = -20.0880,
                    Longitude = -43.7910,
                    Cep = "34300-000"
                });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}