using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using GeoDataInsight.Client.Models;

namespace GeoDataInsight.Client.Services
{
    public class MapService
    {
        private readonly HttpClient _httpClient;

        public MapService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "GeoDataInsight-Squad1");
        }

        public async Task<LocationModel> BuscarLocalAsync(string busca)
        {
            try
            {
                string url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(busca)}&format=json&addressdetails=1&limit=1";
                var response = await _httpClient.GetStringAsync(url);
                var jsonArray = JArray.Parse(response);

                if (jsonArray.Count > 0)
                {
                    var resultado = jsonArray[0];
                    var address = resultado["address"];

                    return new LocationModel
                    {
                        Nome = resultado["display_name"]?.ToString() ?? "Desconhecido",
                        Bairro = address?["suburb"]?.ToString() ?? address?["neighbourhood"]?.ToString() ?? "N/A",
                        Cep = address?["postcode"]?.ToString() ?? "N/A",
                        Latitude = double.Parse(resultado["lat"].ToString(), System.Globalization.CultureInfo.InvariantCulture),
                        Longitude = double.Parse(resultado["lon"].ToString(), System.Globalization.CultureInfo.InvariantCulture),
                        Timestamp = DateTime.Now
                    };
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}