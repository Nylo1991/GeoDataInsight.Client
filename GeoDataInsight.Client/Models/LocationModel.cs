using System;

namespace GeoDataInsight.Client.Models
{
    public class LocationModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Nome { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Cep { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}