# Padel Court Booking Platform

A full-stack padel court booking system: customers book by date/time without an account,
admins manage courts, hours, closures, pricing, offers, and bookings behind JWT auth.

**Tech stack:** ASP.NET Core 8 Web API · React (Vite) · Entity Framework Core ·
SQL Server (production) / SQLite (local dev fallback) · JWT auth · Thawani payment integration

---

## Admin login

```
URL:      http://localhost:5173/admin/login
Username: admin
Password: Admin@123
```

This account is auto-seeded the first time the API runs — no manual setup needed.

---

## Project structure

```
backend/
  PadelBooking.sln
  src/
    PadelBooking.Domain/          # Entities, enums — no external dependencies
    PadelBooking.Application/     # DTOs, interfaces, business logic services
    PadelBooking.Infrastructure/  # EF Core, JWT, password hashing, Thawani client
    PadelBooking.Api/             # Controllers, Program.cs, appsettings

frontend/
  src/
    pages/customer/               # Booking flow, lookup, payment callback
    pages/admin/                  # Login, bookings, courts, hours, closures, prices, offers
    components/, context/, api/
```

Clean architecture: `Api → Infrastructure → Application → Domain`. Domain has zero
framework dependencies; Application depends only on Domain; Infrastructure implements
Application's interfaces (`IAppDbContext`, `IJwtTokenGenerator`, etc.).

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) 18+ and npm
- A SQL Server instance **or** just use the SQLite fallback below (zero install)

---

## Backend setup

```bash
cd backend
dotnet restore
dotnet build
```

### Database

The project supports two providers, switched by one setting — same entities, same
migrations, same code either way.

**Option A — SQLite (default, zero install, good for local dev/demo):**

Already configured in `src/PadelBooking.Api/appsettings.json`:
```json
"DatabaseProvider": "Sqlite",
"ConnectionStrings": { "SqliteConnection": "Data Source=padelbooking.db" }
```
Nothing further to do — the database file and schema are created automatically on first run.

**Option B — SQL Server (production target per spec):**

1. Update the connection string in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost,1433;Database=PadelBookingDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
   }
   ```
2. Change the provider flag:
   ```json
   "DatabaseProvider": "SqlServer"
   ```
3. Apply migrations:
   ```bash
   cd src/PadelBooking.Infrastructure
   dotnet ef database update --startup-project ../PadelBooking.Api
   ```

### Run the API

```bash
cd src/PadelBooking.Api
dotnet run
```

- API: `http://localhost:5000`
- Swagger UI: `http://localhost:5000/swagger`

On first run, the API automatically applies migrations and seeds:
- 1 admin account (`admin` / `Admin@123`)
- 3 courts ("Court 1", "Court 2", "Court 3")
- Working hours: every day, 08:00–23:00
- 1 default price rule: 6.000 OMR/hour

---

## Frontend setup

```bash
cd frontend
npm install
npm run dev
```

Runs at `http://localhost:5173`. The Vite dev server proxies `/api` requests to
`http://localhost:5000`, so both servers need to be running at the same time.

- Customer booking: `http://localhost:5173/`
- Booking lookup: `http://localhost:5173/lookup`
- Admin: `http://localhost:5173/admin/login`

---

## Thawani online payment

The Thawani checkout integration (`PadelBooking.Infrastructure/ExternalServices/Thawani`)
is implemented against Thawani's real Checkout API (`uatcheckout.thawani.om`) — session
creation, redirect URL construction, and server-side payment verification on callback.

**To actually exchange traffic with Thawani, real UAT sandbox credentials are required.**
Register for a Thawani merchant sandbox account, then fill in `appsettings.json`:
```json
"Thawani": {
  "PublishableKey": "your_uat_publishable_key",
  "SecretKey": "your_uat_secret_key"
}
```
Without real credentials, "Pay on arrival" bookings work end-to-end as normal; Thawani
checkout session creation will fail gracefully with a clear error message instead of a crash.

---

## Key design decisions

- **No court names shown to customers** — the public API/UI only ever return time-slot
  availability, never court identity. Court assignment happens server-side, randomly,
  only at the moment a booking is created.
- **Race-condition safety** — a `BookingSlots` table has a database-level **unique
  constraint** on (court, date, hour). Even under concurrent requests for the last
  available slot, the database itself rejects the second attempt; the app automatically
  retries with a different court rather than surfacing the failure to the user.
- **Pricing is always computed server-side** at booking time from active `PriceRule`s
  and `Offer`s — the client never dictates price.
- **No customer accounts** — bookings are looked up later via a short reference code
  (e.g. `PB-7F3K2Q`), not a login.

---

## Default admin-seeded data

| Courts | Working hours | Price |
|---|---|---|
| Court 1, 2, 3 | Every day 08:00–23:00 | 6.000 OMR/hour (default rate) |

Adjust all of this from the admin panel after logging in.
