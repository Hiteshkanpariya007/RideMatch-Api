# RideMatch API

A .NET 10 REST API for rider and driver registration, ride requests, nearby-driver matching, and a driver ride lifecycle.

## Repository and API collection

- GitHub: [Hiteshkanpariya007/RideMatch-Api](https://github.com/Hiteshkanpariya007/RideMatch-Api)
- Postman collection: [RideMatch-Api.postman_collection.json](RideMatch-Api.postman_collection.json)
- Swagger UI (Development): <https://localhost:7158/swagger/index.html>

## Architecture overview

```text
HTTP clients / Postman / Swagger
              |
              v
RideMatch-Api: controllers, JWT authorization, validation, middleware, SignalR hub
              |
              v
Services: auth, driver and rider workflows, repositories, matching worker
              |
              v
Data: EF Core DbContext, SQL Server entities and migrations
              |
              v
SQL Server                         Redis (production)
```

`Common` contains shared enums, options, constants, and exceptions. Controllers delegate rider and driver operations to services, which use repositories for persistence. Ride creation queues work for a hosted matching worker. The matching service reads candidate locations and assigns a driver in a SQL transaction. SignalR publishes ride events through `/hubs/ride`.

## Requirements

- .NET 10 SDK
- SQL Server / SQL Server Express
- `dotnet-ef` CLI for applying migrations (EF Core 10)
- Redis for non-Development deployments; local Development uses an in-memory implementation by default

If the EF CLI is not installed, install the version matching the project:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.12
```

## Setup and run

Run commands from the repository root in PowerShell.

1. Clone the repository and enter the folder:

   ```powershell
   git clone https://github.com/Hiteshkanpariya007/RideMatch-Api.git
   cd RideMatch-Api
   ```

2. Restore packages and set a local JWT signing secret. Use your own random value with at least 32 characters; User Secrets keeps it out of source control.

   ```powershell
   dotnet restore RideMatch-Api.slnx
   dotnet user-secrets set "Jwt:SecretKey" "replace-with-your-own-random-secret-at-least-32-chars" --project RideMatch-Api/RideMatch-Api.csproj
   ```

3. Ensure the SQL Server instance in `RideMatch-Api/appsettings.json` is running and accessible. The checked-in Development connection string targets `localhost\SQLEXPRESS` and uses Windows Integrated Security. Update `ConnectionStrings:RideMatchDB` if your instance differs.

4. Apply the included EF Core migration:

   ```powershell
   dotnet ef database update --project Data/Data.csproj --startup-project RideMatch-Api/RideMatch-Api.csproj
   ```

5. Run the API:

   ```powershell
   dotnet run --project RideMatch-Api/RideMatch-Api.csproj --launch-profile https
   ```

   The HTTPS profile listens on `https://localhost:7158` (HTTP: `http://localhost:5224`). Swagger is enabled in Development. Health endpoints are `/health/live` and `/health/ready`.

   If the local HTTPS certificate is not trusted, run `dotnet dev-certs https --trust` once, then restart the API.

### Redis configuration

`RideMatch-Api/appsettings.Development.json` enables `Redis:UseInMemory`. This lets a single local API process run without installing Redis. The in-memory values and matching queue are lost when the process stops and are not shared between API instances.

For production or multi-instance use, set `Redis:UseInMemory` to `false` and configure `ConnectionStrings:Redis` to point to a reachable Redis server. Do not use the in-memory provider for deployment.

## API and Postman flow

The Postman collection uses `https://localhost:7158` as `baseUrl`. Import `RideMatch-Api.postman_collection.json` and run its folders sequentially:

1. **Health and Swagger**
2. **Authentication** - registers rider and driver accounts and saves their JWTs as collection variables.
3. **Driver Setup** - log in, go online, and publish a recent driver location.
4. **Rider Ride Create and Read** - create a ride near the driver and read it as the rider.
5. **Driver Ride Workflow** - wait for `DriverAssigned`, then accept, arrive, start, and complete the ride.

The matcher requires a driver to be online, available, and have a recent location (30-second freshness window). It searches from 5 km out to 20 km, expanding the radius in 5 km steps, and considers the nearest candidates first. Location updates queue still-searching rides for another match attempt. A driver with an unfinished assigned ride is unavailable until that ride is completed or cancelled.

Core routes:

| Purpose | Routes |
| --- | --- |
| Authentication | `POST /api/Auth/register`, `POST /api/Auth/login` |
| Driver | `/api/v1/drivers/online`, `/location`, `/me`, `/rides` |
| Driver ride lifecycle | `/api/v1/drivers/rides/{rideId}/accept`, `/arrived`, `/start`, `/complete` |
| Rider rides | `POST` and `GET /api/v1/riders/rides`; `GET /api/v1/riders/rides/{rideId}`; `POST /api/v1/riders/rides/{rideId}/cancel` |
| Health | `GET /health/live`, `GET /health/ready` |
| Real-time | SignalR hub `/hubs/ride` |

Protected routes use JWT bearer authentication and role authorization (`Rider` or `Driver`). In Swagger, click **Authorize** and provide the login token. In Postman, the collection saves tokens after successful registration/login and uses them for the corresponding role folders.

## Assumptions and tradeoffs

- SQL Server is the durable store for users, profiles, rides, and driver availability/location fields.
- The Development in-memory Redis substitute is process-local and intended for a single-instance interview/demo run. Production uses Redis through `IRedisService`.
- The ride queue is an in-process `Channel<Guid>`. It keeps the example simple, but queued work is not durable across restarts and is not shared across API instances.
- Matching uses a bounding-box query followed by Haversine distance calculation; the current configurable search radius is capped at 20 km.
- Driver locations are considered fresh for 30 seconds. Location updates are rate-limited to 60 requests per minute per authenticated driver (or client IP when unauthenticated).
- Matching and assignment are asynchronous. A newly created ride may initially return `Searching`; poll rider or driver history until it becomes `DriverAssigned` before accepting it.
- The matching service uses EF Core directly for candidate selection and the transactional assignment; API request workflows otherwise use services and repositories.

## Scalability considerations

- Replace the in-process matching channel with a durable broker (for example, Azure Service Bus, RabbitMQ, or Kafka), and make workers safe to run on multiple instances. Use an outbox or equivalent so a committed ride and its match message cannot drift apart.
- Use shared Redis for distributed idempotency and live driver availability/location, with TTLs and atomic operations. Keep SQL Server as the source of truth for durable ride state.
- Add spatial indexes and/or SQL Server spatial queries as location volume grows; review indexes and transaction contention around driver assignment.
- Use Azure SignalR or a Redis backplane when scaling SignalR across instances.
- Move rate-limit state to a distributed store if the API runs on multiple instances; the current ASP.NET Core limiter is process-local.
- Add metrics and tracing for queue age, match attempts, candidate counts, assignment conflicts, and stale driver locations.
