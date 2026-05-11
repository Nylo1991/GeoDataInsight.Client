using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using GeoDataInsight.Client.Models;

namespace GeoDataInsight.Client.Services
{
    public class SearchHistoryService
    {
        private readonly string _filePath;
        private const int MaxHistoryLimit = 15; // Limite de itens igual ao Waze

        public SearchHistoryService()
        {
            // Cria uma pasta oculta segura nos Documentos ou AppData do Windows para salvar o histórico
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GeoDataInsight");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            _filePath = Path.Combine(folderPath, "search_history.json");
        }

        /// <summary>
        /// Busca o histórico salvo no computador do usuário.
        /// </summary>
        public List<LocationModel> GetHistory()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return new List<LocationModel>();

                string json = File.ReadAllText(_filePath);
                return JsonConvert.DeserializeObject<List<LocationModel>>(json) ?? new List<LocationModel>();
            }
            catch (Exception)
            {
                // Em caso de erro de leitura (arquivo corrompido), retorna lista vazia
                return new List<LocationModel>();
            }
        }

        /// <summary>
        /// Adiciona um local recém-pesquisado ou clicado ao topo do histórico.
        /// </summary>
        public void AddToHistory(LocationModel location)
        {
            if (location == null) return;

            var history = GetHistory();

            // 1. Remove o local caso ele já exista no histórico (compara por Logradouro ou Coordenadas)
            history.RemoveAll(x => x.Latitude == location.Latitude && x.Longitude == location.Longitude);

            // 2. Atualiza o Timestamp para o momento atual do clique/busca
            location.Timestamp = DateTime.Now;

            // 3. Insere no topo da lista (posição 0)
            history.Insert(0, location);

            // 4. Se passar do limite, corta os mais antigos
            if (history.Count > MaxHistoryLimit)
            {
                history = history.Take(MaxHistoryLimit).ToList();
            }

            // 5. Salva de volta no arquivo JSON
            SalvarNoArquivo(history);
        }

        /// <summary>
        /// Limpa todo o histórico (Útil para um botão de "Apagar Histórico" nas configurações).
        /// </summary>
        public void ClearHistory()
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }

        private void SalvarNoArquivo(List<LocationModel> history)
        {
            try
            {
                string json = JsonConvert.SerializeObject(history, Formatting.Indented);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                // Aqui você pode fazer um log do erro futuramente
                Console.WriteLine($"Erro ao salvar histórico: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove um item específico do histórico local.
        /// </summary>
        public void RemoveFromHistory(LocationModel location)
        {
            if (location == null) return;

            var history = GetHistory();

            // Remove o item combinando a Chave (se existir) ou as Coordenadas
            history.RemoveAll(x =>
                (!string.IsNullOrEmpty(x.Key) && x.Key == location.Key) ||
                (x.Latitude == location.Latitude && x.Longitude == location.Longitude));

            // Salva o histórico atualizado (sem o item deletado) de volta no arquivo
            SalvarNoArquivo(history);
        }
    }
}