using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GeoDataInsight.Client.DTO;


public class Squad2IntegrationService
{
    // 1. Declarado e inicializado aqui. O static garante que a conexão seja reutilizada.
    private static readonly HttpClient _httpClient = new HttpClient();
    private const string ApiUrl = "https://webapimaps.runasp.net/api/Mapas";

    // 2. O construtor fica vazio, pois a inicialização já foi feita acima.
    public Squad2IntegrationService()
    {
    }

    // Enviar um único registro
    public async Task<bool> EnviarLocalizacaoAsync(MapasApiRequest dados)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(ApiUrl, dados);

            if (response.IsSuccessStatusCode) return true;

            // Se falhar, você pode ler o corpo do erro para depurar
            var erroDetalhado = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Falha na API Squad 2: {response.StatusCode} - {erroDetalhado}");
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Exceção de rede: {ex.Message}");
            return false;
        }
    }

    // Enviar múltiplos registros (Lote)
    public async Task EnviarLoteAsync(List<MapasApiRequest> lista)
    {
        foreach (var item in lista)
        {
            bool sucesso = await EnviarLocalizacaoAsync(item);
            if (!sucesso)
            {
                // Lógica de log ou retry pode ser inserida aqui
                System.Diagnostics.Debug.WriteLine($"Falha ao enviar registro: {item.Logradouro}");
            }
        }
    }
}