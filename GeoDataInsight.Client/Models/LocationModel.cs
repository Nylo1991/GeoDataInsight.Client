using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GeoDataInsight.Client.Models
{
    public class LocationModel : INotifyPropertyChanged
    {
        private int id;
        private string logradouro = string.Empty;
        private string numero = string.Empty;
        private string bairro = string.Empty;
        private string cep = string.Empty;
        private double latitude;
        private double longitude;
        private DateTime timestamp;

        public int Id
        {
            get => id;
            set { id = value; OnPropertyChanged(); }
        }

        public string Logradouro
        {
            get => logradouro;
            set { logradouro = value; OnPropertyChanged(); }
        }

        public string Numero
        {
            get => numero;
            set { numero = value; OnPropertyChanged(); }
        }

        public string Bairro
        {
            get => bairro;
            set { bairro = value; OnPropertyChanged(); }
        }

        public string Cep
        {
            get => cep;
            set { cep = value; OnPropertyChanged(); }
        }

        public double Latitude
        {
            get => latitude;
            set { latitude = value; OnPropertyChanged(); }
        }

        public double Longitude
        {
            get => longitude;
            set { longitude = value; OnPropertyChanged(); }
        }

        public DateTime Timestamp
        {
            get => timestamp;
            set { timestamp = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}