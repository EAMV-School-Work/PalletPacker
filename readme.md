# PalletPacker

PalletPacker er et ASP.NET Core MVC-projekt til administration af paller, optimeringsindstillinger og generering af pallepakningsplaner.

Projektet er udviklet som et 3. semester eksamensprojekt på Datamatikeruddannelsen ved EAMV med udgangspunkt i en case fra Acies A/S.

Systemet benytter .NET 10, ASP.NET Core MVC, Entity Framework Core og Microsoft SQL Server. Docker anvendes til at sikre et ensartet udviklingsmiljø og gøre det muligt at starte hele applikationen uden manuel installation af SQL Server eller .NET.

## 1. Projektstruktur og arkitektur

Projektet er organiseret i en .NET solution med fem projekter.

```text
PalletPacker/
│
├── src/
│   ├── PalletPacker.Web/
│   ├── PalletPacker.Application/
│   ├── PalletPacker.Core/
│   └── PalletPacker.Infrastructure/
│
├── tests/
│   └── PalletPacker.Tests/
│
├── .config/
│   └── dotnet-tools.json
│
├── .env.example
├── .gitignore
├── .dockerignore
├── compose.yaml
├── run-dev.ps1
└── PalletPacker.sln
```

### 1.1 Projekternes ansvarsområder

| Projekt | Ansvar |
|---|---|
| **PalletPacker.Web** | ASP.NET Core MVC, Controllers, Views, ViewModels, routing og applikationens opstart |
| **PalletPacker.Application** | Applikationsservices, use cases og koordinering af forretningsoperationer |
| **PalletPacker.Core** | Domænemodeller, forretningsregler, interfaces og selve optimeringsalgoritmen |
| **PalletPacker.Infrastructure** | Databasetilgang, Entity Framework Core, DbContext, migrations og repositories |
| **PalletPacker.Tests** | Automatiserede tests med xUnit |

**Web** fungerer som applikationens entry point og er det eneste projekt, der skal startes for at køre applikationen. De øvrige produktionsprojekter er class libraries, som kompileres til DLL-filer og anvendes af webapplikationen.

#### Web

Indeholder brugergrænsefladen og håndteringen af HTTP-requests.

Projektet følger ASP.NET Core MVC, hvor:

- Controllers modtager og håndterer HTTP-requests.
- Views er Razor-filer (`.cshtml`), som genererer HTML.
- ViewModels overfører data mellem Controllers og Views og kan indeholde inputvalidering.

Controllers bør primært koordinere requests og anvende services frem for selv at implementere kompleks forretningslogik.

#### Application

Indeholder services, der udfører applikationens use cases.

En service kan eksempelvis hente relevante palledefinitioner og optimeringsindstillinger gennem repositories, kalde optimeringsalgoritmen og returnere resultatet til webapplikationen.

Laget fungerer dermed som bindeled mellem præsentationslaget, domænelogikken og datalaget.

#### Core

Indeholder systemets centrale forretningslogik.

Her placeres blandt andet optimeringsalgoritmen og de modeller og regler, den arbejder med.

Core skal kunne fungere uafhængigt af ASP.NET, Entity Framework Core og SQL Server, så algoritmen kan testes isoleret.

#### Infrastructure

Indeholder konkrete implementeringer af systemets eksterne afhængigheder, især persistens.

Projektet anvender Entity Framework Core til kommunikation med SQL Server.

### 1.2 MVC og trelagsarkitektur

Projektet kombinerer ASP.NET Core MVC med en lagdelt arkitektur.

MVC beskriver primært, hvordan webapplikationen håndterer brugerinteraktion, mens trelagsarkitekturen beskriver ansvarsfordelingen i hele applikationen.

| Trelagsarkitektur | Projekt | MVC-relation |
|---|---|---|
| Præsentationslag | Web | Controllers, Views og ViewModels |
| Logiklag | Application + Core | Forretningslogik og services |
| Datalag | Infrastructure | Persistente modeller, repositories og DbContext |

I ASP.NET Core MVC modtager en Controller en HTTP-request og bestemmer, hvordan den skal håndteres.

Controlleren kan kalde en application service, som benytter domænelogik og repositories. Derefter returnerer Controlleren typisk et View med de nødvendige data.

### 1.3 Projektreferencer

Projekterne er konfigureret med følgende afhængigheder:

```text
Web
 ├── Application
 └── Infrastructure

Application
 └── Core

Infrastructure
 ├── Application
 └── Core

Core
 └── Ingen interne projektreferencer

Tests
 ├── Application
 └── Core
```

Infrastructure kan implementere repository-interfaces defineret i Application eller Core. Dermed behøver Application og Core ikke kende de konkrete EF Core-implementationer.

Web refererer også til Infrastructure for at kunne registrere nødvendige services gennem Dependency Injection.

Denne struktur gør det muligt at udvikle og teste optimeringsalgoritmen uden at skulle starte SQL Server eller webapplikationen.

---

## 2. Teknologier og infrastruktur

| Teknologi | Version / anvendelse |
|---|---|
| .NET | .NET 10 |
| ASP.NET Core | MVC med Razor Views |
| Entity Framework Core | EF Core 10 |
| Database | Microsoft SQL Server 2022 Developer |
| Tests | xUnit |
| Containerisering | Docker og Docker Compose |
| Versionsstyring | Git |

### 2.1 SQL Server og Entity Framework Core

Projektet anvender Microsoft SQL Server til persistens.

SQL Server kører i en Docker-container frem for at være installeret direkte på udviklerens computer.

Entity Framework Core fungerer som ORM (Object-Relational Mapper) mellem C# og SQL Server.

Databasetilgangen konfigureres gennem `AppDbContext` i Infrastructure-projektet:

```text
PalletPacker.Infrastructure/
└── Data/
    ├── AppDbContext.cs
    └── Migrations/
```

ASP.NET registrerer DbContext gennem Dependency Injection.

Det gør det muligt for repositories at få en DbContext gennem constructor injection uden selv at skulle oprette databaseforbindelser.

### 2.2 Databaseoprettelse og migrations

EF Core Migrations anvendes til at oprette og vedligeholde databasestrukturen.

Migrations beskriver ændringer af databaseobjekter og gemmes som C#-filer i Infrastructure-projektet.

**Migrations genereres af udviklerne, men anvendes automatisk ved applikationens opstart.**

Applikationen anvender:

```csharp
await db.Database.MigrateAsync();
```

Det betyder, at EF Core ved opstart:

1. Opretter applikationsdatabasen, hvis den ikke eksisterer.
2. Undersøger hvilke migrations der allerede er anvendt.
3. Anvender manglende migrations.
4. Fortsætter opstarten.

SQL Server skal være tilgængelig, før migreringen kan lykkes. Applikationen har derfor en simpel retry-mekanisme til opstartssituationer, hvor databasen endnu ikke er klar.

EF Core registrerer anvendte migrations i tabellen `__EFMigrationsHistory`.

#### Oprettelse af nye migrations

Når en udvikler ændrer databasestrukturen gennem EF Core-modellerne, oprettes en ny migration:

```powershell
dotnet ef migrations add MigrationName --project src/PalletPacker.Infrastructure --startup-project src/PalletPacker.Web --output-dir Data/Migrations
```

Migrationsfilerne committes til Git sammen med kodeændringerne.

Andre udviklere og førstegangsbrugere behøver ikke generere migrations eller køre `dotnet ef database update`. De eksisterende migrations anvendes automatisk ved næste opstart.

Den første migration hedder `InitialCreate` og blev oprettet, før de egentlige domæne-entities blev implementeret.

---

## 3. Docker-opsætning

Docker anvendes til at køre SQL Server og, når det ønskes, ASP.NET-applikationen i containere.

Projektet kan derfor køres uden en lokal SQL Server-installation.

### 3.1 Docker Compose

Docker Compose-konfigurationen findes i:

```text
compose.yaml
```

Den definerer to services:

| Service | Container | Funktion |
|---|---|---|
| `database` | `palletpacker-db` | SQL Server 2022 |
| `web` | `palletpacker-web` | ASP.NET Core MVC |

Database-containeren startes som standard.

Web-containeren er tilknyttet Compose-profilen `full`, så den kun startes automatisk, når denne profil aktiveres, eller servicen eksplicit vælges.

Denne opdeling gør det muligt enten at køre hele systemet i Docker eller kun anvende Docker til databasen under lokal udvikling.

Der gøres brug af 2 images til dette projekt

#### SQL Server-image

```text
mcr.microsoft.com/mssql/server:2022-latest
```

Dette er Microsofts SQL Server 2022-image.

Det downloades automatisk første gang Compose starter databasen, hvis det ikke allerede findes lokalt.

SQL Server konfigureres som Developer Edition med adgangskoden fra `.env`.

#### ASP.NET-image

ASP.NET-imaget bygges lokalt ud fra:

```text
src/PalletPacker.Web/Dockerfile
```

Dockerfile anvender et multi-stage build med to trin:

**Build stage**

```text
mcr.microsoft.com/dotnet/sdk:10.0
```

Indeholder .NET SDK og bruges til at hente NuGet-pakker, kompilere og publicere applikationen.

**Runtime stage**

```text
mcr.microsoft.com/dotnet/aspnet:10.0
```

Indeholder ASP.NET Core Runtime og bruges til at køre den publicerede applikation.

Kun de nødvendige publicerede filer kopieres fra build-stage til runtime-stage. Det endelige image behøver derfor ikke indeholde hele .NET SDK'et.

### 3.3 Docker-netværk og porte

Docker Compose opretter et internt netværk, hvor containerne kan kommunikere ved hjælp af deres servicenavne.

SQL Server kan derfor tilgås fra web-containeren på:

```text
database:1433
```

Når ASP.NET køres direkte på Windows, bruges i stedet:

```text
tcp:127.0.0.1,14330
```

De anvendte portmappinger er:

| Service | Windows | Container |
|---|---|---|
| SQL Server | `127.0.0.1:14330` | `1433` |
| ASP.NET | `127.0.0.1:8080` | `8080` |

Portene er bundet til `127.0.0.1`, så de kun er eksponeret på den lokale computer.

Port `14330` anvendes på Windows for at undgå konflikter med eksisterende SQL Server-installationer, som typisk anvender port `1433`.

De to miljøer bruger forskellige adresser, fordi `localhost` inde i en container henviser til containeren selv og ikke til Windows-computeren.

### 3.4 Docker Volumes

SQL Server gemmer sine data i et navngivet Docker Volume:

```text
palletpacker_sqlserver-data
```

Det er monteret på:

```text
/var/opt/mssql
```

inde i SQL Server-containeren.

Dataene bevares, selvom containeren stoppes, fjernes og genskabes.

Det betyder eksempelvis, at databasen ikke slettes ved almindelig:

```powershell
docker compose down
```

**Advarsel:** Kommandoen nedenfor fjerner også projektets Docker Volumes og dermed databasesdataene:

```powershell
docker compose down -v
```

Brug kun denne kommando, hvis databasen bevidst skal nulstilles.

Et Docker Volume er persistent lagring, men ikke en erstatning for en backup.

### 3.5 Miljøvariabler og adgangskoder

Projektet bruger en lokal `.env`-fil til databaseadgangskoden.

Eksempel:

```dotenv
MSSQL_SA_PASSWORD=ReplaceWithStrongPassword123!
```

Ved første initialisering bruger SQL Server denne værdi til at konfigurere den eksisterende administratorbruger `sa`.

Adgangskoden bruges også til at opbygge ASP.NET-applikationens databaseforbindelse.

`.env` er udelukket fra Git gennem `.gitignore`.

I stedet versionsstyres `.env.example`, som fungerer som skabelon.

Bemærk, at ændring af adgangskoden i `.env` ikke automatisk ændrer adgangskoden i en eksisterende SQL Server-installation med persistent data.

---

## 4. Installation og første opstart

Projektet kan startes på en ny computer uden manuel installation af SQL Server eller .NET SDK, når hele applikationen køres gennem Docker.

### 4.1 Forudsætninger

Følgende skal være installeret:

- Git
- Docker Desktop med Docker Compose
- Docker Engine skal være startet
- Linux-containere skal være understøttet (WSL)

.NET SDK 10 er kun nødvendig, hvis applikationen skal udvikles eller køres direkte på computeren.

### 4.2 Hent projektet

Klon repository'et og gå til projektmappen:

```powershell
git clone https://github.com/EAMV-School-Work/PalletPacker
cd PalletPacker
```

### 4.3 Opret miljøkonfiguration

Kopiér `.env.example` til `.env`:

```powershell
Copy-Item .env.example .env
```

Åbn `.env` og vælg en stærk SQL Server-adgangskode:

```dotenv
MSSQL_SA_PASSWORD=YourStrongPassword123!
```

SQL Server kræver, at adgangskoden overholder dens kompleksitetskrav.

### 4.4 Start hele applikationen

Kør:

```powershell
docker compose --profile full up --build -d
```

Docker vil nu:

1. Hente de nødvendige images, hvis de mangler.
2. Bygge ASP.NET-applikationen.
3. Oprette netværk og persistent database-volume.
4. Starte SQL Server.
5. Starte ASP.NET-webapplikationen.
6. Anvende manglende EF Core migrations automatisk.

Når applikationen er startet, kan den åbnes på:

**http://localhost:8080**

Kontrollér containernes status med:

```powershell
docker compose --profile full ps
```

Ved første opstart kan initialiseringen tage længere tid, da images skal downloades og SQL Server skal initialiseres.

---

## 5. Udvikling og opstart

Projektet understøtter to opstartsmåder.

### 5.1 Lokal udvikling (anbefalet)

Under almindelig udvikling køres SQL Server i Docker, mens ASP.NET køres direkte på Windows.

Det gør det nemmere at anvende debugging, breakpoints og andre værktøjer i udviklingsmiljøet.

Følgende kræves:

- .NET SDK 10
- Docker Desktop
- PowerShell (Skulle gerne være der på enhver almindelig Windows PC)
- Lokal `.env`-fil

Start projektet fra rodmappen:

```powershell
.\run-dev.ps1
```

Scriptet:

1. Indlæser databaseadgangskoden fra `.env`.
2. Konfigurerer connection string til `tcp:127.0.0.1,14330`.
3. Starter SQL Server-containeren, hvis den ikke allerede kører.
4. Starter ASP.NET med `dotnet run`.

ASP.NET anvender derefter migrations automatisk ved opstart.

Med den nuværende launch-konfiguration kan applikationen åbnes på:

**http://localhost:5127**

Hvis PowerShell blokerer scriptet på grund af Execution Policy, kan det startes med:

```powershell
powershell -ExecutionPolicy Bypass -File .\run-dev.ps1
```

Hvis Docker-webcontaineren allerede kører, bør den stoppes inden lokal udvikling:

```powershell
docker compose stop web
```

### 5.2 Fuld Docker-opstart

Hele applikationen kan også køres gennem Docker:

```powershell
docker compose --profile full up --build -d
```

ASP.NET åbnes på:

**http://localhost:8080**

Hvis kildekoden ændres, skal web-imaget genbygges, før ændringerne er tilgængelige i Docker-containeren.

---

## 6. Docker-kommandoer

| Kommando | Beskrivelse |
|---|---|
| `docker compose up -d` | Starter standardsservices, normalt databasen |
| `docker compose --profile full up --build -d` | Bygger og starter hele systemet |
| `docker compose ps` | Viser containernes status |
| `docker compose logs web --tail 50` | Viser de seneste logs fra ASP.NET |
| `docker compose logs database --tail 50` | Viser de seneste logs fra SQL Server |
| `docker compose logs -f web` | Følger webapplikationens logs |
| `docker compose stop web` | Stopper web-containeren |
| `docker compose stop` | Stopper services uden at fjerne dem |
| `docker compose down` | Fjerner projektets containere og Compose-netværk, men bevarer volumes |
| `docker compose down -v` | Fjerner også volumes og dermed databasedata |

Bemærk, at `docker compose up -d` ikke nødvendigvis stopper en web-container, som allerede er startet med `full`-profilen.

Brug `docker compose stop web`, hvis der ønskes lokal udvikling uden den Docker-baserede webapplikation.

---

## 7. Tests

Projektet anvender xUnit til automatiserede tests.

Testprojektet findes i:

```text
tests/PalletPacker.Tests/
```

Kør alle tests fra projektets rodmappe:

```powershell
dotnet test
```

---