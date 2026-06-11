# EventosVivos — Sistema de Reservas

Núcleo del sistema de reservas para **EventosVivos**: creación de eventos, control de aforo en tiempo real, gestión de reservas/pagos y reportes de ocupación, con las reglas de negocio del enunciado implementadas y probadas.

- **Backend:** .NET 8 (ASP.NET Core Web API) · Clean Architecture + CQRS (MediatR) · EF Core + SQLite
- **Frontend:** Angular 18 (standalone components, signals, formularios reactivos)
- **Pruebas:** xUnit (unitarias de dominio + integración de handlers contra SQLite real)

## 🔗 Demo en vivo

- **API (Render):** https://eventosvivos-reservas.onrender.com — [Swagger](https://eventosvivos-reservas.onrender.com/swagger) · [/api/venues](https://eventosvivos-reservas.onrender.com/api/venues)
- **Frontend (Vercel):** _pendiente_
- **Base de datos:** PostgreSQL en Supabase

> El backend usa el plan gratuito de Render y se suspende tras inactividad: el primer request puede tardar ~50 s en responder.

---

## 1. Stack tecnológico

| Capa | Tecnología | Por qué |
|---|---|---|
| API | ASP.NET Core 8 (controllers) | Estándar, RESTful, OpenAPI/Swagger integrado |
| Orquestación | MediatR (CQRS) | Separa comandos/consultas, handlers pequeños y testeables |
| Validación | FluentValidation + pipeline behavior | Validación de entrada centralizada y declarativa |
| Persistencia | EF Core 8 + SQLite | Cero infraestructura, corre con un comando, persistencia real |
| Dominio | C# puro (sin dependencias) | Reglas de negocio aisladas y 100% testeables |
| Frontend | Angular 18 standalone | Última versión, sin NgModules, signals y control flow `@if/@for` |

---

## 2. Arquitectura

Se eligió **Clean Architecture** con **CQRS**. La dependencia apunta siempre hacia el dominio:

```
┌───────────────────────────────────────────────┐
│                 EventosVivos.Api                │  Controllers, middleware de errores, Swagger, CORS
│  depende de Application + Infrastructure        │
├───────────────────────────────────────────────┤
│             EventosVivos.Application            │  Commands/Queries (MediatR), DTOs, validators,
│  depende de Domain                              │  interfaces (IAppDbContext, IDateTimeProvider…)
├───────────────────────────────────────────────┤
│            EventosVivos.Infrastructure          │  AppDbContext (EF Core/SQLite), configuraciones,
│  depende de Application + Domain                │  seed, generador de códigos, reloj del sistema
├───────────────────────────────────────────────┤
│               EventosVivos.Domain               │  Entidades, value objects, enums y políticas
│  sin dependencias                               │  de negocio (reglas puras)
└───────────────────────────────────────────────┘
```

### ¿Por qué esta arquitectura?

- **El dominio no conoce a EF ni a ASP.NET.** Las invariantes (RN01, RN03, máquina de estados de la reserva, RN07…) viven en entidades y *policies* puras, lo que las hace triviales de probar sin base de datos ni servidor.
- **CQRS con MediatR** mantiene cada caso de uso en su propio handler pequeño (un archivo por operación), con un *pipeline behavior* que ejecuta la validación antes de llegar al handler. Es fácil de leer, extender y testear.
- **Inversión de dependencias:** la capa de aplicación depende de `IAppDbContext`, no de `AppDbContext`. Cambiar SQLite por PostgreSQL es un cambio localizado en Infrastructure.
- No es la única arquitectura válida; para un CRUD trivial sería *over-engineering*, pero el enunciado pondera explícitamente la **decisión de diseño** y la cantidad de reglas de negocio justifica aislar el dominio.

### Estructura

```
backend/
  EventosVivos.sln
  src/
    EventosVivos.Domain/         # Entities, ValueObjects, Enums, Services (policies), Common
    EventosVivos.Application/     # Events/, Reservations/, Reports/, Venues/, Common (behaviors, interfaces)
    EventosVivos.Infrastructure/  # Persistence (DbContext, Configurations, Converters, Seeder), Services
    EventosVivos.Api/             # Controllers, Middleware, Contracts, Program.cs
  tests/
    EventosVivos.Domain.UnitTests/
    EventosVivos.Application.IntegrationTests/
frontend/
  src/app/core/                   # models, services (ApiService, NotificationService), interceptors
  src/app/features/events/        # event-list, event-form, event-detail
docker-compose.yml
```

---

## 3. Cómo ejecutar

### Requisitos
- .NET SDK 8.0+
- Node.js 20+ (para el frontend)
- (Opcional) Docker + Docker Compose

### Backend

```bash
cd backend
dotnet restore
dotnet run --project src/EventosVivos.Api
```

La API queda en `http://localhost:5080` y Swagger en `http://localhost:5080/swagger`.
La base SQLite (`eventosvivos.db`) y los 3 venues de referencia se crean automáticamente al arrancar.

### Frontend

```bash
cd frontend
npm install
npm start
```

La app queda en `http://localhost:4200` y consume la API en `http://localhost:5080/api`
(configurable en `src/environments/environment.ts`).

### Con Docker (todo junto)

```bash
docker compose up --build
```

- Frontend: `http://localhost:4200` (Nginx sirve el build y hace proxy de `/api` al backend)
- Backend: `http://localhost:8080`

### Pruebas

```bash
cd backend
dotnet test
```

### Despliegue (nube)

Stack desplegado: **Vercel** (frontend Angular) + **Render** (API .NET en Docker) + **Supabase** (PostgreSQL). El proxy de Vercel reenvía `/api/*` a Render, así que frontend y API quedan en el mismo origen (sin CORS).

El cambio SQLite → PostgreSQL es solo configuración (no se toca el dominio):

```
Database__Provider=Postgres
ConnectionStrings__Default=Host=db.<ref>.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=...;SSL Mode=Require;Trust Server Certificate=true
```

Paso a paso completo en **[DEPLOY.md](DEPLOY.md)**.

---

## 4. API REST

Base: `/api`. Los enums se serializan/leen como strings en *camelCase* (`conferencia`, `activo`, `pendientePago`…).

| Método | Ruta | Descripción | RF |
|---|---|---|---|
| POST | `/api/events` | Crear evento | RF-01 |
| GET | `/api/events` | Listar con filtros `type, venueId, status, title, startFromUtc, startToUtc` | RF-02 |
| GET | `/api/events/{id}` | Detalle de evento | — |
| GET | `/api/events/{id}/report` | Reporte de ocupación | RF-06 |
| GET | `/api/events/{id}/reservations` | Reservas de un evento | — |
| POST | `/api/reservations` | Reservar entradas | RF-03 |
| POST | `/api/reservations/{id}/confirm` | Confirmar pago | RF-04 |
| POST | `/api/reservations/{id}/cancel` | Cancelar reserva | RF-05 |
| GET | `/api/venues` | Catálogo de venues | — |

### Contrato de errores (RFC 7807 / ProblemDetails)

Todos los errores se devuelven con un cuerpo `application/problem+json` consistente:

- **400** validación de entrada → incluye `errors` (campo → mensajes).
- **404** recurso inexistente.
- **409** violación de una regla de negocio → incluye `code` estable (p. ej. `EVENT_VENUE_OVERLAP`, `EVENT_INSUFFICIENT_CAPACITY`, `RESERVATION_ALREADY_CONFIRMED`).

Ejemplo:

```json
{
  "title": "Conflicto con una regla de negocio",
  "status": 409,
  "detail": "El venue ya tiene un evento activo en un horario que se superpone.",
  "code": "EVENT_VENUE_OVERLAP"
}
```

---

## 5. Mapeo de requerimientos y reglas

| Regla | Dónde se implementa |
|---|---|
| RF-01 Crear evento | `Event.Create()` + `CreateEventCommandHandler` |
| RF-02 Listar con filtros | `ListEventsQueryHandler` |
| RF-03 Reservar (incl. máx. 5 si <24h) | `CreateReservationCommandHandler` + `ReservationPolicy` |
| RF-04 Confirmar pago (código `EV-######`) | `ConfirmReservationPaymentCommandHandler` + `Reservation.ConfirmPayment()` |
| RF-05 Cancelar reserva | `CancelReservationCommandHandler` + `Reservation.Cancel()` |
| RF-06 Reporte de ocupación | `GetOccupancyReportQueryHandler` |
| RN01 Capacidad ≤ venue | `Event.Create()` |
| RN02 Superposición de venue | `CreateEventCommandHandler` (`AnyAsync` con overlap) + `SchedulingPolicy.Overlaps` |
| RN03 Weekend después de 22:00 | `Event.Create()` |
| RN04 Reserva <1h antes | `ReservationPolicy.EnsureReservationWindowOpen()` |
| RN05 Precio >$100 → máx 10 | `ReservationPolicy.MaxTicketsPerTransaction()` |
| RN06 Completado automático | `Event.GetEffectiveStatus()` (estado derivado) |
| RN07 Cancelación con penalización (perdida) | `Reservation.Cancel()` |

---

## 6. Decisiones de diseño y supuestos

El enunciado deja varios casos borde abiertos. Estas son las decisiones tomadas y su justificación:

1. **Inventario derivado del estado de las reservas.** El aforo disponible se calcula como `capacidad − (pendientes + confirmadas + perdidas)`. Solo las reservas **canceladas** liberan entradas. Esto evita un contador mutable propenso a inconsistencias: "liberar entradas" (RF-05) es simplemente cambiar el estado a `cancelada`.

2. **RN07 — reservas "perdidas".** Cancelar una reserva **confirmada** con menos de 48h pasa a estado `Perdida`: **no** libera entradas (siguen ocupando aforo) y aparece en el reporte. Se modeló como un estado propio en lugar de un flag para que la máquina de estados sea explícita.

3. **RF-05 con redacción contradictoria.** El enunciado dice "cambiar de confirmada a cancelada" pero también "rechazar si está confirmada". Se interpretó lo coherente con el negocio: se puede cancelar una reserva `pendiente_pago` o `confirmada`; se rechaza solo si ya está `cancelada`/`perdida` (`RESERVATION_ALREADY_CLOSED`).

4. **Ingresos del reporte.** Se siguió la fórmula literal del enunciado: `ingresos = precio × entradas confirmadas`. Las "perdidas" se reportan aparte (`ticketsLost`) sin sumar a ingresos.

5. **% de ocupación** = `(pendientes + confirmadas + perdidas) / capacidad`, coherente con "entradas disponibles restantes".

6. **Límites por transacción acumulables.** Si aplican RN05 (>$100 → 10) y la regla de <24h (→ 5) a la vez, gana **el más estricto** (5).

7. **Fechas en UTC.** Toda fecha/hora se maneja y persiste en UTC (un *value converter* re-estampa el `Kind` al leer de SQLite). RN03 se evalúa sobre el valor almacenado.

8. **Estado del evento parcialmente derivado.** `completado` (RN06) se calcula al vuelo según la hora actual; por eso el filtro por estado se resuelve en memoria tras consultar.

9. **Concurrencia / overselling.** La verificación de aforo y el alta de la reserva ocurren en el handler. Para el alcance de la prueba (SQLite, escrituras serializadas) es suficiente. En producción se reforzaría con una transacción `SERIALIZABLE` o un token de concurrencia optimista sobre un contador de aforo por evento, para descartar condiciones de carrera bajo alta concurrencia.

---

## 7. Pruebas

- **Unitarias de dominio** (`EventosVivos.Domain.UnitTests`): reglas puras y casos borde — RN01 (incl. límite exacto), RN03 (sábado 22:00 vs 23:00), futuro/orden de fechas, máquina de estados de la reserva, RN07, límites por transacción (RN05/RF-03), `Overlaps`, validación de email.
- **Integración** (`EventosVivos.Application.IntegrationTests`): handlers ejecutados contra **SQLite en memoria real** (no el provider in-memory de EF), para validar también las consultas (GROUP BY, filtros, conversiones). Cubren: superposición de venue (RN02), agotamiento y liberación de aforo, RF-03 (<24h), RN04, generación de código `EV-######`, RN07 que conserva inventario, y el reporte de ocupación completo.

El reloj se inyecta vía `IDateTimeProvider` (fake en tests) para que las reglas dependientes del tiempo sean deterministas.

---

## 8. Seguridad

- Validación estricta de entrada (FluentValidation) + invariantes en el dominio (defensa en profundidad).
- Sin SQL crudo: EF Core parametriza todas las consultas.
- Contrato de error uniforme que **no** filtra *stack traces* (los 500 se loguean en servidor y devuelven un mensaje genérico).
- CORS restringible por configuración (`Cors:AllowedOrigins`).
- *Nota:* no se incluyó autenticación porque el enunciado define acciones de "usuario" y "administrador" a nivel funcional pero no pide auth. La separación natural de endpoints (reservar vs. confirmar pago) permite añadir autorización por rol (p. ej. JWT + políticas) sin tocar el dominio.

---

## 9. Posibles mejoras

- Autenticación/autorización por roles (JWT) sobre los endpoints de administración.
- Migraciones EF versionadas (hoy se usa `EnsureCreated` por simplicidad de arranque).
- Concurrencia optimista sobre el aforo para escenarios de alta demanda.
- Paginación en el listado de eventos.
- Tests E2E del frontend (Playwright) y unitarios de componentes.
