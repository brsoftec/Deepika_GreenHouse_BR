Title: Network
Description: Network API
---
# GET `/Network/IsFriend/{userId }`

> Check if user is in network

Query the status of whether a user is in current user's network, or in a pending relationship

## Params

Provide the entity ID of the user to check

## Response
 
If found in either network (**Normal** or **Trust**), the type of network is included 

    {
        "success": true,
        "message": "User is network member",
        "data": {
            "isMember": true,
            "userId": "585a367553bc292828bdac1f",
            "network": "normal"      // or "trust"
        }
    }

If not found in any network:

    {
        "success": true,
        "message": "User is not network member",
        "data": {
            "isMember": false,
            "userId": "585a367553bc292828bdac1f"
        }
    }

If a network invitation is pending:

    {
        "success": true,
        "message": "User is pending network member",
        "data": {
            "isMember": false,
            "isPending": true,
            "userId": "585a367553bc292828bdac1f",
            "invitationId": "5a19234a05100b2b58d884cd"
        }
    }
    
# GET `/Network/IsFriend`

> Check if user is in network

Same as previous endpoint, but using account ID as key

## Params
Provide the account ID of the user to check, URL encoded

    ?accountId=efbfae6c-b935-43cb-878c-dbea7166c78f 

## Response    
Same as previous endpoint

# GET `/Network/Friends`

> List network members

List members in current user's personal network, including those in pending relationship

## Params

None

## Response

    {
        "success": true,
        "message": "List network members",
        "data": [
            {       // NORMAL friend
                "userId": "58c67a256e1b455d503d3b47",
                "accountId": "1699e8c3-17d2-447f-abe7-ba369b732c23",
                "displayName": "Bob Lee",
                "avatar": "/Content/ProfilePictures/58c67a256e1b455d503d3b47_profile_pic_636265661474536571.jpg",
                "isMember": true,
                "network": "normal"
            },
            {       // TRUSTED friend
                "userId": "58dc6f9a6e1b3a0d48a607fa",
                "accountId": "201566ab-c51f-47ce-af6f-193422a9fe3a",
                "displayName": "Soledad Simson",
                "avatar": "/Content/ProfilePictures/58dc6f9a6e1b3a0d48a607fa_profile_pic_636265661942532012.jpg",
                "isMember": true,
                "network": "trust"
            },
            {       // PENDING friend (outstanding invitation)
                "userId": "58c680ed6e1b455d503d3b71",
                "accountId": "76603d38-424a-42f1-823a-30307c5b2a00",
                "displayName": "Lincohn D.",
                "avatar": "/Content/ProfilePictures/58c680ed6e1b455d503d3b71_profile_pic_636265659085029010.jpg",
                "isMember": false,
                "isPending": true,
                "direction": "out",             // "out" = outgoing invitation, or "in" = incoming invitation
                "invitationId": "5a21432005100b2784e69466"
            },
            ...
        ]
    }
    
    
# POST `/Network/Trust`

> Move a member to trust network

## Params

Provide either the entiry ID or account ID of the network member to move, URL encoded

    ?userId=585a367553bc292828bdac1f
    
 Or

    ?accountId=fbfae6c-b935-43cb-878c-dbea7166c78f

## Response

    {
        "success": true,
        "message": "Member moved to trust network",
        "data": {
            "userId": "585a367553bc292828bdac1f",
            "accountId": "efbfae6c-b935-43cb-878c-dbea7166c78f",
            "isMember": true,
            "network": "trust"
        ]
    }
    
# POST `/Network/Untrust`

> Move a member to normal network

## Params

Same as moving to trust network

## Response

    {
        "success": true,
        "message": "Member moved to normal network",
        "data": {
            "userId": "585a367553bc292828bdac1f",
            "accountId": "efbfae6c-b935-43cb-878c-dbea7166c78f",
            "isMember": true,
            "network": "normal"
        ]
    }

# POST `/Network/Remove`

> Remove a member from network

If any delegation relationships exist with the member, those will also be removed. It is therefore advised for GUI to 
display warning and ask for user's confirmation before proceeding with removal.

## Params

Provide either the entiry ID or account ID of the network member to remove, URL encoded

    ?userId=585a367553bc292828bdac1f
    
 Or

    ?accountId=fbfae6c-b935-43cb-878c-dbea7166c78f

## Response

    {
        "success": true,
        "message": "Member removed from network",
        "data": {
            "userId": "585a367553bc292828bdac1f",
            "accountId": "efbfae6c-b935-43cb-878c-dbea7166c78f",
            "status": "removed",
            "isMember": false
        }
    }
        

# POST `/Network/Invite`

> Invite user to network

Send an invitation by notification and email to another individual user prompting them to join current user's personal network.

The network relationship is considered pending and will only take effect after the other user accepts the invitation. 
Until then calling this API on a user with pending invitation in either direction will return error. 

## Params

Provide the account ID (identity ID) of the recipient, URL encoded

    ?accountId=fbfae6c-b935-43cb-878c-dbea7166c78f

## Response

    {
        "success": true,
        "message": "Network invitation sent",
        "data": {
            "inviteeId": "efbfae6c-b935-43cb-878c-dbea7166c78f",   // Recipient account ID
            "invitationId": "5a1539ed05100b224ca3dd61"      // ID of the created invitation
        }
    }

The invitation ID can be used later for tracking this invitation status.
    
# POST `/Network/Accept`

> Accept a network invitation

A mutual network relationship with the sender will be created as a result.

## Params

Provide ID of the network invitation, URL encoded

    ?invitationId=5a25358005100b05b880f3a0

## Response

    {
        "success": true,
        "message": "Network invitation denied",
        "data": {
            "invitationId": "5a25358005100b05b880f3a0",
            "status": "accepted",
            "memberAccountId": "efbfae6c-b935-43cb-878c-dbea7166c78f",
            "isMember": true,
            "network": "normal
        }
    }
    
# POST `/Network/Deny`

> Deny a network invitation

The network invitation will be removed as a result.

## Params

Provide ID of the network invitation, URL encoded

    ?invitationId=5a25358005100b05b880f3a0

## Response

    {
        "success": true,
        "message": "Network invitation denied",
        "data": {
            "invitationId": "5a25358005100b05b880f3a0",
            "status": "removed",
            "senderAccountId": "efbfae6c-b935-43cb-878c-dbea7166c78f",
            "isMember": false
        }
    }    
    
# GET `/Network/Business/List`

> List business network

Business network is the collection of all businesses that current user is following.

## Params

None

## Response

    {
        "success": true,
        "message": "List 5 businesses",
        "data": [
            {
                "id": "58c6622b6e1b435d50a853a7",
                "accountId": "25d6bc5d-8931-43f0-8ba2-87c2ed798eab",
                "displayName": "East Asia School",
                "avatar": "/Content/ProfilePictures/58c6622b6e1b435d50a853a7_profile_pic_636250322947178559.jpg",
                "followed": true,
                "followedAt": "2017-07-14T15:57:49.8014262+08:00"
            },
            ...
        ]
    }
    
# How to Handle Network Invitations

An inbound network invitation comes to user via a notification of type **Network Invitation**.

A typical network invitation notification is in this format:

    {
        "Id": "5a7a939e07e12d08ac4c73b1",
        "Category": "network",
        "Type": "Invite Friend",
        "DateTime": "2018-02-07T05:50:22.7322666Z",
        "FromAccountId": "efbfae6c-b935-43cb-878c-dbea7166c78f",
        "FromUserDisplayName": "Son Nguyen",
        "ToAccountId": "f8b00a20-e88d-4a0f-a0f2-3c07f896b2b7",
        "ToUserDisplayName": "Long Tran",
        "Title": "Network invitation from Son Nguyen",
        "PreserveBag": "5a7a939e07e12d08ac4c73af",
    }   
    
Pay attention to two keys:

- **FromAccountId** is account ID of the user inviting you.
- **PreserveBag** is ID of the invitation.

Typical UI behavior when receiving such notification is to display it with action buttons **Accept** and **Deny**. However, sometimes the invitation has been acted on before (either accepted or denied), and there's no way to tell if that has happened by looking at this notification data. So more work is needed to properly display the notification.

For _push notification_ (received from FCM) the invitation is brand-new, no previous actions are possible so just display the buttons.

For _offline notification_ (retrieved by API), everytime the client needs to display the notification, it should do this check first:

Call the [Network Status API](#get-networkisfriend) on the sending user account ID:

    /Network/IsFriend?accountId=efbfae6c-b935-43cb-878c-dbea7166c78f
    
Inspect the response and act on one of three possible cases:

- If `isMember` is `true`: network relationship already established. Hide the buttons.
- Otherwise if `isPending` is `false` or missing: the invitation has been resolved. Hide the buttons.
- Otherwise if `isPending` is `true`: the invitation is still pending. Show the buttons.
    
When buttons are shown, and user taps on either _Accept_ or _Deny_ button, call API on the invitation ID to do according action:
    
    /Network/Accept?invitationId=5a7a939e07e12d08ac4c73af       // or /Network/Deny...