# ConferenceHub

A conference-room booking system built with .NET 10, exposing both an MVC web interface and a REST API over a shared domain and application layer.

## Overview

Business use cases:
- **Users** browse rooms with capacity/availability filters, book rooms for hourly slots with optional add-on services (catering, projector, etc.), and view their reservations.
- **Admins** manage the room and service catalog, view all reservations, and inspect utilization and revenue reports for arbitrary periods.

Pricing follows a **time-band model** — a room's hourly rate can differ per band (e.g. peak / off-peak). Services are billed with a **price snapshot** taken at booking time, so historical revenue reports remain accurate even if catalog prices change later.

## Tech Stack

- **.NET 10** — ASP.NET Core MVC (Web) + Web API (Api)
- **PostgreSQL 17** — via `Npgsql.EntityFrameworkCore.PostgreSQL`
- **EF Core** — code-first migrations, `timestamptz` with a UTC-only converter, cascade soft-delete query filters
- **ASP.NET Core Identity** — shared user store, roles (`Admin`, `User`), `AddIdentityCore + AddSignInManager` (so both cookie and JWT auth can coexist)
- **JWT auth (Api)** — access + refresh token rotation with reuse detection
- **Cookie auth (Web)** — `IdentityConstants.ApplicationScheme`, 7-day sliding expiration, HttpOnly + Secure + SameSite=Lax
- **FluentValidation** — API DTO validation (auto-discovered via `AddValidatorsFromAssembly`)
- **Data Annotations** — Web ViewModel validation (integrates with jQuery Unobtrusive Validation)
- **xUnit + FluentAssertions + NSubstitute + MockQueryable** — unit tests
- **Docker Compose** — only for the Postgres container (Api/Web run on the host)

## Architecture

Solution layout (Clean Architecture, no CQRS/MediatR — kept intentionally simple):

```
src/
├── ConferenceHub.Domain          — entities, value objects, domain rules (no dependencies)
├── ConferenceHub.Application     — services, DTOs, validators, interfaces (depends on Domain)
├── ConferenceHub.Infrastructure  — EF Core, repositories, seeders, auth-token generators
├── ConferenceHub.Api             — REST controllers, JWT auth, Swagger
└── ConferenceHub.Web             — MVC controllers, views, cookie auth
tests/
└── ConferenceHub.Tests           — xUnit tests for pricing, booking, reports
```

Key design decisions:
- **Repository<T> + UnitOfWork** — chosen over `IAppDbContext` for readability at code review; both patterns are valid.
- **Serializable transaction + retry loop** on `BookingService.CreateAsync` — prevents overlap-check race conditions under concurrent load (max 3 attempts, 50 ms × attempt backoff on Postgres `serialization_failure`).
- **Soft delete** via `IsDeleted` + EF Core `HasQueryFilter` — preserves history for reports.
- **Half-open time intervals** `[start, end)` — the standard pattern for scheduling; 12:00–14:00 does not conflict with 14:00–16:00.
- **Operating hours 06:00–23:00** enforced at validation time for booking start/end hours.
- **Multi-day bookings supported** — a reservation may span multiple days (e.g. day 1 20:00 → day 2 09:00). Nighttime hours (23:00–06:00) are skipped by `PricingCalculator` and excluded from utilization reports. The "billable hours" logic lives in a single iterator (`EnumerateBillableHours`) reused by both pricing and reporting.
- **Whole-hour bookings only** — enforced via `CreateReservationDtoValidator`.
- **Data Protection API** — default file-system key persistence in development; production deployments should configure a shared key store (Redis / file share / Azure Key Vault) for multi-instance auth.

## Getting Started

### Prerequisites
- .NET 10 SDK
- Docker + Docker Compose
- (optional) `dotnet-ef` CLI: `dotnet tool install --global dotnet-ef`

### 1. Configure secrets

```bash
cd src
cp .env.example .env
# edit .env — set POSTGRES_PASSWORD and JWT_KEY (min 32 chars)
```

### 2. Start Postgres

```bash
cd src
docker compose up -d
docker compose ps   # confirm postgres is healthy
```

Postgres listens on host port **5434** (mapped from container 5432) to avoid clashing with a local instance.

### 3. Run the API

```bash
cd src/ConferenceHub.Api
dotnet run --launch-profile http
```

> **Note on secrets:** `Jwt__Key` is injected via `launchSettings.json` environment variables in development. Always run the Api with a launch profile (`--launch-profile http` or `https`) — a plain `dotnet run --no-launch-profile` will start but return **500** on any authenticated request because the JWT signing key is missing. For production, replace `launchSettings.json` with user secrets, environment variables, or a secrets manager.

On startup the Api will:
1. Apply pending EF Core migrations automatically
2. Seed roles (`Admin`, `User`)
3. Seed a default admin user (`admin@ch.local` / `Admin123!`)
4. Seed 6 services (Projector, Whiteboard, Video conferencing, Coffee break, Flipchart, Sound system) and 8 rooms with 1–4 random amenities each

> **Note on seed data:** the specification lists 3 illustrative rooms and 3 services as example initial data. The seeder deliberately generates a larger, more realistic dataset via [Bogus](https://github.com/bchavez/Bogus) with a fixed random seed (`42`) so results are deterministic across runs. The wider variety gives the utilization and revenue reports meaningful data to display; the exact spec rows are not required for correctness.

Swagger UI: **http://localhost:5150/swagger** (or **https://localhost:7025/swagger**).

### 4. Run the Web project

In a separate terminal:
```bash
cd src/ConferenceHub.Web
dotnet run
```

Web UI: **http://localhost:5144** (or **https://localhost:7254**).

## Default Credentials

| Role  | Email             | Password    |
|-------|-------------------|-------------|
| Admin | admin@ch.local    | Admin123!   |

Override via `Seed:AdminEmail` / `Seed:AdminPassword` in `appsettings.json` (or user secrets) if desired.

To create a second admin: register a normal user through `/account/register`, then either (a) assign the `Admin` role directly in the database, or (b) extend `IdentitySeeder` with additional users.

## API

### Auth flow

```
POST /api/auth/register  { email, password, userName }         → 200
POST /api/auth/login     { email, password }                   → { accessToken, refreshToken }
POST /api/auth/refresh   { refreshToken }                      → { accessToken, refreshToken }  (rotation)
POST /api/auth/logout    { refreshToken }                      → 204
```

- **Access token** — 15 min, sent as `Authorization: Bearer <token>`
- **Refresh token** — 7 days, rotated on every `/refresh`. Reusing a revoked refresh token revokes **all** tokens of that user (reuse-detection).

### Main endpoints

| Method | Path                          | Auth       | Purpose                         |
|--------|-------------------------------|------------|---------------------------------|
| GET    | `/api/rooms`                  | anonymous  | List rooms + search filter      |
| GET    | `/api/rooms/{id}`             | anonymous  | Room details + amenities        |
| POST   | `/api/rooms`                  | Admin      | Create room                     |
| PUT    | `/api/rooms/{id}`             | Admin      | Update room                     |
| DELETE | `/api/rooms/{id}`             | Admin      | Soft-delete room                |
| GET    | `/api/services`               | anonymous  | List services                   |
| POST/PUT/DELETE `/api/services/...` | Admin | Service CRUD                    |
| POST   | `/api/reservations`           | User       | Create reservation              |
| GET    | `/api/reservations/mine`      | User       | My reservations                 |
| GET    | `/api/reservations`           | Admin      | All reservations                |
| GET    | `/api/reports/utilization`    | Admin      | Room hours booked vs available  |
| GET    | `/api/reports/revenue`        | Admin      | Revenue grand total + breakdowns|

Validation errors return **400 ProblemDetails** (via `ValidationExceptionHandler`). Overlap conflicts return **409**.

## Web

| URL                  | Role       | Purpose                            |
|----------------------|------------|------------------------------------|
| `/`                  | anonymous  | Home                               |
| `/account/login`     | anonymous  | Login                              |
| `/account/register`  | anonymous  | Register                           |
| `/rooms`             | anonymous  | Browse + filter rooms              |
| `/rooms/details/{id}`| anonymous  | Room details                       |
| `/reservations/book/{roomId}` | User | Book a room (live price preview + billable hours) |
| `/reservations/mine` | User       | My reservations                    |
| `/admin`             | Admin      | Admin hub (Reports / Rooms / Services) |
| `/reports`           | Admin      | Utilization + revenue reports      |
| `/adminrooms`        | Admin      | Rooms CRUD                         |
| `/adminservices`     | Admin      | Services CRUD                      |

## Testing

```bash
# Unit tests (no Docker required)
dotnet test tests/ConferenceHub.Tests

# Integration tests (requires Docker — Testcontainers spins up Postgres automatically)
dotnet test tests/ConferenceHub.IntegrationTests

# All tests
dotnet test src/ConferenceHub.sln
```

Coverage focuses on business-critical paths:
- **PricingCalculatorTests** — time-band pricing, cross-band bookings, boundary hours
- **BookingServiceTests** — overlap detection, boundary touches, price snapshot, service validation
- **ReportServiceTests** — utilization percent, revenue aggregation, out-of-period exclusion, service grouping
- **BookingServiceIntegrationTests** — concurrent booking race condition (Serializable isolation), boundary touch, overlapping slot against a real Postgres 17 instance

## Known Limitations / Backlog

- **Timezone handling** — currently all timestamps are treated as UTC end-to-end (no per-user timezone). Adequate for a single-region deployment; a production system should convert at the boundary using `TimeZoneInfo`.
- **Admin cancel / user cancel** — reservations cannot be cancelled after creation; adding `ReservationStatus` cascades into the overlap-check and reports (see backlog notes).
- **Room-services editing** — the room ↔ service join table is populated only by the seeder; admin UI does not currently let you edit the amenity list per room after creation.
## License

Test task — not licensed for redistribution.
