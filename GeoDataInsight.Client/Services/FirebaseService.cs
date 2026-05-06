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

        public async Task SalvarNoHistoricoAsync(LocationModel local)
        {
            await _client.Child("HistoricoBuscas").PostAsync(local);
        }
    }
}