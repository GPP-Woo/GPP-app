# ODRC Patches

This directory contains patches that are automatically applied to the ODRC container on startup.

## Why These Patches Are Needed

The ODRC application uses a Docker image from MaykinMedia. We need to apply runtime patches to fix integration issues with OpenZaak.

## How It Works

1. **Entrypoint Script**: The `docker/odrc/entrypoint.sh` script runs before ODRC starts
2. **Automatic Patching**: All Python files in this directory are executed in order (001_, 002_, etc.)
3. **Idempotent**: Patches check if they're already applied before making changes
4. **Reproducible**: These patches are part of the repository and will be applied on any machine

## Current Patches

### 001_fix_informatieobjecttype_url.py

**Problem**: ODRC was constructing `informatieobjecttype` URLs pointing to its own Catalogi API endpoint (e.g., `http://odrc/catalogi/api/v1/informatieobjecttypen/...`). However, OpenZaak's Documents API only accepts `informatieobjecttype` URLs from OpenZaak's own Catalogi API, not external ones.

**Solution**: Patches the `Document.register_in_documents_api()` method to:
1. Query the configured Catalogus API service (ztc) from the database
2. Construct URLs using OpenZaak's Catalogi API base URL
3. Fall back to the original behavior if no Catalogus API is configured

**Files Modified**: `/app/src/woo_publications/publications/models.py`

## Adding New Patches

To add a new patch:

1. Create a numbered Python file: `00X_description.py`
2. Implement an `apply_patch()` function that returns `True` on success
3. Make the patch idempotent (check if already applied)
4. Add error handling and informative messages
5. Test locally before committing

Example structure:
```python
"""
Description of what this patch fixes.
"""

def apply_patch():
    # Check if already applied
    if already_applied:
        print("Patch already applied")
        return True

    # Apply the patch
    try:
        # ... patch logic ...
        print("SUCCESS: Patched X")
        return True
    except Exception as e:
        print(f"ERROR: {e}", file=sys.stderr)
        return False

if __name__ == '__main__':
    import sys
    sys.exit(0 if apply_patch() else 1)
```

## Troubleshooting

If a patch fails to apply:

1. Check container logs: `docker logs gpp-app-odrc-django-1`
2. The patch script will show success/failure for each patch
3. Patches that fail will log errors but won't stop the container from starting
4. You can manually test a patch: `docker exec gpp-app-odrc-django-1 python /app/patches/001_fix_informatieobjecttype_url.py`

## Verification

To verify patches are applied:

```bash
# Check if patch was applied
docker exec gpp-app-odrc-django-1 grep -A 5 "PATCH: Use OpenZaak" /app/src/woo_publications/publications/models.py

# View startup logs
docker logs gpp-app-odrc-django-1 | head -20
```
