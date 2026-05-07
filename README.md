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

## 📁 Estrutura do Projeto
 
```
GeoDataInsight.Client/
│
├── Models/
│   ├── LocationModel.cs       # Modelo de dados de uma localização
│   └── RelayCommand.cs        # Implementação de ICommand para MVVM
│
├── Services/
│   ├── MapService.cs          # Consumo da API Nominatim (OpenStreetMap)
│   └── FirebaseService.cs     # Integração com Firebase Realtime Database
│
├── ViewModels/
│   └── MainViewModel.cs       # ViewModel principal (padrão MVVM)
│
├── Views/
│   ├── MainWindow.xaml        # Interface gráfica principal
│   └── MainWindow.xaml.cs     # Code-behind da janela principal
│
├── App.xaml                   # Configuração da aplicação WPF
├── App.xaml.cs                # Entry point da aplicação
└── GeoDataInsight.Client.csproj  # Arquivo de projeto .NET
```
 
---

## ⚙️ Funcionalidades Implementadas
 
### ✅ Busca de Localização
- Campo de texto para digitação do local (cidade, endereço, coordenadas)
- Botão de busca que consome a API **Nominatim** do OpenStreetMap
- Retorna até **5 resultados** por consulta
- Extrai automaticamente: logradouro, número, bairro, CEP, latitude e longitude
### ✅ Exibição de Resultados
- Lista com os locais encontrados
- Painel de detalhes com todos os campos do local selecionado
- Mapa interativo (via **Mapsui**) que centraliza automaticamente no local selecionado ao clicar na lista
### ✅ Histórico em Nuvem (Firebase)
- Botão para salvar o local selecionado no **Firebase Realtime Database**
- Os dados ficam armazenados no nó `HistoricoBuscas` do banco
### ✅ Tratamento de Erros
- API fora do ar → retorna lista vazia sem travar a aplicação
- Campo de busca vazio → bloqueio antes de fazer a requisição
- Erros de rede → mensagem exibida via `MessageBox`
- Botão de busca desabilitado durante o carregamento
### ✅ Feedback Visual
- Barra de status com mensagens em tempo real (`"Buscando..."`, `"Busca concluída"`, `"Erro na operação"`)

- ---
 
## API Utilizada
 
### Nominatim — OpenStreetMap
 
- **URL base:** `https://nominatim.openstreetmap.org/search`
- **Gratuita** e sem necessidade de chave de API
- **Parâmetros usados:**
| Parâmetro | Valor | Descrição |
|---|---|---|
| `q` | texto da busca | Endereço ou local pesquisado |
| `format` | `json` | Formato de retorno |
| `addressdetails` | `1` | Retorna campos separados (rua, bairro, etc.) |
| `limit` | `5` | Número máximo de resultados |
 
>  A API Nominatim exige um `User-Agent` identificado no cabeçalho HTTP. O projeto já configura isso automaticamente como `"GeoDataInsight-App"`.
 
---

## ☁️ Configuração do Firebase
 
1. Acesse [https://console.firebase.google.com](https://console.firebase.google.com)
2. Crie um projeto chamado `geosquadexplorer` (ou outro nome)
3. Ative o **Realtime Database**
4. Copie a URL do banco (ex: `https://seu-projeto-default-rtdb.firebaseio.com/`)
5. No arquivo `Services/FirebaseService.cs`, substitua a URL:
```csharp
private readonly string FireBaseUrl = "https://SEU-PROJETO-default-rtdb.firebaseio.com/";
```
 
> O projeto já está pré-configurado com a URL `geosquadexplorer-default-rtdb.firebaseio.com`.
 
---

## 🚀 Como Executar
 
### Pré-requisitos
- Visual Studio 2022 ou superior
- .NET 8.0 SDK instalado
- Conexão com a internet (para API de mapas e Firebase)
### Passo a Passo
 
```bash
# 1. Clone o repositório
git clone https://github.com/seu-usuario/GeoDataInsight-Client.git
 
# 2. Abra o projeto no Visual Studio
# Abra o arquivo GeoDataInsight.Client.csproj
 
# 3. Restaure os pacotes NuGet
# O Visual Studio faz isso automaticamente, ou via terminal:
dotnet restore
 
# 4. Execute o projeto
dotnet run
# ou pressione F5 no Visual Studio
```
 
---

## 📦 Dependências (NuGet)
 
```xml
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.2" />
<PackageReference Include="FirebaseDatabase.net" Version="5.0.0" />
<PackageReference Include="Mapsui.Wpf" Version="5.0.2" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
<PackageReference Include="RestSharp" Version="114.0.0" />
```
 
---

## 🤝 Integração com o Squad 2 (Backend)
 
O Squad 1 envia os dados do local selecionado para a **API REST do Squad 2** via HTTP POST.
 
> 🔧 A integração com o endpoint do Squad 2 está preparada na camada de serviços e será conectada assim que a URL da API do backend estiver disponível.
 
**Dados enviados (LocationModel):**
 
```json
{
  "id": 1,
  "logradouro": "Avenida Paulista",
  "numero": "1578",
  "bairro": "Bela Vista",
  "cep": "01310-200",
  "latitude": -23.5614,
  "longitude": -46.6558,
  "timestamp": "2026-05-07T14:30:00"
}
```
 
---
