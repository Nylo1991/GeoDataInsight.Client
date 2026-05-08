sbusing System;
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
            // A API Nominatim exige um User-Agent identificado
            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "GeoDataInsight-App");
        }

        public async Task<List<LocationModel>> SearchLocationAsync(string query)
        {
            try
            {
                // A URL inclui addressdetails=1 para trazer rua, bairro e número separadamente
                string url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(query)}&format=json&addressdetails=1&limit=5";

                var response = await _httpClient.GetStringAsync(url);
                var results = JsonConvert.DeserializeObject<List<OsmResult>>(response);
                var listaFinal = new List<LocationModel>();

                if (results == null) return listaFinal;

                int contadorId = 1;

                foreach (var item in results)
                {
                    var addr = item.address;

                    listaFinal.Add(new LocationModel
                    {
                        Id = contadorId++,
                        Logradouro = addr?.road ?? item.display_name.Split(',')[0],
                        Numero = addr?.house_number ?? "S/N",
                        Bairro = addr?.suburb ?? addr?.neighbourhood ?? addr?.city_district ?? "Não informado",
                        Cep = addr?.postcode ?? "00000-000",
                        Latitude = double.Parse(item.lat, CultureInfo.InvariantCulture),
                        Longitude = double.Parse(item.lon, CultureInfo.InvariantCulture),
                        Timestamp = DateTime.Now
                    });
                }

                return listaFinal;
            }
            catch (Exception)
            {
                // Retorna lista vazia em caso de erro para não travar o App
                return new List<LocationModel>();
            }
        }

        // Classes internas para mapear o JSON da API
        private class OsmResult
        {
            public string display_name { get; set; } = string.Empty;
            public string lat { get; set; } = string.Empty;
            public string lon { get; set; } = string.Empty;
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