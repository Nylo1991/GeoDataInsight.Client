using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using GeoDataInsight.Client.Models;

namespace GeoDataInsight.Client.Services
{
    public class FirebaseService
    {
        private readonly string _baseUrl = "https://geosquadexplorer-default-rtdb.firebaseio.com/";
        private readonly FirebaseClient _client;
        private readonly HttpClient _httpClient;

        public FirebaseService()
        {
            _client = new FirebaseClient(_baseUrl);
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        }

        // TESTE DE CONEXÃO: Rápido e eficiente
        public async Task<bool> TesteConexaoAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}.json?shallow=true");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // BUSCAR TODOS: Mapeamento limpo
        public async Task<List<LocationModel>> GetTodosRegistrosAsync()
        {
            try
            {
                var registros = await _client.Child("HistoricoBuscas").OnceAsync<LocationModel>();

                if (registros == null) return new List<LocationModel>();

                return registros.Select(item =>
                {
                    var model = item.Object;
                    model.Key = item.Key;
                    return model;
                }).ToList();
            }
            catch { return new List<LocationModel>(); }
        }

        public async Task<string> SalvarNoHistoricoAsync(LocationModel local)
        {
            // Garante que o ID seja gerado
            local.Id = await GetProximoIdAsync();

            // Boa prática: Garante que o registro tenha data/hora se não foi definida
            if (local.Timestamp == default)
                local.Timestamp = DateTime.Now;

            var resultado = await _client.Child("HistoricoBuscas").PostAsync(local);
            return resultado.Key;
        }

        // DELETAR ÚNICO
        public async Task DeletarDoHistoricoAsync(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            await _client.Child("HistoricoBuscas").Child(key).DeleteAsync();
        }

        // DELETAR EM LOTE (Otimizado)
        public async Task DeletarEmLoteAsync(IEnumerable<string> keys)
        {
            var tarefas = keys.Select(key => DeletarDoHistoricoAsync(key));
            await Task.WhenAll(tarefas);
        }

        public async Task<int> GetProximoIdAsync()
        {
            try
            {
                // Busca o valor atual do contador
                var snapshot = await _client.Child("Configuracoes").Child("UltimoId").OnceSingleAsync<int?>();
                int novoId = (snapshot ?? 0) + 1;

                // Atualiza o contador no Firebase para o próximo registro
                await _client.Child("Configuracoes").Child("UltimoId").PutAsync(novoId);

                return novoId;
            }
            catch
            {
                return 1; // Fallback caso o nó não exista
            }
        }

        public async Task ReordenarBancoAsync(List<LocationModel> listaParaReordenar)
        {
            // 1. Ordena a lista atual pela data ou ID antigo para garantir a ordem correta
            var listaOrdenada = listaParaReordenar.OrderBy(x => x.Timestamp).ToList();

            for (int i = 0; i < listaOrdenada.Count; i++)
            {
                int novoId = i + 1;
                var item = listaOrdenada[i];

                // Só atualiza no Firebase se o ID mudou
                if (item.Id != novoId)
                {
                    item.Id = novoId;
                    await _client.Child("HistoricoBuscas").Child(item.Key).PatchAsync(item);
                }
            }

            // 2. Reseta o contador global para o valor da nova contagem
            await _client.Child("Configuracoes").Child("UltimoId").PutAsync(listaOrdenada.Count);
        }


    }
}