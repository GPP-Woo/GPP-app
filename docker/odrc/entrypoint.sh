#!/bin/bash
set -e

echo "Applying ODRC patches..."

# Apply all Python patches
for patch in /app/patches/*.py; do
    if [ -f "$patch" ]; then
        echo "Applying patch: $(basename $patch)"
        python "$patch" || echo "Warning: Patch $(basename $patch) failed"
    fi
done

echo "Patches applied. Starting application..."

# Execute the original command
exec "$@"
