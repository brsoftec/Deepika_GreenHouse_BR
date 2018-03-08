Title: Interaction Registration
Description: Interaction Registration API
---
# POST `/Interaction/Register`

> Register an interaction

Participate in an interaction, submitting data for current user. The data will be sent to the owning business, and new values will be updated to user's
information vault.

## Params:
Provide the interaction ID and the list of fields with submitted data, as JSON body.

In case of delegated operation (registering on behalf of a delegator), provide the ID of the delegation, where current user
must be the delegatee.

    {
        interactionId: "5a1b913005100b5104fd7dbf",      // ID of the interaction
        delegationId: "",                               // (Optional) ID of the delegation
        fields: [
            { source: "vaultCurrentValue", path: ".basicInformation.firstName", value: "Chris" },
            { source: "newValue", path: ".contact.home", value: "84909090909" },
            { source: "vaultCurrentValue", path: ".governmentID.birthCertificate.location", value: {country:"Spain", city:"Ibiza" } },
            { source: "newValue", path: ".governmentID.passportID.expiryDate", value: "2019-01-01T00:00:00.000Z" },
            { source: "newValue", path: ".others.body.type", value:["Slim", "Tall"]},
            { source: "newValue", path: ".address.billingAddress.address", value: "New Billing Address" },
        ]
    }

To indicate new values that user submitted to be updated into the vault, the `source` property
must be expicitly set to `newValue`. Otherwise, leave it as the default value of `vaultCurrentValue` (meaning user
has not changed).

> All required fields (not marked optional) as defined by the interaction must be submitted. Any missing in the list,
 or containing invalid value will be rejected as an error.

Be careful to represent data values in the proper format. The most common type is string, while some fields are
stored as objects or arrays. Consult the [Interaction Data Reference Guide](interaction-data.html).

**New:** To facilitate type-restricted clients, the API also accepts objects or arrays as JSON strings.

For documents (`doc` fields), do one preparation step first before calling this API. Upload the file(s) that user submitted
using the Upload API endpoint listed below, then submit the server paths returned from the call along with the data.

## Response

The status of updating new user information to the vault is included. Status for each field is one of the following:

- _Updated_: The new value has been updated to the vault.
- _Not updated_: This may be due to an error, but most likely, the submitted value has been found to be the same
as the current value, so the updating was skipped.

        {
             "success": true,
             "message": "Interaction registered successfully",
             "data": {
                 "interactionId": "5a1b913005100b5104fd7dbf",
                 "accountId": "f8b00a20-e88d-4a0f-a0f2-3c07f896b2b7",
                 "updatedFields": [
                      {
                          "path": ".basicInformation.firstName",
                          "success": false,
                          "status": "notUpdated"
                      },
                      {
                          "path": ".contact.home",
                          "success": true,
                          "status": "updated"
                      },
                     {
                         "path": ".governmentID.birthCertificate.location",
                         "success": false,
                         "status": "notUpdated"
                     },
                     {
                         "path": ".governmentID.passportID.expiryDate",
                         "success": true,
                         "status": "updated"
                     },
                     {
                         "path": ".others.body.type",
                         "success": false,
                         "status": "notUpdated"
                     },
                     {
                         "path": ".address.billingAddress.address",
                         "success": true,
                         "status": "updated"
                     }
                 ]
             }
         }
         
# POST `/Interaction/Upload`

> Upload documents

Upload to server one or more files that user submitted for a `doc` type field, for later registration.

## Params

Provide the ID of the interaction whose data is being processed, URL encoded

    ?interactionId=5a1b913005100b5104fd7dbf
    
In addition, the content of the file (or files) must be encoded in standard **multipart/form-data** format. Any other way is not acceptable.

_How to form a Postman request_: Select `Body` -> `form-data`. From the dropdown beside the `key` input, select `File` instead of `Text`, 
then upload a file. Add as many files as needed.

## Response

    {
        "success": true,
        "message": "Files uploaded",
        "data": [
            {
                "filePath": "/Content/vault/documents/c52283cf-c77b-4433-ad69-9e8bc6451aa9/uploads",
                "fileName": "newphoto.jpg",
                "status": "uploaded"
            },
            ...
        ]
    }

Record the obtained file path for later submission of the `doc` data on registration.

For example, following above result, the data associated with the uploaded image is submitted for registration as follows:

    { 
        source: "newValue", 
        path: ".governmentID.passportID.photo", 
        value: [
            { 
               "filePath": "/Content/vault/documents/c52283cf-c77b-4433-ad69-9e8bc6451aa9/uploads",
               "fileName": "newphoto.jpg"
            },
            ...
        ]
    }

# POST `/Interaction/Unregister`

> Unregister an interaction

Cancel current user's (or a delegator's) participation in an interaction.

## Params:
Provide the interaction ID in JSON body.

In case of delegated operation (unregistering on behalf of a delegator), provide the ID of the delegation, where current user
must be the delegatee.

    {
        interactionId: "5a4b73c005100b20109c5c0c",      // ID of the interaction
        delegationId: "",                               // (Optional) ID of the delegation
    }

## Response

    {
        "success": true,
        "message": "Interaction unregistered successfully",
        "data": {
            "interactionId": "5a4b73c005100b20109c5c0c",
            "accountId": "efbfae6c-b935-43cb-878c-dbea7166c78f",    // ID cf current user, or delegator
            "participated": false
        }
    }

## Notes
This operation does not apply to handshake interactions. Use the [Handshake API](handshakes.html) Terminate/Remove instead. 