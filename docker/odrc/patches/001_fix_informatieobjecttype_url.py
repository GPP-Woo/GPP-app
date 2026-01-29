"""
Patch ODRC to use OpenZaak's Catalogi API URL instead of its own.

OpenZaak only accepts informatieobjecttype URLs from its own Catalogi API.
This patch modifies the register_in_documents_api method in Document model
to construct URLs pointing to OpenZaak's Catalogi API.
"""

import os
import sys


def apply_patch():
    models_path = '/app/src/woo_publications/publications/models.py'

    if not os.path.exists(models_path):
        print(f"ERROR: {models_path} not found", file=sys.stderr)
        return False

    with open(models_path, 'r') as f:
        content = f.read()

    # Check if patch is already applied
    if 'PATCH: Use OpenZaak\'s Catalogi API' in content:
        print(f"Patch already applied to {models_path}")
        return True

    old_code = '''        assert isinstance(information_category, InformationCategory)
        iot_path = reverse(
            "catalogi-informatieobjecttypen-detail",
            kwargs={"uuid": information_category.uuid},
        )
        documenttype_url = build_absolute_uri(iot_path)'''

    new_code = '''        assert isinstance(information_category, InformationCategory)

        # PATCH: Use OpenZaak's Catalogi API URL instead of ODRC's own
        # OpenZaak only accepts informatieobjecttype URLs from its own Catalogi API
        from zgw_consumers.models import Service
        try:
            ztc_service = Service.objects.get(api_type='ztc')
            documenttype_url = f"{ztc_service.api_root}informatieobjecttypen/{information_category.uuid}"
        except Service.DoesNotExist:
            # Fallback to old behavior if no Catalogus API is configured
            iot_path = reverse(
                "catalogi-informatieobjecttypen-detail",
                kwargs={"uuid": information_category.uuid},
            )
            documenttype_url = build_absolute_uri(iot_path)'''

    if old_code not in content:
        print(f"ERROR: Could not find code to patch in {models_path}", file=sys.stderr)
        return False

    content = content.replace(old_code, new_code)

    with open(models_path, 'w') as f:
        f.write(content)

    print(f"SUCCESS: Patched {models_path}")
    return True


if __name__ == '__main__':
    success = apply_patch()
    sys.exit(0 if success else 1)
