Title: Information Vault
Description: Vault API
---
# GET `/Vault/Fields`
> [Public]   
> Get all vault fields

This utility retrieves the full list of available fields as pre-defined by the structure of the information vault.
See [Interaction Data Reference](interaction-data.html) for more information on the use of the field properties
as included in this list.

## Params

None

## Response

    {
        "success": true,
        "message": "List 127 vault fields",
        "data": [
            {
                "path": ".basicInformation.title",
                "title": "Title",
                "type": "textbox",
                "options": []
            },
            ...
        ]
    }