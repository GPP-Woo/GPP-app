# Metadata Generation Setup

This document explains how the "Genereer metadata" (Generate metadata) button works.

## Architecture

```
Vue Frontend → ODPC Backend → woo-hoo Service
```

1. **Frontend** (`odpc.client`): Shows "Genereer metadata" button when woo-hoo service is available
2. **ODPC Backend** (`ODPC.Server`): Downloads PDF from ODRC, uploads to woo-hoo, returns metadata
3. **woo-hoo Service**: AI-powered metadata generation from PDF files

## Local Development Setup

### 1. Start woo-hoo service

```bash
cd /Users/moritz/Code/gppwoo/woo-hoo
uv run uvicorn woo_hoo.main:app --host 0.0.0.0 --port 8003 --reload
```

Or use the Makefile (from GPP-app directory):
```bash
make up-woo-hoo
```

Verify it's running (in a separate terminal):
```bash
curl http://localhost:8003/health
# Should return: {"status":"healthy","service":"woo-hoo","version":"0.1.0"}
```

### 2. Start ODPC stack

```bash
cd /Users/moritz/Code/gppwoo/GPP-app
docker compose up -d
```

### 3. Configure frontend

The frontend is already configured for local dev via `odpc.client/.env.local`:
```
VITE_ODPC_API_URL=http://localhost:62230
```

This tells the frontend to call ODPC directly at port 62230 (bypassing CORS).

### 4. Test

Check all services are running:
```bash
make health
```

Then test the feature:

**Option A: Development mode (with hot reload)**
1. Run `make dev` to start Vite dev server
2. Open http://localhost:5173
3. Changes to Vue files will auto-reload

**Option B: Production-like mode (via Docker)**
1. Access http://localhost:62230 (Vue app served by ODPC)
2. No authentication required for local dev

Both options allow you to:
1. Navigate to a publication
2. Upload a PDF document
3. Click "Opslaan als concept" to save the document
4. The "Genereer metadata" button should appear in the actions menu
5. Select a document from the dropdown (if multiple documents)
6. Click "Genereer metadata" to generate metadata

Note: http://localhost:8002 serves the WOO-Publicatiebank-API (Django ODRC), not the GPP-app frontend.

## Production Deployment

### Nginx Configuration

The nginx reverse proxy ([docker/nginx/nginx.conf](docker/nginx/nginx.conf)) routes requests:

- `/api/v1/metadata/*` → ODPC backend (port 8080)
- Everything else → ODRC/Django backend (port 8000)

This allows the frontend to use relative URLs without CORS issues.

### Frontend Configuration

In production, VITE_ODPC_API_URL should be empty or not set:

```bash
# .env.production (or don't set VITE_ODPC_API_URL at all)
VITE_ODPC_API_URL=
```

With an empty value, the frontend uses relative URLs like `/api/v1/metadata/health`.
Nginx receives these requests and routes them to the appropriate backend.

### ODPC Configuration

Set these environment variables on the ODPC backend:

```bash
WOO_HOO_BASE_URL=https://your-woo-hoo-service.example.com
ODRC_BASE_URL=https://your-odrc-instance.example.com
ODRC_API_KEY=your-api-key

# Optional timeout settings (defaults shown)
WOO_HOO_HEALTH_TIMEOUT_SECONDS=30
WOO_HOO_GENERATE_TIMEOUT_SECONDS=120
```

### Enable Authentication

For production, configure OIDC authentication:

```bash
OIDC_AUTHORITY=https://your-oidc-provider.example.com
OIDC_CLIENT_ID=your-client-id
OIDC_CLIENT_SECRET=your-client-secret
OIDC_ADMIN_ROLE=admin
```

## How It Works

1. **Health Check**: Frontend calls `/api/v1/metadata/health` to check if woo-hoo is available
2. **Button Display**: If healthy, the "Genereer metadata" button appears
3. **Generate Metadata**: When clicked:
   - Frontend → `POST /api/v1/metadata/generate/{documentUuid}`
   - ODPC downloads PDF from ODRC: `GET /api/v2/documenten/{uuid}/download`
   - ODPC uploads PDF to woo-hoo: `POST /api/v1/metadata/generate-from-file`
   - woo-hoo processes PDF and returns metadata
   - ODPC forwards metadata to frontend
   - Frontend displays generated metadata

## Troubleshooting

### Button doesn't appear

Check health endpoint:
```bash
curl http://localhost:62230/api/v1/metadata/health
```

Should return HTTP 200. If not:
- Verify woo-hoo is running on port 8003
- Check ODPC logs: `docker compose logs odpc`
- Verify WOO_HOO_BASE_URL is set correctly

### Metadata generation fails

Check ODPC logs for the full error:
```bash
docker compose logs -f odpc
```

Common issues:
- ODRC document not found (document not saved yet)
- woo-hoo service not running
- Network connectivity issues
