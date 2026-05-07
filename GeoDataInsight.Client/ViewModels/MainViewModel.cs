using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using GeoDataInsight.Client.Models;

namespace GeoDataInsight.Client.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // Guarda a lista de lugares que a API encontrar
        public ObservableCollection<LocationModel> Resultados { get; set; } = new ObservableCollection<LocationModel>();

        // Guarda o lugar específico que o usuário clicou na lista
        private LocationModel _selecionado;
        public LocationModel Selecionado
        {
            get => _selecionado;
            set
            {
                _selecionado = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}