# Guía de despliegue — Supabase + Render + Vercel

Arquitectura desplegada:

```
Navegador ──> Vercel (Angular SPA + proxy /api) ──> Render (.NET API) ──> Supabase (PostgreSQL)
```

El proxy de Vercel reescribe `/api/*` hacia el backend en Render, así el frontend y el API quedan
en el mismo origen y **no hace falta configurar CORS**.

---

## 1. Base de datos — Supabase

1. Crea un proyecto en https://supabase.com (free tier).
2. **Project Settings → Database → Connection string**. Copia los datos y arma la cadena en
   formato Npgsql (no la URI):

   ```
   Host=db.<ref>.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=<tu-password>;SSL Mode=Require;Trust Server Certificate=true
   ```

   > Si Render no conecta por el puerto 5432, usa el **Session Pooler** de Supabase
   > (Connection Pooling, puerto 6543) con el mismo formato.

3. No hace falta crear tablas a mano: el API ejecuta `EnsureCreated` y siembra los venues al arrancar.

## 2. Backend — Render

1. Sube el repo a GitHub.
2. En https://render.com → **New → Blueprint**, apunta al repo. Render leerá `render.yaml`.
   (O bien **New → Web Service**, runtime *Docker*, raíz `backend/`.)
3. En **Environment**, define la variable secreta:
   - `ConnectionStrings__Default` = la cadena Npgsql de Supabase del paso 1.
   - (`Database__Provider=Postgres` y `healthCheckPath=/health` ya vienen en `render.yaml`.)
4. Deploy. Anota la URL pública, p. ej. `https://eventosvivos-api.onrender.com`.
   Verifica `https://<tu-app>.onrender.com/health` → `{ "status": "healthy" }`.

   > Plan free: el servicio se duerme tras ~15 min de inactividad; la primera petición tras dormir
   > tarda unos segundos en responder.

## 3. Frontend — Vercel

1. Edita `frontend/vercel.json` y reemplaza `REEMPLAZA-CON-TU-APP.onrender.com` por tu URL real de Render.
2. En https://vercel.com → **Add New → Project**, importa el repo y selecciona la carpeta raíz `frontend/`.
   Vercel detecta `vercel.json` (build `npm run build`, output `dist/eventosvivos/browser`).
3. Deploy. La app queda en `https://<tu-app>.vercel.app` consumiendo el API vía el proxy `/api`.

## 4. Comprobación

- `https://<tu-app>.vercel.app` carga el listado de eventos.
- Crear evento, reservar, confirmar/cancelar y el reporte de ocupación funcionan de extremo a extremo.

---

### Notas
- Cambiar de SQLite (local) a Postgres (nube) es solo cuestión de `Database:Provider` + la connection string;
  el dominio y los handlers no cambian (ventaja de la inversión de dependencias).
- Para alta concurrencia real se recomienda el pooler de Supabase y, a nivel de aplicación, una transacción
  serializable o un token de concurrencia sobre el aforo (ver README §6).
