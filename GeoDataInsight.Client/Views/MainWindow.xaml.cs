using BruTile.Wms;
using GeoDataInsight.Client.Services;
using GeoDataInsight.Client.ViewModels;
using Mapsui.Projections;
using Mapsui.Tiling;
using System;
using System.Windows;
using System.Windows.Controls;

namespace GeoDataInsight.Client.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private MapService _mapService;
        private FirebaseService _firebaseService;

        public MainWindow()
        {
            InitializeComponent();

            // Inicializa as classes de serviço
            _viewModel = new MainViewModel();
            _mapService = new MapService();
            _firebaseService = new FirebaseService();

            // Conecta a tela aos dados
            this.DataContext = _viewModel;

            // Inicia o mapa interativo
            InitializeMap();
        }

        private void InitializeMap()
        {
            var map = new Mapsui.Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            mapControl.Map = map;
        }

        private async void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            string termo = txtBusca.Text;
            if (string.IsNullOrWhiteSpace(termo)) return;

            try
            {
                btnBuscar.IsEnabled = false;
                txtStatus.Text = "Buscando...";

                var resultados = await _mapService.SearchLocationAsync(termo);

                _viewModel.Resultados.Clear();
                foreach (var local in resultados)
                {
                    _viewModel.Resultados.Add(local);
                }

                txtStatus.Text = resultados.Count > 0 ? "Pronto" : "Nenhum local encontrado.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao buscar: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatus.Text = "Erro na busca.";
            }
            finally
            {
                btnBuscar.IsEnabled = true;
            }
        }

        private async void btnSalvarFirebase_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.Selecionado == null)
            {
                MessageBox.Show("Selecione um local na lista antes de salvar!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                btnSalvarFirebase.IsEnabled = false;
                txtStatus.Text = "Salvando no Firebase...";

                // Usa o método que você criou na imagem enviada!
                await _firebaseService.SalvarNoHistoricoAsync(_viewModel.Selecionado);

                MessageBox.Show("Dados salvos no Firebase com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                txtStatus.Text = "Salvo com sucesso!";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar no Firebase: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatus.Text = "Erro ao salvar.";
            }
            finally
            {
                btnSalvarFirebase.IsEnabled = true;
            }
        }

        private void lstResultados_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Quando você clica em um item da lista, o mapa voa para lá!
            if (_viewModel.Selecionado != null)
            {
                var lat = _viewModel.Selecionado.Latitude;
                var lon = _viewModel.Selecionado.Longitude;

                var (x, y) = SphericalMercator.FromLonLat(lon, lat);
                var point = new Mapsui.MPoint(x, y);

                mapControl.Map.Navigator.CenterOn(point);
                mapControl.Map.Navigator.ZoomTo(2); git add .
            }
        }
    }
}