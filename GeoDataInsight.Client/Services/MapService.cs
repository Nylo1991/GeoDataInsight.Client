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
                // Aumentamos a precisão com addressdetails e extra_tags para pegar códigos regionais
                string url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(query)}" +
                             $"&format=json&addressdetails=1&extratags=1&limit=15" +
                             $"&accept-language=pt-BR";

                var response = await _httpClient.GetStringAsync(url);
                var results = JsonConvert.DeserializeObject<List<OsmResult>>(response);
                var listaFinal = new List<LocationModel>();

                if (results == null) return listaFinal;

                int contadorId = 1;

                foreach (var item in results)
                {
                    var addr = item.address;

                    // Identifica a Região/Continente (Lógica simplificada baseada no país ou extratags)
                    string regiao = ObterRegiaoPorCodigoPais(addr?.country_code?.ToUpper());

                    listaFinal.Add(new LocationModel
                    {
                        Id = contadorId++,
                        Logradouro = addr?.GetNomeLocal() ?? item.display_name.Split(',')[0],
                        Numero = addr?.house_number ?? "S/N",

                        // Exibição do Bairro + País para dar contexto global
                        Bairro = $"{(addr?.suburb ?? addr?.city ?? "N/A")}, {addr?.country?.ToUpper()}",

                        // LÓGICA DE CÓDIGO POSTAL: 
                        // Se não tem CEP, tenta um código regional das extratags ou o código do país + região
                        Cep = addr?.postcode ?? (item.extratags?.ContainsKey("ref") == true ? item.extratags["ref"] : $"{addr?.country_code?.ToUpper()}-{regiao}"),

                        Latitude = double.Parse(item.lat, CultureInfo.InvariantCulture),
                        Longitude = double.Parse(item.lon, CultureInfo.InvariantCulture),
                        Timestamp = DateTime.Now
                    });
                }

                return listaFinal;
            }
            catch (Exception)
            {
                return new List<LocationModel>();
            }
        }

        private string ObterRegiaoPorCodigoPais(string code)
        {
            if (string.IsNullOrEmpty(code)) return "GL"; // Global

            // Mapeamento básico de regiões
            return code switch
            {
                "BR" or "AR" or "CL" or "CO" => "SA", // South America
                "US" or "CA" or "MX" => "NA",         // North America
                "FR" or "DE" or "IT" or "PT" => "EU", // Europe
                "CN" or "JP" or "IN" => "AS",         // Asia
                _ => "INT" // International
            };
        }

        private class OsmResult
        {
            public string display_name { get; set; } = string.Empty;
            public string lat { get; set; } = string.Empty;
            public string lon { get; set; } = string.Empty;
            public OsmAddress? address { get; set; }
            public Dictionary<string, string>? extratags { get; set; }
        }

        private class OsmAddress
        {
            public string? road { get; set; }
            public string? house_number { get; set; }
            public string? suburb { get; set; }
            public string? city { get; set; }
            public string? postcode { get; set; }
            public string? country { get; set; }
            public string? country_code { get; set; }

            // Pontos de interesse
            public string? attraction { get; set; }
            public string? tourism { get; set; }
            public string? amenity { get; set; }

            public string? GetNomeLocal()
            {
                return attraction ?? tourism ?? amenity ?? road;
            }
        }
    }
}