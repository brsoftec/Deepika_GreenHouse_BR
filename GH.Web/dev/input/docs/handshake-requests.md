Title: Handshake Requests
Description: Handshake Requests API
---
    
# Overview on Handshake Requests

Business handshakes are created by a business and sent to an individual user to initiate a relationship. 
Sometimes the use case may have the reverse direction: a user may wish to form a handshake relationship with a certain business.
In that case the user can send a **handshake request** to ask the business to initiate such relationship.
The handshake request may include some optional basic personal information given by the user. 

The recipient of the request is normally a business account, but it can be a [User Created Business](ucb.html) as well.

On receiving, the business can then **complete** the request by establishing a relationship with the user, based on one of the available handshake interactions.

- Once created, the user that sent the request is the owner. 
- Once received by business, the request is assigned status of **sent**.
- Once completed by business, the request is assigned status of **completed**.

The individual user can later cancel (remove) any handshake request he created, but it won't affect the handshake relationship created from
the request.
 
# GET `/Handshake/Request/List`

> List handshake requests

Retrieve current user's list of handshake requests

## Params
None

## Response

    {
        "success": true,
        "message": "List 5 handshake requests",
        "data": [
            {
                "id": "59a673e06e1b1b46e069b9d0",                           // ID of the request
                "toAccountId": "25d6bc5d-8931-43f0-8ba2-87c2ed798eab",      // Account ID of business (null if is UCB)
                "toProfile": {                                              // Profile of business    
                    "id": "58c6622b6e1b435d50a853a7",
                    "displayName": "East Asia School",
                    "avatar": "..."
                },
                "fromAccountId": "c52283cf-c77b-4433-ad69-9e8bc6451aa9",    // Account ID of owner (always current user)
                "firstName": "Yamada",                                      // Personal information submitted with the request
                "lastName": "Takai",
                "phoneNumber": "+84932651298",
                "email": "son@regit.today",
                "message": "Contact me",                                    // Message submitted with the request
                "createdAt": "2017-10-02T03:57:04.8260000Z",                // Date of creation of request
                "status": "sent"                                            // "sent" or "completed"
            },
            {
                "id": "5a5db78905100b30c42e4e59",
                "toAccountId": null,                                        // Null means business is UCB
                "toUcbId": "59979f9705100b86040f895a",                      // ID of the UCB
                "toProfile": {
                    "id": "59979f9705100b86040f895a",
                    "displayName": "A UCB Business",
                    "avatar": "..."
                },
                "fromAccountId": "f8b00a20-e88d-4a0f-a0f2-3c07f896b2b7",
                "firstName": "Yamada",
                "lastName": "Takai",
                "phoneNumber": null,
                "email": null,
                "message": "",
                "createdAt": "2018-01-16T08:27:53.1150000Z",
                "status": "sent"
            }
            ...
        ]
    }
        
# POST `/Handshake/Request/Remove/{requestId }`

> Remove a handshake request

Only the owner can remove a request, otherwise an error is returned.

## Params

Provide the ID of the handshake request, as route parameter

## Response

    {
        "success": true,
        "message": "Handshake request removed",
        "data": {
            "requestId": "5a5d688b05100b03607a0840",
            "toBusinessId": "19ab8a86-6b50-40b2-934c-15fbb42ea4aa"
        }
    }
    
    
# POST `/Handshake/Request/Add`

> Add handshake request

Create a handshake request and send to business

## Params

Provide the details of the handshake request to create, as JSON body:

    {
        toAccountId: "efbfae6c-b935-43cb-878c-dbea7166c78f",    // Account ID of business (null if UCB)
        toUcb: null,                                            // ID of UCB (null if normal business)
        firstName: "...",                                          // First name given by user (optional) 
        lastName: "...",                                           // Last name given by user (optional) 
        phoneNumber: "...",                                        // Phone number given by user (optional) 
        email: "...",                                              // Email address given by user (optional) 
        message: "...",                                         // Message given by user
    }
    
The personal information accompanying the request should be provided by user on creation.
For user convenience, the client UI should pre-polulate with information obtained from the user profile.

The ID of the business will not be validated. Make sure to send a valid ID, otherwise a request is still created,
but it may be ignored by some operations (like Web app manager).

## Response

    {
        "success": true,
        "message": "Handshake request created successfully",
        "data": {
            "requestId": "5a5db78905100b30c42e4e59"
        }
    }