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
                return registros.Select(item =>
                {
                    var model = item.Object;
                    model.Key = item.Key; // Importante para deletar depois
                    return model;
                }).ToList();
            }
            catch { return new List<LocationModel>(); }
        }

        // SALVAR
        public async Task<string> SalvarNoHistoricoAsync(LocationModel local)
        {
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
    }
}