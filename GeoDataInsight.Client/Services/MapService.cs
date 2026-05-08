using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using GeoDataInsight.Client.Models;
using System.Globalization;

namespace GeoDataInsight.Client.Services
{
    public class MapService
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public MapService()
        {
            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "GeoDataInsight-App");
        }

        public async Task<List<LocationModel>> SearchLocationAsync(string query)
        {
            try
            {
                string url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(query)}&format=json&addressdetails=1&limit=5";

                var response = await _httpClient.GetStringAsync(url);
                var results = JsonConvert.DeserializeObject<List<OsmResult>>(response);
                var listaFinal = new List<LocationModel>();

                if (results == null) return listaFinal;

                int contadorId = 1;

                foreach (var item in results)
                {
                    var addr = item.address;

                    // Lógica para definir a Categoria e Ícone dinamicamente
                    var cat = ObterCategoria(item.ClassType, item.type);

                    // Simula uma avaliação fixa baseada no nome do local para manter o aspeto do Google Maps
                    var random = new Random(item.display_name.GetHashCode());
                    double nota = Math.Round(random.NextDouble() * (5.0 - 3.5) + 3.5, 1);
                    int total = random.Next(10, 2000);
                    string formatadoTotal = total > 1000 ? $"({Math.Round(total / 1000.0, 1)}k)" : $"({total})";

                    listaFinal.Add(new LocationModel
                    {
                        Id = contadorId++,
                        Logradouro = addr?.road ?? item.display_name.Split(',')[0],
                        Numero = addr?.house_number ?? "S/N",
                        Bairro = addr?.suburb ?? addr?.neighbourhood ?? addr?.city_district ?? "Não informado",
                        Cep = addr?.postcode ?? "00000-000",
                        Latitude = double.Parse(item.lat, CultureInfo.InvariantCulture),
                        Longitude = double.Parse(item.lon, CultureInfo.InvariantCulture),
                        Timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),

                        // Ligações da nova UI
                        Categoria = cat.categoria,
                        IconeCategoria = cat.icone,
                        Avaliacao = nota.ToString("0.1", CultureInfo.InvariantCulture),
                        TotalAvaliacoes = formatadoTotal,
                        IconeImagem = cat.icone
                    });
                }

                return listaFinal;
            }
            catch (Exception)
            {
                return new List<LocationModel>();
            }
        }

        private (string icone, string categoria) ObterCategoria(string osmcClass, string osmType)
        {
            if (osmcClass == "amenity" || osmcClass == "shop" || osmcClass == "office") return ("🏢", "Business");
            if (osmcClass == "highway") return ("🛣️", "Endereço");
            if (osmcClass == "tourism" || osmcClass == "historic") return ("📸", "Turismo");
            if (osmcClass == "building") return ("🏬", "Edifício");
            if (osmcClass == "leisure" || osmcClass == "natural") return ("🌳", "Natureza/Lazer");
            return ("📍", "Localização");
        }

        private class OsmResult
        {
            public string display_name { get; set; } = string.Empty;
            public string lat { get; set; } = string.Empty;
            public string lon { get; set; } = string.Empty;
            [JsonProperty("class")]
            public string ClassType { get; set; } = string.Empty;
            public string type { get; set; } = string.Empty;
            public OsmAddress? address { get; set; }
        }

        private class OsmAddress
        {
            public string? road { get; set; }
            public string? house_number { get; set; }
            public string? suburb { get; set; }
            public string? neighbourhood { get; set; }
            public string? city_district { get; set; }
            public string? postcode { get; set; }
        }
    }
}