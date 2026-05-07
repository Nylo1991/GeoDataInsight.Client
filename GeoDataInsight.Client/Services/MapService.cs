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
        private readonly HttpClient _client;

        public MapService()
        {
            _client = new HttpClient();
            // User-Agent obrigatório para o OpenStreetMap
            _client.DefaultRequestHeaders.Add("User-Agent", "GeoDataInsight-Squad1");
        }

        public async Task<List<LocationModel>> SearchLocationAsync(string query)
        {
            try
            {
                // Buscamos com addressdetails=1 para pegar os campos separados (rua, bairro, etc)
                string url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(query)}&format=json&addressdetails=1&limit=5";

                var response = await _client.GetStringAsync(url);
                var results = JsonConvert.DeserializeObject<List<OsmResult>>(response);

                var listaFinal = new List<LocationModel>();

                foreach (var item in results)
                {
                    listaFinal.Add(new LocationModel
                    {
                        // Mapeando os atributos específicos que você pediu
                        Logradouro = item.address?.road ?? item.display_name,
                        Numero = item.address?.house_number ?? "S/N",
                        Bairro = item.address?.suburb ?? item.address?.neighbourhood ?? "N/A",
                        Cep = item.address?.postcode ?? "N/A",
                        Latitude = double.Parse(item.lat, CultureInfo.InvariantCulture),
                        Longitude = double.Parse(item.lon, CultureInfo.InvariantCulture),
                        Timestamp = DateTime.Now
                    }
                    // Adicionamos uma propriedade extra apenas para exibir na lista (opcional)
                    );
                }

                return listaFinal;
            }
            catch
            {
                return new List<LocationModel>();
            }
        }

        // Classes auxiliares para o JSON complexo do OSM
        private class OsmResult
        {
            public string display_name { get; set; }
            public string lat { get; set; }
            public string lon { get; set; }
            public OsmAddress address { get; set; }
        }

        private class OsmAddress
        {
            public string road { get; set; }
            public string house_number { get; set; }
            public string suburb { get; set; }
            public string neighbourhood { get; set; }
            public string postcode { get; set; }
        }
    }
}