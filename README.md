# FamilyTools

Suite d'outils familiaux auto-hébergée. Module actuel : **EasyCompta**, une application
de comptabilité familiale qui importe les relevés bancaires (CSV) et répartit les
dépenses entre les membres du foyer.

## Architecture

Solution **.NET Aspire (.NET 10)** orchestrant plusieurs services + un front Angular.

| Projet | Rôle |
|---|---|
| `FamilyTools.AppHost` | Orchestrateur Aspire (SQL Server, Redis, RabbitMQ, migrations, API, worker, front) |
| `FamilyTools.EasyCompta` | API REST (Controllers → Business → Data) ; publie les imports CSV dans RabbitMQ et notifie le front en temps réel (SignalR) |
| `FamilyTools.EasyCompta.Business` | Couche métier partagée (Business + interfaces), réutilisée par l'API et le worker |
| `FamilyTools.CsvWorker` | Worker dédié : consomme la file RabbitMQ, parse les CSV, persiste les écritures, puis publie un événement de fin |
| `FamilyTools.Data` | EF Core : `DbContext`, entités, migrations, seed |
| `FamilyTools.MigrationService` | Worker appliquant les migrations au démarrage |
| `FamilyTools.ServiceDefaults` | OpenTelemetry, health checks, service discovery, résilience |
| `FamilyTools.Web` | SPA Angular 21 (standalone, signals, OnPush) |

### Flux métier (EasyCompta)

`CSV bancaire → CSVCA → AccountEnter (écriture) → AccountLine (part par membre) → AccountPage (mois)`,
catégorisé par `AccountTag` et `OperationType`, avec suivi des règlements (`PaymentDone`).

## Prérequis

- .NET SDK 10
- Node.js 20+
- Docker (SQL Server + Redis + RabbitMQ lancés par Aspire)

## Lancer en développement

```bash
dotnet run --project FamilyTools.AppHost
```

Le dashboard Aspire orchestre la base, les migrations, l'API et le front.
Pour lancer le front seul : `cd FamilyTools.Web && npm install && npm start`.

## Tests

- Back : `dotnet test FamilyTools.sln`
- Front : `cd FamilyTools.Web && npm test`

## Structure du front (Angular)

Organisation **par feature**, avec lazy-loading :

- `core/` — singletons applicatifs (http helper, interceptor d'erreurs, notifications, config)
- `shared/` — modèles transverses + UI réutilisable
- `easycompta/`, `user/` — features (composants, `data/`, `models/`, routes)

Alias TypeScript : `@core`, `@shared`, `@easycompta`, `@user`.
