# 🌍 GeoDataInsight — Client (Squad 1)
 
> Aplicação desktop **WPF (.NET 8)** responsável pela interface de busca geográfica, visualização de mapas interativos, histórico de pesquisas, painel administrativo e integração real com a API do **Squad 2** do sistema distribuído **GeoData Insight**.
 
---
 
## 📌 Sobre o Projeto
 
O **GeoDataInsight Client** é a camada de apresentação do sistema **GeoData Insight**, desenvolvido como atividade colaborativa em squads no curso **Técnico em Desenvolvimento de Sistemas Noite**.
 
A aplicação permite que o usuário busque localizações em tempo real, visualize os resultados em um mapa interativo, salve dados no Firebase, gerencie o histórico por meio de um painel administrativo completo e **sincronize registros diretamente com a API REST do Squad 2**.
 
---
 
## 🏗️ Arquitetura do Sistema (Microserviços)
 
```
┌──────────────────────────────────────────────────────────────────────┐
│                          GeoData Insight                             │
│                                                                      │
│  ┌──────────────────────┐    HTTP POST     ┌──────────────────────┐  │
│  │   Squad 1 — WPF      │ ───────────────► │  Squad 2 — Backend   │  │
│  │   (Este projeto)     │  /api/Mapas      │  API REST (C#/ASP)   │  │
│  └──────────┬───────────┘                  └──────────┬───────────┘  │
│             │                                         │              │
│        Firebase SDK                            Firebase DB           │
│             ▼                                         │              │
│      [Firebase Realtime DB] ◄──────────────────────────              │
│             │                                                        │
│             ▼                                                        │
│  ┌──────────────────────┐                                            │
│  │  Squad 3 — Análise   │                                            │
│  │  Geográfica/Insights │                                            │
│  └──────────────────────┘                                            │
└──────────────────────────────────────────────────────────────────────┘
```
 
---
 
## 🛠️ Tecnologias Utilizadas
 
| Tecnologia | Versão | Finalidade |
|---|---|---|
| .NET | 8.0 | Plataforma base da aplicação |
| WPF | — | Interface gráfica desktop Windows |
| OpenStreetMap (Nominatim API) | — | Busca de localizações gratuita e sem chave |
| Mapsui.Wpf | 5.0.2 | Renderização do mapa interativo com pinos |
| Firebase Realtime Database | — | Persistência do histórico em nuvem |
| FirebaseDatabase.net | 5.0.0 | SDK do Firebase Realtime para .NET |
| Google.Cloud.Firestore | 4.2.0 | Suporte a anotações `[FirestoreData]` no modelo |
| Newtonsoft.Json | 13.0.4 | Serialização/deserialização de JSON |
| RestSharp | 114.0.0 | Cliente HTTP auxiliar |
| CommunityToolkit.Mvvm | 8.4.2 | Padrão MVVM (INotifyPropertyChanged) |
| System.Net.Http.Json | — | Envio de JSON para a API do Squad 2 |
 
---
 
## 📁 Estrutura do Projeto
 
```
GeoDataInsight.Client/
│
├── DTO/
│   └── MapasApiRequest.cs           # DTO de envio para a API REST do Squad 2
│
├── Models/
│   ├── LocationModel.cs             # Modelo principal com atributos [FirestoreData]
│   ├── LayerModel.cs                # Modelo de camada do mapa (visibilidade reativa)
│   └── RelayCommand.cs              # Implementação de ICommand para MVVM
│
├── Services/
│   ├── MapService.cs                # Consumo da API Nominatim (OpenStreetMap)
│   ├── FirebaseService.cs           # CRUD completo + contador de IDs + reordenação
│   ├── SearchHistoryService.cs      # Histórico local persistido em JSON (AppData)
│   └── Squad2IntegrationService.cs  # Integração HTTP com a API REST do Squad 2
│
├── ViewModels/
│   ├── BaseViewModel.cs             # Classe base com INotifyPropertyChanged
│   ├── MainViewModel.cs             # ViewModel principal com todos os comandos
│   └── AdminViewModel.cs            # ViewModel do painel admin (filtros, lote, sync)
│
├── Views/
│   ├── MainWindow.xaml              # Interface principal com mapa e busca
│   ├── MainWindow.xaml.cs           # Code-behind: mapa, pinos, navegação
│   ├── AdminWindow.xaml             # Painel administrativo com DataGrid + detalhes
│   └── AdminWindow.xaml.cs          # Code-behind do painel admin
│
├── App.xaml                         # Configuração e startup da aplicação WPF
├── App.xaml.cs                      # Entry point
└── GeoDataInsight.Client.csproj     # Arquivo de projeto .NET 8
```
 
---
 
## ⚙️ Funcionalidades Implementadas
 
### 🔍 Busca de Localização Avançada
- Campo de busca com suporte a **Enter** para pesquisar
- Consulta à **API Nominatim** do OpenStreetMap com retorno em português (`pt-BR`)
- Retorna até **15 resultados** por consulta
- Detecta automaticamente: logradouro, número, bairro, CEP, cidade, país, latitude e longitude
- Suporte a pontos de interesse: `attraction`, `tourism`, `amenity`
- Lógica de **região geográfica** automática por código de país
- Código postal inteligente: usa CEP nativo quando disponível, senão gera código regional
### 🗺️ Mapa Interativo com Pinos
- Mapa **OpenStreetMap** completo renderizado via **Mapsui**
- Pino visual azul (`#2563EB`) com borda branca ao selecionar um resultado
- **Centralização e zoom automático** (nível 16) ao clicar em qualquer item da lista
- Foco inicial em **Nova Lima, MG** ao abrir o app
- Camada dedicada de pinos (`WritableLayer`) limpa e recria a cada seleção
### 📋 Histórico Local de Pesquisas
- Histórico salvo em arquivo **JSON** no `AppData\Local\GeoDataInsight\search_history.json`
- Limite de **15 itens**, removendo os mais antigos automaticamente
- Última busca sempre aparece **no topo** da lista
- Duplicatas removidas automaticamente por coordenadas
- Botão de histórico na barra de busca exibe os últimos locais
- Remoção individual de itens do histórico
### ☁️ Integração com Firebase (CRUD Completo)
- **Salvar** localização no Firebase com **ID sequencial automático** gerado via contador no nó `Configuracoes/UltimoId`
- **Listar** todos os registros com suas chaves únicas do Firebase
- **Deletar** registro individual por chave Firebase
- **Deletar em lote** com `Task.WhenAll` para máxima performance
- **Reordenar banco**: função `ReordenarBancoAsync` que reorganiza os IDs sequencialmente após exclusões
- **Teste de conexão** automático ao abrir o app
### 🔁 Integração com a API do Squad 2 — `Squad2IntegrationService`
Esta é a **principal novidade desta versão**. O projeto agora possui integração real e funcional com a API REST do Squad 2.
 
- **Endpoint configurado:** `https://webapimaps.runasp.net/api/Mapas`
- Envio via **HTTP POST** com `PostAsJsonAsync`
- Suporte a **envio individual** e **envio em lote** (`EnviarLoteAsync`)
- **Lógica de mascaramento de CEP:** CEPs internacionais (sem 8 dígitos) são automaticamente convertidos para o formato aceito pela API do Squad 2 com prefixo `999` + sufixo numérico
- Log de debug via `System.Diagnostics.Debug.WriteLine` para falhas de rede
- `HttpClient` declarado como `static` para reutilização de conexão (boas práticas .NET)
**Fluxo de sincronização:**
```
Usuário seleciona registros no Admin
       ↓
Clica em "☁️ Sincronizar Squad 2"
       ↓
Para cada registro selecionado:
  - Extrai dígitos do CEP
  - Aplica máscara se necessário (8 dígitos)
  - Monta MapasApiRequest (DTO)
  - POST para https://webapimaps.runasp.net/api/Mapas
       ↓
Exibe resultado: "Sucessos: X | Falhas: Y"
```
 
### 📦 DTO — `MapasApiRequest`
Objeto de transferência de dados criado exclusivamente para comunicação com o Squad 2:
 
```csharp
public class MapasApiRequest
{
    public string Id { get; set; }
    public string Logradouro { get; set; }
    public string Numero { get; set; }
    public string Bairro { get; set; }
    public string Cep { get; set; }          // Sempre 8 dígitos
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime DataHora { get; set; }
    public DateTime UltimaAtualizacao { get; set; }
    public DateTime Timestamp { get; set; }
}
```
 
### 🏷️ Model com Suporte ao Firestore — `LocationModel`
O modelo `LocationModel` foi atualizado com anotações do **Google Cloud Firestore**:
 
```csharp
[FirestoreData]
public class LocationModel : INotifyPropertyChanged
{
    [FirestoreProperty] public string Id { get; set; }
    [FirestoreProperty] public string Logradouro { get; set; }
    [FirestoreProperty] public double Latitude { get; set; }
    // ...
    [JsonIgnore] public int DisplayId { get; set; }   // ID sequencial exibido na UI
    [JsonIgnore] public bool IsSelected { get; set; } // Seleção para operações em lote
}
```
 
- `Id` agora é `string` (compatível com Firebase e Firestore)
- `DisplayId` é um campo visual separado, calculado na UI sem afetar o banco
- Campos com `[JsonIgnore]` são controlados localmente e não persistidos
### 🛡️ Painel Administrativo Atualizado
Acessível pelo botão de engrenagem (⚙️) no canto superior direito.
 
**Layout em duas colunas:**
- **Esquerda:** DataGrid com lista de registros (checkbox + ID + CEP + Logradouro)
- **Direita:** Painel de detalhes do registro selecionado com campos editáveis (Logradouro, Número, Bairro, CEP) e coordenadas somente leitura
**Comandos disponíveis:**
 
| Botão | Ação |
|---|---|
| Selecionar Todos | Marca/desmarca todos os checkboxes |
| 🔄 Atualizar Banco | Recarrega os dados do Firebase |
| ☁️ Sincronizar Squad 2 | Envia registros selecionados para a API do Squad 2 |
| 🗑️ Excluir Selecionados | Remove os registros marcados do Firebase (com confirmação) |
| Limpar Seleção | Deseleciona o item no painel de detalhes |
| Limpar Filtros | Redefine todos os filtros |
 
**Filtros:**
- Filtro por **ID** (texto parcial)
- Filtro por **CEP** (texto parcial, busca flexível)
- **Switch** para ativar/desativar os filtros
- Numeração sequencial (`DisplayId`) recalculada automaticamente após cada filtro
### 🟢 Indicador de Status da API
- Badge no topo com ponto colorido: 🟢 Online / 🔴 Offline
- Verificação automática ao abrir o app
- Timer periódico a cada 30 segundos
### ⚠️ Tratamento de Erros
- API de mapas fora do ar → lista vazia sem travar
- Firebase offline → badge vermelho + `MessageBox` ao tentar salvar
- Campo de busca vazio → bloqueio antes de requisitar
- Histórico corrompido → retorna lista vazia automaticamente
- Falha no envio ao Squad 2 → contabiliza falhas e exibe resumo ao final
- CEP fora do padrão → mascaramento automático antes do envio
---
 
## 🔌 API Utilizada — Nominatim (OpenStreetMap)
 
- **URL base:** `https://nominatim.openstreetmap.org/search`
- **Gratuita**, sem necessidade de chave de API
- Cabeçalho `User-Agent: GeoDataInsight-App` configurado automaticamente
| Parâmetro | Valor | Descrição |
|---|---|---|
| `q` | texto | Endereço, cidade ou ponto de interesse |
| `format` | `json` | Formato de retorno |
| `addressdetails` | `1` | Campos separados (rua, bairro, país...) |
| `extratags` | `1` | Dados extras (refs regionais) |
| `limit` | `15` | Máximo de resultados |
| `accept-language` | `pt-BR` | Resultados em português |
 
**Mapeamento de regiões:**
 
| Código | Países | Região |
|---|---|---|
| `SA` | BR, AR, CL, CO | América do Sul |
| `NA` | US, CA, MX | América do Norte |
| `EU` | FR, DE, IT, PT | Europa |
| `AS` | CN, JP, IN | Ásia |
| `INT` | demais | Internacional |
 
---
 
## ☁️ Configuração do Firebase
 
1. Acesse [https://console.firebase.google.com](https://console.firebase.google.com)
2. Use o projeto `geosquadexplorer` (ou crie um novo)
3. Ative o **Realtime Database** em modo teste
4. Confirme ou atualize a URL em `Services/FirebaseService.cs`:
```csharp
private readonly string _baseUrl = "https://geosquadexplorer-default-rtdb.firebaseio.com/";
```
 
### Estrutura dos dados no Firebase
 
```
geosquadexplorer-default-rtdb/
│
├── Configuracoes/
│   └── UltimoId: 42              ← Contador global de IDs sequenciais
│
└── HistoricoBuscas/
    ├── -ABC123.../
    │   ├── Id: "1"
    │   ├── Logradouro: "Avenida Paulista"
    │   ├── Numero: "1578"
    │   ├── Bairro: "Bela Vista, BRAZIL"
    │   ├── Cep: "01310200"
    │   ├── Latitude: -23.5614
    │   ├── Longitude: -46.6558
    │   └── Timestamp: "2026-05-07T14:30:00"
    └── -DEF456.../
        └── ...
```
 
---
 
## 🚀 Como Executar
 
### Pré-requisitos
- **Visual Studio 2022** ou superior (workload de .NET Desktop)
- **.NET 8.0 SDK** ([download](https://dotnet.microsoft.com/download/dotnet/8.0))
- **Windows 10/11** (WPF é exclusivo para Windows)
- Conexão com a internet (para API de mapas, Firebase e API do Squad 2)
### Passo a Passo
 
```bash
# 1. Clone o repositório
git clone https://github.com/seu-usuario/GeoDataInsight-Client.git
 
# 2. Entre na pasta do projeto
cd GeoDataInsight-Client/GeoDataInsight.Client
 
# 3. Restaure os pacotes NuGet
dotnet restore
 
# 4. Execute o projeto
dotnet run
```
 
Ou abra `GeoDataInsight.Client.csproj` no Visual Studio e pressione **F5**.
 
---
 
## 📦 Dependências NuGet
 
```xml
<PackageReference Include="CommunityToolkit.Mvvm"    Version="8.4.2" />
<PackageReference Include="FirebaseDatabase.net"     Version="5.0.0" />
<PackageReference Include="Google.Cloud.Firestore"   Version="4.2.0" />
<PackageReference Include="Mapsui.Wpf"               Version="5.0.2" />
<PackageReference Include="Newtonsoft.Json"          Version="13.0.4" />
<PackageReference Include="RestSharp"                Version="114.0.0" />
```
 
---
 
## 🧱 Padrões de Projeto Utilizados
 
| Padrão | Onde é usado |
|---|---|
| **MVVM** | Toda a aplicação (View ↔ ViewModel ↔ Model) |
| **Command Pattern** | `RelayCommand` para todos os botões e ações |
| **Observer Pattern** | `INotifyPropertyChanged` em todos os ViewModels e `LocationModel` |
| **DTO Pattern** | `MapasApiRequest` separa o modelo interno do contrato da API externa |
| **Service Layer** | `MapService`, `FirebaseService`, `SearchHistoryService`, `Squad2IntegrationService` |
| **Base Class** | `BaseViewModel` centraliza `OnPropertyChanged` para todos os ViewModels |
| **Static HttpClient** | `Squad2IntegrationService` reutiliza a conexão HTTP (boas práticas .NET) |
 
---
 
## 🤝 Integração entre Squads
 
### Squad 2 — API Backend
- **URL da API:** `https://webapimaps.runasp.net/api/Mapas`
- Integração **já implementada e funcional** via `Squad2IntegrationService`
- Envio via `HTTP POST` com payload `MapasApiRequest`
- CEPs internacionais são convertidos automaticamente para 8 dígitos antes do envio
### Squad 3 — Análise Geográfica
- Consome os dados do Firebase para gerar análises
- Recebe dados salvos pelo Squad 1 via nó `HistoricoBuscas` do Realtime Database
---
## 👥 Equipe — Squad 1
 
| Posição | Nome |
|---|---|
| Primeiro | Vinícius Augusto|
| **Segundo** | Lucas Aquino|
| Terceiro | Luis Ivan |
| Quarto | Alice Virgilia |
| Quinto | Danilo da Silva |
| Sexto | Erick Silva |
 
---
 
## 📄 Licença
 
Projeto desenvolvido para fins educacionais — Curso Técnico em Desenvolvimento de Sistemas Noite.  
Instrutor: **Frederico Aguiar**
 
---
 
*GeoData Insight — Aula 08 | Situação de Aprendizagem | Squad 1 - Aplicação Cliente WPF*
 
