# GPP-app Local Development Setup Fixes

This document describes the fixes applied to enable local development and testing of the metadata generation feature.

## Date: 2026-01-29

## Problems Fixed

### 1. Document Upload to OpenZaak Failing (400 Bad Request)

**Symptoms**: When uploading a PDF to a publication, ODRC would fail with:
```
ValidationError: {'informatieobjecttype': [ErrorDetail(string='Voer een geldige URL in.', code='bad-url')]}
```

**Root Cause**:
- ODRC was sending `informatieobjecttype` as a URL pointing to its own Catalogi API endpoint
- OpenZaak's Documents API only accepts `informatieobjecttype` URLs from OpenZaak's own Catalogi API

**Solution**:
- Created a patch system for ODRC (see `docker/odrc/patches/`)
- Patch `001_fix_informatieobjecttype_url.py` modifies ODRC to query the configured Catalogus API service and construct URLs using OpenZaak's API base URL
- Modified `docker-compose.yml` to apply patches on container startup

**Files Changed**:
- `docker-compose.yml` - Added entrypoint override and volume mounts for patches
- `docker/odrc/entrypoint.sh` - New entrypoint script that applies patches
- `docker/odrc/patches/001_fix_informatieobjecttype_url.py` - Patch script
- `docker/odrc/patches/README.md` - Documentation

**Database Changes**:
- Added Catalogus API service configuration to ODRC database
- Created two `informatieobjecttype` records in OpenZaak (published, not concept):
  - `a8836b30-8b25-4af6-9b35-e68e4f644c59` - Main WOO Document type
  - `0f06fded-7e08-437d-9fa2-021841a93842` - Fallback for publications without categories

### 2. Metadata Generation Failing (403 Forbidden)

**Symptoms**: After document upload succeeded, clicking "Genereer metadata" button would fail with:
```
Metadata generatie mislukt: Failed to download document from ODRC
```

**Root Cause**:
- ODPC was sending the Authorization token but missing required audit headers
- ODRC's API requires three headers for all requests:
  - `Audit-User-ID`
  - `Audit-User-Representation`
  - `Audit-Remarks`

**Solution**:
- Modified `ODPC.Server/Features/Metadata/MetadataGenerateController.cs` to add the required audit headers when downloading PDFs from ODRC

**Files Changed**:
- `ODPC.Server/Features/Metadata/MetadataGenerateController.cs` - Added audit headers (lines 67-69)

### 3. Authentication Issues (Previous Session)

**Symptoms**: Unable to access the application due to authentication errors

**Solutions Applied** (already in place):
- Modified `ODPC.Server/Authentication/DevAutoLoginMiddleware.cs` to set `context.User` for current request
- Modified `ODPC.Server/Authentication/AuthenticationExtensions.cs` to make AdminPolicy environment-dependent
- Added `local-dev` user to Groep 1 in database

**Files Changed**:
- `ODPC.Server/Authentication/DevAutoLoginMiddleware.cs`
- `ODPC.Server/Authentication/AuthenticationExtensions.cs`
- Database: Added local-dev to GebruikersgroepGebruikers table

### 4. Redis Configuration for Celery

**Symptoms**: Document upload failing due to Celery backend errors

**Solution**:
- Added CELERY_BROKER_URL and CELERY_RESULT_BACKEND environment variables to docker-compose.yml

**Files Changed**:
- `docker-compose.yml` - Lines 98-99

## Reproducibility

All fixes are now reproducible across machines:

1. **ODPC Changes**: Source code changes in `ODPC.Server/` are committed to the repository. The Docker build process incorporates these changes.

2. **ODRC Patches**: The patch system in `docker/odrc/patches/` is automatically applied on container startup. No manual intervention needed.

3. **Database Schema**: All database changes are applied via fixtures and migrations when containers start.

4. **Docker Configuration**: `docker-compose.yml` contains all necessary environment variables and volume mounts.

## How to Use

On a fresh machine:

```bash
# Clone the repository
git clone <repo-url>
cd GPP-app

# Start all services
docker-compose up -d

# Wait for services to be ready (about 30 seconds)
# The ODRC patches will be automatically applied on startup

# Access the application
# - ODPC UI: http://localhost:62230
# - ODRC UI: http://localhost:8000
# - ODRC API: http://localhost:8002

# Default credentials
# - Username: admin
# - Password: admin
```

## Testing the Metadata Generation

1. Log in to ODPC at http://localhost:62230
2. Navigate to a publication
3. Upload a PDF document
4. Once uploaded, the "Genereer metadata" button will appear
5. Click the button to generate metadata using the woo-hoo service

## Architecture

```
┌─────────────────┐
│   Browser       │
└────────┬────────┘
         │
         v
┌─────────────────┐
│   ODPC (.NET)   │ ◄── Adds audit headers
└────────┬────────┘
         │
         v
┌─────────────────┐
│  NGINX Proxy    │
└────────┬────────┘
         │
         v
┌─────────────────┐
│ ODRC (Django)   │ ◄── Patched on startup
└────────┬────────┘      to use correct URLs
         │
         v
┌─────────────────┐
│ OpenZaak        │ ◄── Validates informatieobjecttype URLs
└─────────────────┘
```

## Future Improvements

1. **Upstream Contribution**: Consider contributing the informatieobjecttype URL fix to the upstream ODRC project

2. **Configuration-Based**: Explore making the informatieobjecttype URL pattern configurable via environment variables instead of requiring a code patch

3. **Audit Headers**: Consider making audit headers configurable via environment variables in ODPC

## Notes

- All patches are idempotent and safe to run multiple times
- Patches check if they're already applied before making changes
- Container builds will work on both Intel and ARM machines (though you may see platform warnings)
- The local dev environment uses insecure credentials - never use these in production
