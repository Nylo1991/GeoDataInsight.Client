using System;
using System.Windows;
using System.Windows.Controls;
using GeoDataInsight.Client.ViewModels;
using GeoDataInsight.Client.Services;
using Mapsui.Tiling;
using Mapsui.Projections;

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
            _viewModel = new MainViewModel();
            _mapService = new MapService();
            _firebaseService = new FirebaseService();
            this.DataContext = _viewModel;
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
                _viewModel.StatusMensagem = "Buscando...";

                var resultados = await _mapService.SearchLocationAsync(termo);
                _viewModel.Resultados.Clear();

                foreach (var local in resultados)
                {
                    _viewModel.Resultados.Add(local);
                }
                _viewModel.StatusMensagem = "Pronto";
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Erro ao buscar: " + ex.Message);
            }
            finally { btnBuscar.IsEnabled = true; }
        }

        private async void btnSalvarFirebase_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.Selecionado == null) return;
            try
            {
                await _firebaseService.SalvarNoHistoricoAsync(_viewModel.Selecionado);
                MessageBox.Show("Salvo com sucesso!");
            }
            catch (System.Exception ex) { MessageBox.Show("Erro: " + ex.Message); }
        }

        private void lstResultados_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel.Selecionado != null)
            {
                var lon = _viewModel.Selecionado.Longitude;
                var lat = _viewModel.Selecionado.Latitude;
                var (x, y) = SphericalMercator.FromLonLat(lon, lat);
                mapControl.Map.Navigator.CenterOn(new Mapsui.MPoint(x, y));
                mapControl.Map.Navigator.ZoomTo(2);
            }
        }
    }
}