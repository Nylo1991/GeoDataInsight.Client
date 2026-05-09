using GeoDataInsight.Client.ViewModels;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.UI.Wpf;
using System.ComponentModel;
using System.Windows;
using Mapsui.Styles; 
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui;

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



// ... (resto do seu código inicial) ...

private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.Selecionado) && _viewModel.Selecionado != null)
        {
            // Agora passamos também o nome do local para o pino
            FocarLocal(_viewModel.Selecionado.Latitude, _viewModel.Selecionado.Longitude, _viewModel.Selecionado.Logradouro);
        }
    }

    private void FocarLocal(double lat, double lon, string nomeLocal = "Resultado")
    {
        var smPoint = SphericalMercator.FromLonLat(lon, lat);
        var pontoExato = new MPoint(smPoint.x, smPoint.y);

        // 1. Limpa qualquer pino de pesquisas anteriores
        _layerPins.Clear();

        // 2. Cria o novo marcador geométrico
        var pino = new PointFeature(pontoExato)
        {
            ["Name"] = nomeLocal
        };

        // 3. Define o estilo visual (Um círculo azul com borda branca, estilo GPS)
        pino.Styles.Add(new SymbolStyle
        {
            SymbolScale = 0.8,
            Fill = new Brush(new Color(37, 99, 235)), // Azul do botão de pesquisa
            Outline = new Pen(Color.White, 3)
        });

        // 4. Adiciona à camada do mapa
        _layerPins.Add(pino);

        // 5. Centraliza a câmera e dá um zoom bem próximo (nível 16)
        mapControl.Map.Navigator.CenterOnAndZoomTo(pontoExato, 16);

        // 6. Força a tela a se redesenhar para mostrar o pino imediatamente
        mapControl.Refresh();
    }

    private void InicializarMapa()
        {
            var map = new Mapsui.Map();

            // Volta para o mapa vetorial clássico (Street View) do OpenStreetMap
            map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

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