using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GeoDataInsight.Client.Models
{
    public class LocationModel : INotifyPropertyChanged
    {
        // ID original (mantenha intacto para uso interno do banco)
        public int Id { get; set; }

        // NOVA PROPRIEDADE: Apenas para exibição sequencial na interface
        private int _displayId;
        [JsonIgnore]
        public int DisplayId
        {
            get => _displayId;
            set { _displayId = value; OnPropertyChanged(); }
        }

        public string Logradouro { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Cep { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Timestamp { get; set; }
        public string Key { get; set; }
        public string Nome { get; set; }

        private bool _isSelected;

        [JsonIgnore]
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}