using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GeoDataInsight.Client.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // O atributo [CallerMemberName] pega automaticamente o nome da propriedade que chamou o método
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}