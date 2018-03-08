Title: Handshakes
Description: Handshakes API
---
# Overview on Handshakes

There are two kinds of handshakes: business and handshakes. This explanation applies to the most common type: business handshakes.
Individual handshakes are explained below.
 
Handshakes are a special kind of interaction with the following differences:

- Handshakes are not distributed on feed to be discovered. The registration (ie. forming a relationship) is instead initiated by business 
in form of an invitation, which is sent to user via notification.
- Registration of a normal interaction is static, ie. the business-user relationship is limited to sending a snapshot of current data at 
the moment of registration, which will never be updated in the whole lifetime of the relationship. In contrast, registration of a handshake
is dynamic in that the data will be automatically updated (sync'ed) with the actual data user changes in their information vault.
- To facilitate keeping track of the mutual relationship, handshakes once established can be managed by both business and user. 

## Handshake Registration

Since a handshake is an interaction, user registration is implemented in the same way:

- Open the handshake form with information and data obtained from the [Interaction Details API](interaction-details.html). The form is displayed as
a normal interaction, with two minor differences: the interaction description text should be shown on top, and the _I Agree_ statement
should be updated for case of handshake.

- Register with the [Interaction Registration API](interaction-register.html) as usual.

## Handshake Sync Cycle

Once established (registration complete), an effective handshake relationship is actively running between the two parties. Every time
user changes something in their vault that corresponds to a handshake field, information about the change is automatically sent
to business via a notification, where business then acts on the information. 
This is a recurring process, where the current phase can be indicated by the **status** of the handshake, which alternates between two states:

- **Standby**: Nothing is happening, though the vault checking logic is always running in background to poll for changes. 
This is the normal state. Since this most commonly results from completion of a previous cycle (business acknowledging), on web app,
this is labelled _acknowledged_, represented by a color code of green. The only exception is for a new relationship where
no sync happened before, in which case it is labelled _inactive_, represented by a color code of gray.
- **Sent**: User change just detected on a relevant vault field. Information about the change has been sent to business, awaiting
acknowledgement. On web app, this is represented by a color code of red.

Once business acknowledges the handshake notification, the sync is considered complete, and the handshake status is turned back
to _standby_, starting the next cycle.

## Handshake Lifecycle

In addition to the above-mentioned status related to the sync cycle, a handshake relationship maintains another state called
**user status** that is related to the handshake _lifecycle_.  

- Once a handshake is initiated, an invitation is sent to user awaiting registration. The relationship is then in the initial
status of **pending**. 
- Once user accepts and submits the form, the relationship is established and begins in the normal status
of **active**.

At any time during the relationship, user can do any of the following unilateral operations on the handshake, which updates the user status. 

- **Pause**: temporarily suspends the handshake sync operations. Data changes will not be notified. As a result, user status
becomes **paused**.
- **Resume**: turns a paused handshake back to active, resuming sync as normal. As a result, user status returns to **active**.
- **Terminate**: ends the relationship. The handshake becomes invisible and locked out of registration, but still kept in database, with user status of **terminated**.

The client UI should display warning asking for user confirmation before proceeding with the operation (especially termination).

The API endpoints for those operations are listed below. Note that:
 
- The key to identify a handshake to act on is the **interaction ID**, 
not the handshake ID which is used internally only and should be ignored by the client.
- The key is passed as a route-based parameter within the URL.

# GET `/Handshake/List`

> List handshakes

Retrieve current user's list of handshake relationships, including pending invitations

## Params
Normally none. Provide an URL parameter to indicate the list should include terminated handshakes:

    ?include=terminated 

## Response

    {
        "success": true,
        "message": "List 9 handshakes",
        "data": [
            {
                "id": "58d23a0e6e1b2763a0752962",                       // ID of handshake, internal use only
                "interactionId": "58d2387f6e1b2763a0752950",            // ID of the interaction defining the handshake
                "accountId": "c52283cf-c77b-4433-ad69-9e8bc6451aa9",    // Account ID of individual user
                "businessId": "25d6bc5d-8931-43f0-8ba2-87c2ed798eab",   // Account ID of business (interaction owner)
                "userStatus": "active"                                  // "pending", "active" or "paused", or "terminated"
                "status": "standby",                                    // "standby" or "sent", only present if active
                "lastUpdated": "2017/12/10" ,                           // Date of last sync, only present if active
                "interaction": {                                        // Details of interaction
                    "id": "58d2387f6e1b2763a0752950",
                    "type": "handshake",
                    "name": "Handshake Name",
                    "description": "Handshake description...",
                    ...
                    "form": {                                            // Form details including user data
                        ...
                    }
                }
            },
            ...
        ]
    }
    
Included in the list is the details of the interaction associated with the handshake, identical to that obtained by
the [Interaction Details API](interaction-details.html).

# POST `/Handshake/Pause/{interactionId }`

> Pause a handshake

## Params

Provide the ID of the handshake interaction, as route parameter

## Response

    {
        "success": true,
        "message": "Handshake paused",
        "data": {
            "handshakeId": "5a27786c6e1b1b6cc0f2d74d",
            "interactionId": "59cb69246e1b1b0cc89580af",
            "status": "paused"
        }
    }
    

# POST `/Handshake/Resume/{interactionId }`

> Resume a paused handshake

## Params

Provide the ID of the handshake interaction, as route parameter

## Response

    {
        "success": true,
        "message": "Handshake resumed",
        "data": {
            "handshakeId": "5a27786c6e1b1b6cc0f2d74d",
            "interactionId": "59cb69246e1b1b0cc89580af",
            "status": "active"
        }
    }

# POST `/Handshake/Terminate/{interactionId }`      

> Terminate a handshake

Once terminated, a handshake cannot be restored. The only possible operation on a terminated handshake is removing it.

## Params

Provide the ID of the handshake interaction, as route parameter

## Response

    {
        "success": true,
        "message": "Handshake terminated successfully",
        "data": {
            "handshakeId": "5a27786c6e1b1b6cc0f2d74d",
            "interactionId": "59cb69246e1b1b0cc89580af",
            "status": "terminated"
        }
    }
    
# POST `/Handshake/Remove/{interactionId }`      

> Remove a terminated handshake

Only a previously terminated handshake can be removed, otherwise an error is returned.

## Params

Provide the ID of the handshake interaction, as route parameter

## Response

    {
        "success": true,
        "message": "Handshake removed successfully",
        "data": {
            "handshakeId": "5a27786c6e1b1b6cc0f2d74d",
            "interactionId": "59cb69246e1b1b0cc89580af",
        }
    }
    
# Overview on Individual Handshakes

Individual handshakes are similar in that it represents a relationship between two parties, one of which acts as source 
of information taken from their vault, while the other receives updates when such information is changed. However, 
the latter is an individual user, instead of a business user. Such relationship is thus much more simpler:

- A handshake is created by an individual user who wants to share information with others. It is not associated
with any interaction as in the case of business.
- The recipient is normally a Regit individual user, but can be anybody with an email address.
- The relationship is one-way; once created it will effect immediately, without waiting for the recipient to accept.
- The update flow is one-way; on data change, the notification will be sent to the recipient and require no action from them 
(no need to acknowledge). So there is no sync cycle.
- Notifications are sent by email only.
- Similar to business handshakes, the possible operations on an individual handshake are **pause**, **resume**, **block**, **terminate** 
and **remove**. Accordingly along the lifecycle, user status can be one of these: **active**, **paused**, **blocked**, or **terminated**.

Each handshake is assigned a list of fields to sync. Any change to the information vault related to a field in the list
will trigger the notification for the handshake. A field is represented by its path, and there are 
a limited number of applicable fields (see the [list below](#post-handshakeindividualadd)). When creating a handshake, user
should be allowed to choose which fields to include in the list. 

On a user's perspective, a handshake can have one of two directions:

- **Outbound**: created and owned by current user and sent to other party, who will be the recipient of change notifications 
originating from current user's information vault. 
- **Inbound**: created and owned by another user and sent to current user, who will be the recipient of change notifications 
originating from the owner's information vault.

Following are API endpoints to act on individual handshakes, similar to business counterparts. One difference is that 
 the key used to identify a handshake must be the handshake ID, not interaction ID.
 
 
# GET `/Handshake/Individual/List`

> List individual handshakes

Retrieve current user's list of individual handshake relationships

## Params
Provide an URL parameter to indicate which direction to filter out (if omitted, "_all_" is implied):

    ?filter=all              // Either "all", "out" or "in"

The direction can be:

- **"all"** (or omitted): list all handshakes of both directions
- **"out"**: list only outbound handshake
- **"in"**: list only inbound handshakes

Optionally, provide an extra URL paramemter to indicate the list should include terminated handshakes.

    &include=terminated 

## Response

    {
        "success": true,
        "message": "List 12 handshakes",
        "data": [
            {
                "id": "59dcb8bf6e1b3d2a24c0be7b",                           // Handshake ID
                "direction": "out",                                         // Handshake direction
                "accountId": "c52283cf-c77b-4433-ad69-9e8bc6451aa9",        // Account ID of owner (creator)
                "toAccountId": "eb73fdd0-1bc9-43be-aeb9-eda2d010b919",      // Account ID of recipient
                "withProfile": {                                            // Profile of other user (either receiver or owner
                    "id": "...,
                    "displayName": "...",
                    "avatar": "..."
                },
                "expires": "2018-02-16",                                    // Expiry date in YYYY-MM-DD format (or "Indefinite")
                "userStatus": "active",                                     // User status (normally "active" or "paused"
                                                                            // or "terminated", only present if include=terminated
                "lastUpdated": "2017-10-14T11:05:07.5830000Z"               // Only present if last sync took place
            },
            {
                "id": "59dcbb906e1b3d2a24c0be7e",
                "direction": "out",                                         
                "accountId": "c52283cf-c77b-4433-ad69-9e8bc6451aa9",
                "toAccountId": null,                                        // Account ID = null means a special case of outbound handshake
                                                                            // where recipient is a public person (non-Regit)
                "toName": "",                                               // Name of public recipient (as set by creator)
                "toEmail": "person@hotmail.com",                            // Email address of pubic recipient
                "userStatus": "active",
                "expires": "Indefinite",
            },
            ...
        ]
    }
    
# POST `/Handshake/Individual/Pause/{handshakeId }`

> Pause an individual handshake

## Params

Provide the ID of the handshake, as route parameter

## Response

    {
        "success": true,
        "message": "Handshake paused",
        "data": {
            "handshakeId": "59dc53206e1b2e2a240f74a0",
            "withAccountId": "af7dc59a-3184-4ef3-a57c-a752c01dbf9e",
            "status": "paused"
        }
    }
    
# POST `/Handshake/Individual/Block/{handshakeId }`
`      
> Block an individual handshake

Blocking works the same way as pausing, but in a one-way fashion:
 only the recipient of the handshake, not the owner, is allowed to block or unblock.
 As a result, this operation only applies to an inbound handshake. 

## Params

Provide the ID of the handshake, as route parameter

## Response

    {
        "success": true,
        "message": "Handshake blocked",
        "data": {
            "handshakeId": "59dc53206e1b2e2a240f74a0",
            "withAccountId": "af7dc59a-3184-4ef3-a57c-a752c01dbf9e",
            "status": "blocked"
        }
    }
    
# POST `/Handshake/Individual/Resume/{handshakeId }`

> Resume a paused or blocked individual handshake

## Params

Provide the ID of the handshake, as route parameter

## Response

    {
        "success": true,
        "message": "Handshake resumed",
        "data": {
            "handshakeId": "59dc53206e1b2e2a240f74a0",
            "withAccountId": "af7dc59a-3184-4ef3-a57c-a752c01dbf9e",
            "status": "active"
        }
    }

# POST `/Handshake/Individual/Terminate/{handshakeId }`

> Terminate an individual handshake

Once terminated, a handshake cannot be restored. The only possible operation on a terminated handshake is removing it.

## Params

Provide the ID of the handshake, as route parameter

## Response

    {
        "success": true,
        "message": "Handshake terminated successfully",
        "data": {
            "handshakeId": "59dc53206e1b2e2a240f74a0",
            "withAccountId": "af7dc59a-3184-4ef3-a57c-a752c01dbf9e",
            "status": "terminated"
        }
    }
    
# POST `/Handshake/Individual/Remove/{handshakeId }`

> Remove a terminated handshake

Only a previously terminated handshake can be removed, otherwise an error is returned.

## Params

Provide the ID of the handshake interaction, as route parameter

## Response

    {
        "success": true,
        "message": "Handshake removed successfully",
        "data": {
            "handshakeId": "59dc53206e1b2e2a240f74a0",
            "withAccountId": "af7dc59a-3184-4ef3-a57c-a752c01dbf9e",
        }
    }
    
# POST `/Handshake/Individual/Add`

> Add individual handshake

Create an outbound individual handshake and send to recipient

## Params

Provide the details of the handshake to create, as JSON body:

    {
        toAccountId: "efbfae6c-b935-43cb-878c-dbea7166c78f",    // Account ID of recipient (null if public)
        toName: "",                                             // Name of recipient (if public; optional)
        toEmail: "",                                            // Email of recipient (if public; required)
        description: "...",                                     // Description of the handshake
        expires: "2018-02-14",                                  // Expiry date in YYYY-MM-DD format, or empty = indefinite
        notifyFormat: "",                                       // Notification format: empty string = notification only
                                                                // or "values" = include changed values
        fieldPaths: [                                           // List of field paths as string
            "address.currentAddress",
            "contact.mobile",
            ...
        ]
    }
    
The list of fields is required, must contain at least one path, 
and each path must correspond to one of six acceptable fields as follows:

- Current Address (**address.currentAddress**), Mailing Address (**address.mailingAddress**)
- Mobile Phone Number (**contact.mobile**), Office Phone Number (**contact.office**)
- Personal Email (**contact.email**), Work Email (**contact.officeEmail**)

Other paths will be ignored. 

> Only include in the list the fields that user has selected when adding the handshake.

## Response

    {
        "success": true,
        "message": "Individual handshake created successfully",
        "data": {
            "handshakeId": "5a583ddc05100b4a2c2ed62b"           // ID of the newly created handshake
        }
    }