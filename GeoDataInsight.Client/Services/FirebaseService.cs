using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using GeoDataInsight.Client.Models;

namespace GeoDataInsight.Client.Services
{
    public class FirebaseService
    {
        // 👇 COLOQUE O LINK DO SEU FIREBASE AQUI 👇
        private readonly string FireBaseUrl = "https://geosquadexplorer-default-rtdb.firebaseio.com/";
        private readonly FirebaseClient _client;

        public FirebaseService()
        {
            _client = new FirebaseClient(FireBaseUrl);
        }

        public async Task DeletarDoHistoricoAsync(string key)
        {
            try
            {
                // O método DeleteAsync() remove o nó correspondente à chave fornecida
                await _client
                    .Child("HistoricoBuscas")
                    .Child(key)
                    .DeleteAsync();
            }
            catch (Exception ex)
            {
                // Debug para caso ocorra erro de permissão ou conexão
                System.Diagnostics.Debug.WriteLine($"Erro ao deletar: {ex.Message}");
                throw;
            }
        }

        public async Task<string> SalvarNoHistoricoAsync(LocationModel local)
        {
            // Realiza o post e armazena o resultado em uma variável
            var resultado = await _client
                .Child("HistoricoBuscas")
                .PostAsync(local);

            // Retorna a Key (chave única) que o Firebase acabou de criar
            return resultado.Key;
        }

        public async Task<List<LocationModel>> GetTodosRegistrosAsync()
        {
            var registros = await _client
                .Child("HistoricoBuscas")
                .OnceAsync<LocationModel>();

            return registros.Select(item => new LocationModel
            {
                Key = item.Key,
                Logradouro = item.Object.Logradouro,
                Latitude = item.Object.Latitude,
                Longitude = item.Object.Longitude,
                Timestamp = item.Object.Timestamp,
                Cep = item.Object.Cep,
                Id = item.Object.Id // Se você tiver esse campo
            }).ToList();
        }

        public async Task DeletarEmLoteAsync(IEnumerable<string> keys)
        {
            var tarefas = keys.Select(key =>
                _client.Child("HistoricoBuscas").Child(key).DeleteAsync());

            await Task.WhenAll(tarefas); // Executa todas as exclusões em paralelo
        }

    }
}