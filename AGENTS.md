# AGENTS.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Build & Run

Build the entire solution:
```
dotnet build ScienceApp.slnx
```

Build only the API project:
```
dotnet build ScienceApp.RandomExperimentApi/ScienceApp.RandomExperimentApi.csproj
```

Run the API locally (requires MongoDB to be reachable at the connection string in `app.json`):
```
dotnet run --project ScienceApp.RandomExperimentApi
```
Local API URL: `http://localhost:5577`

## Docker

Spin up the full stack (infrastructure + API):
```
docker compose up
```

Start only infrastructure (MongoDB, PostgreSQL, Kafka, Redis) for local development:
```
docker compose -f docker-compose.DbOnly.yaml up
```

Deploy only the pre-built API image:
```
docker compose -f docker-compose.ServiceOnly.yaml up
```

The `.env` file at the root defines all ports and credentials used by Compose. Default mappings:
- API: `5577→8080`
- MongoDB: `27018→27017`
- PostgreSQL: `3434→3432`
- Redis: `6378→6379`

## Architecture

### Solution Projects

The solution (`ScienceApp.slnx`) is a .NET 10 multi-project library. The only runnable project is `ScienceApp.RandomExperimentApi`; everything else is a class library.

| Project | Role |
|---|---|
| `ScienceApp.RandomExperimentApi` | ASP.NET Core Minimal API — entry point, wires up DI, Hangfire, endpoints |
| `ScienceApp.RandomHistService` | Domain logic, models, and domain repositories |
| `ScienceApp.Dto` | Shared request/response DTOs (`RandomExperimentSetDto`, `SendExperimentOptionsDto`) |
| `DbInterfaces` | Abstractions: `IBaseRepository<T>`, `IEntity`, `ISpecification<T>` |
| `MongoDbBaseRepository` | Generic `MongoBaseRepository<T>` implementing `IBaseRepository<T>` |
| `RedisBaseRepository` | `RedisRepository` wrapping StackExchange.Redis (String, Hash, List, Set, SortedSet, Stream, Geo) |
| `EFBaseRepository` | EF Core `EfRepository` — exists but not yet wired into the API |
| `CommonHelper` | `PagingExtensions` (paging + page-iteration), `CopyHelper`, `ExceptionExtension.GetAllMessages()` |

### Data Flow

1. An HTTP request hits a Minimal API endpoint in `RandomExperimentEndPoint.cs` (route prefix `/api/random-experiment`).
2. The endpoint delegates to `IRandomExperimentSetRepository`.
3. `RandomExperimentSetRepository` extends `MongoBaseRepository<RandomExperimentSet>` and adds domain-specific queries (date-range filters, histogram backfill).
4. `RandomCalc.Calc()` produces a `RandomExperimentSet` (raw integer list), then `getAggregateResult()` calls `RangeSplitter.SplitRange()` to bucket it into histogram ranges before persisting.

### Background Jobs (Hangfire)

Hangfire is backed by MongoDB (database `JobTask`, prefix `todo.hangfire`). A recurring job runs `IRandomExperimentSetRepository.CalcExperiment()` on the cron schedule defined in `app.json` (`HangfireSettings.Cron`, default `*/5 * * * *`). The dashboard is exposed at `/hangfire`.

### Configuration

Runtime config is loaded in priority order: **environment variables → `appsettings.json` → `app.json`**.

Key sections in `app.json`:
- `Database.ConnectionString` — MongoDB connection used for both business data and Hangfire storage.
- `ExperimentSettings` — bound to `ExperimentSettings` options class; controls `CountCalc`, `MinimalValue`, `MaximalValue`, `RangeValue`, `DatabaseName`, `CollectionName`.
- `HangfireSettings` — `DatabaseName`, `Prefix`, `ServerName`, `RecurringJobId`, `Cron`.

`BsonSerializer.RegisterSerializer(DateTimeSerializer.LocalInstance)` is applied globally — all `DateTime` fields are stored and read as local time.

### DI Registration

`ServiceRegistration.AddRandomHistServiceDependencies()` (in `ScienceApp.RandomHistService`) is the single extension method that registers all service-layer dependencies. New repositories or services should be added there, not in `Program.cs`.

## Known Quirks

- The MongoDB repository namespace is `MangoDbBaseRepository` (note "Mango", not "Mongo") — this is consistent throughout the codebase and must be preserved.
- The model namespace contains a typo: `ScientificApp.RandomHistSerice.Model` ("Serice" instead of "Service") — likewise consistent and intentional to preserve until explicitly renamed.
- There are no test projects in the solution.
