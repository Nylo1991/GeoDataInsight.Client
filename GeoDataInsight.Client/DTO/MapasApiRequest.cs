namespace GeoDataInsight.Client.DTO
{
    public class MapasApiRequest
    {
        public string Id { get; set; }
        public string Logradouro { get; set; }
        public string Numero { get; set; }
        public string Bairro { get; set; }
        public string Cep { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Adicione estes campos se não existirem no DTO:
        public DateTime DataCadastro { get; set; }
        public DateTime DataHora { get; set; }
        public DateTime UltimaAtualizacao { get; set; }
        public DateTime Timestamp { get; set; }
    }
}