namespace GeoDataInsight.Client.Models
{
    public class LocationModel
    {
        public int Id { get; set; }
        public string Logradouro { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Numero { get; set; } = "S/N";
        public string Cep { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Timestamp { get; set; } = string.Empty;

        // Novas propriedades dinâmicas para a UI avançada
        public string Categoria { get; set; } = "Localização";
        public string IconeCategoria { get; set; } = "📍";
        public string Avaliacao { get; set; } = "0.0";
        public string TotalAvaliacoes { get; set; } = "(0)";
        public string IconeImagem { get; set; } = "🖼️";
    }
}