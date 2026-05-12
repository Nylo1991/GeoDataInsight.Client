using Google.Cloud.Firestore;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GeoDataInsight.Client.Models
{
    [FirestoreData]
    public class LocationModel : INotifyPropertyChanged
    {
        [FirestoreProperty] public string Id { get; set; } = string.Empty;
        [FirestoreProperty] public string Logradouro { get; set; } = string.Empty;
        [FirestoreProperty] public string Numero { get; set; } = string.Empty;
        [FirestoreProperty] public string Bairro { get; set; } = string.Empty;
        [FirestoreProperty] public string Cep { get; set; } = string.Empty;
        [FirestoreProperty] public double Latitude { get; set; }
        [FirestoreProperty] public double Longitude { get; set; }
        [FirestoreProperty] public DateTime Timestamp { get; set; }

        [JsonIgnore] public string Key { get; set; } = string.Empty;

        // Campos privados (Backing Fields) necessários para evitar o CS0103
        private int _displayId;
        private bool _isSelected;

        [JsonIgnore]
        public int DisplayId
        {
            get => _displayId;
            set { _displayId = value; OnPropertyChanged(); }
        }

        [JsonIgnore]
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}