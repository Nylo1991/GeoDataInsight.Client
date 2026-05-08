using GeoDataInsight.Client.ViewModels;

namespace GeoDataInsight.Client.Models
{
    // Ela herda do BaseViewModel para a interface reagir quando ligarmos/desligarmos a camada no "olhinho"
    public class LayerModel : BaseViewModel
    {
        public string LayerName { get; set; }

        private bool _isVisible;
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                OnPropertyChanged();
            }
        }
    }
}