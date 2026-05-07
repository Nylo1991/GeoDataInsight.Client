# 🌍 GeoDataInsight — Client (Squad 1)

> Aplicação desktop WPF responsável pela interface de busca geográfica e consumo da API de mapas do sistema **GeoData Insight**.
 
---

## 📌 Sobre o Projeto
 
O **GeoDataInsight Client** é a camada de apresentação do sistema distribuído **GeoData Insight**, desenvolvido como parte de uma atividade colaborativa em squads no curso Técnico em Desenvolvimento de Sistemas.
 
Esta aplicação é responsável por:
- Buscar localizações via API de mapas gratuita (OpenStreetMap / Nominatim)
- Exibir os resultados em uma interface visual com mapa interativo
- Permitir que o usuário selecione e salve locais no histórico em nuvem (Firebase)
- Integrar-se com o backend do **Squad 2** para persistência central dos dados

- ---
 
##  Arquitetura do Sistema
 
```
┌─────────────────────────────────────────────────────────┐
│                     GeoData Insight                     │
│                                                         │
│  [Squad 1 - WPF Client] ──► [Squad 2 - API Backend]    │
│         │                          │                    │
│         │                   [Firebase DB]               │
│         │                          │                    │
│         └──────────────► [Squad 3 - Análise Geográfica] │
└─────────────────────────────────────────────────────────┘
```
 
O **Squad 1** é o ponto de entrada do usuário: realiza a busca, exibe os dados e os envia para o backend.
 
---

## Tecnologias Utilizadas
 
| Tecnologia | Versão | Finalidade |
|---|---|---|
| .NET | 8.0 | Plataforma base |
| WPF (Windows Presentation Foundation) | — | Interface gráfica desktop |
| OpenStreetMap (Nominatim API) | — | Busca de localizações gratuita |
| Mapsui.Wpf | 5.0.2 | Renderização do mapa interativo |
| Firebase Realtime Database | — | Persistência do histórico em nuvem |
| FirebaseDatabase.net | 5.0.0 | SDK do Firebase para .NET |
| Newtonsoft.Json | 13.0.4 | Deserialização do JSON da API |
| RestSharp | 114.0.0 | Cliente HTTP |
| CommunityToolkit.Mvvm | 8.4.2 | Padrão MVVM (INotifyPropertyChanged) |
 
---
