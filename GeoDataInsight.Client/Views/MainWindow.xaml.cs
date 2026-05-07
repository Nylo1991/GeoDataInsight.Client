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
        private readonly MainViewModel _viewModel;
        private readonly MapService _mapService;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            _mapService = new MapService();

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
            if (string.IsNullOrWhiteSpace(_viewModel.TermoBusca)) return;

            try
            {
                _viewModel.StatusMensagem = "Buscando...";
                btnBuscar.IsEnabled = false;

                var resultados = await _mapService.SearchLocationAsync(_viewModel.TermoBusca);

                _viewModel.Resultados.Clear();
                foreach (var local in resultados)
                {
                    _viewModel.Resultados.Add(local);
                }

                _viewModel.StatusMensagem = resultados.Count > 0 ? "Busca concluída" : "Nenhum local encontrado";
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Erro na busca: {ex.Message}");
                _viewModel.StatusMensagem = "Erro na operação";
            }
            finally
            {
                btnBuscar.IsEnabled = true;
            }
        }

        private void lstResultados_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel.Selecionado != null)
            {
                var lon = _viewModel.Selecionado.Longitude;
                var lat = _viewModel.Selecionado.Latitude;

                // Converte coordenadas para o formato do Mapsui (Spherical Mercator)
                var (x, y) = SphericalMercator.FromLonLat(lon, lat);

                // CORREÇÃO CS1061: Forma correta de centralizar e dar zoom nas versões atuais do Mapsui
                mapControl.Map?.Navigator.CenterOnAndZoomTo(new Mapsui.MPoint(x, y), 2);

                _viewModel.StatusMensagem = $"Localizado: {_viewModel.Selecionado.Logradouro}";
            }
        }

        private void btnSalvarFirebase_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.Selecionado == null)
            {
                MessageBox.Show("Selecione um local na lista primeiro.");
                return;
            }

            // Lógica de salvar será implementada após a limpeza total
            MessageBox.Show($"Pronto para salvar o ID: {_viewModel.Selecionado.Id} no Firebase.");
        }
    }
}