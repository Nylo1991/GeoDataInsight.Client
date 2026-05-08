using System.ComponentModel;
using System.Windows;
using Mapsui;
using Mapsui.Tiling;
using Mapsui.Layers;
using Mapsui.Projections;
using GeoDataInsight.Client.ViewModels;

namespace GeoDataInsight.Client.Views
{
    public partial class MainWindow : Window
    {
        private WritableLayer _layerPins;
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            // Fica de olho quando você clica em um item da lista
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;

            InicializarMapa();
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.Selecionado) && _viewModel.Selecionado != null)
            {
                FocarLocal(_viewModel.Selecionado.Latitude, _viewModel.Selecionado.Longitude);
            }
        }

        private void InicializarMapa()
        {
            var map = new Mapsui.Map();

            // 1. URL pública do satélite da ESRI (ArcGIS)
            string urlSatelite = "https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}";

            // 2. Criamos a fonte do mapa manualmente, sem depender do enum do BruTile
            var fonteSatelite = new BruTile.Web.HttpTileSource(
                new BruTile.Predefined.GlobalSphericalMercator(),
                urlSatelite,
                name: "Satelite_ESRI");

            // 3. Adicionamos a camada de imagem ao mapa
            map.Layers.Add(new Mapsui.Tiling.Layers.TileLayer(fonteSatelite));

            // Camada para os pinos de localização
            _layerPins = new Mapsui.Layers.WritableLayer { Name = "Pontos" };
            map.Layers.Add(_layerPins);

            mapControl.Map = map;

            // Foco inicial (coordenadas de Nova Lima)
            FocarLocal(-19.982, -43.847);
        }

        private void FocarLocal(double lat, double lon)
        {
            var smPoint = SphericalMercator.FromLonLat(lon, lat);
            mapControl.Map.Navigator.CenterOnAndZoomTo(new MPoint(smPoint.x, smPoint.y), 15);
            mapControl.Refresh();
        }
    }
}