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

                // Correção do erro CS0103: Definindo a variável results
                var results = JsonConvert.DeserializeObject<List<OsmResult>>(response);
                var listaFinal = new List<LocationModel>();

                if (results == null) return listaFinal;

                foreach (var item in results)
                {
                    listaFinal.Add(new LocationModel
                    {
                        // Correção do erro CS0117: Usando Logradouro em vez de Nome
                        Logradouro = item.address?.road ?? item.display_name,
                        Numero = item.address?.house_number ?? "S/N",
                        Bairro = item.address?.suburb ?? "N/A",
                        Cep = item.address?.postcode ?? "N/A",
                        Latitude = double.Parse(item.lat, CultureInfo.InvariantCulture),
                        Longitude = double.Parse(item.lon, CultureInfo.InvariantCulture),
                        Timestamp = DateTime.Now
                    });
                }
                return listaFinal;
            }
            catch { return new List<LocationModel>(); }
        }

        private class OsmResult
        {
            public string display_name { get; set; } = "";
            public string lat { get; set; } = "";
            public string lon { get; set; } = "";
            public OsmAddress? address { get; set; }
        }

        private class OsmAddress
        {
            public string? road { get; set; }
            public string? house_number { get; set; }
            public string? suburb { get; set; }
            public string? postcode { get; set; }
        }
    }
}