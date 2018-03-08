Title: Messaging
Description: Chat API
---
# GET `/Chat/Conversations`

> Get all conversations and messages

This call retrieves the full messaging history for current user, represented by two entities:

- **Conversations** as one-to-one chats with another user (must be a member in personal network)
- **Messages** within each conversation


## Params

None

## Response
 
     {
       "userId": "58cde4d96e1b1b3dd01c29ee",
       "userName": "Yamada Takai",
       "conversations": [           // List of conversations, one for each network member
         {
           "id": "58d36da86e1b2963a0c42548",
           "name": "Cindy Nguyen",
           "isGroupChat": false,
           "from": {                // Second person of the conversation, that is other than current user
             "id": "58c67eec6e1b455d503d3b55",
             "name": "Cindy Nguyen",
             "avatar": "/Content/ProfilePictures/58c67eec6e1b455d503d3b55_profile_pic_636265657958560938.jpg",
             "online": false
           },
           "userIds": [
             "58c67eec6e1b455d503d3b55",
             "58cde4d96e1b1b3dd01c29ee"
           ],
           "users": [       // List of users participating in this ocnversation. Always 2
             {
               "id": "58c67eec6e1b455d503d3b55",
               "name": "Cindy Nguyen",
               "avatar": "/Content/ProfilePictures/58c67eec6e1b455d503d3b55_profile_pic_636265657958560938.jpg",
               "online": false
             },
             {
               "id": "58cde4d96e1b1b3dd01c29ee",
               "name": "Yamada Takai",
               "avatar": "/Content/ProfilePictures/58cde4d96e1b1b3dd01c29ee_profile_pic_636265670361150494.jpg",
               "online": false
             }
           ],
           "messages": [        // List of messages in ascending chronological order
             {
               "messageid": "598ec4ab6e1b1b7474e1e144",
               "created": "2017-08-12T09:04:43.066Z",
               "text": "Hello there",
               "fromMe": true,                              // fromMe = true means current user is the sender
               "toReceiverId": "58c67eec6e1b455d503d3b55",
               "conversationId": "58d36da86e1b2963a0c42548",
               "isread": false,
               "Isdeleted": false
             },
             {
               "messageid": "59c5d5656e1b1b399c5e0105",
               "created": "2017-09-23T03:30:45.916Z",
               "text": "Let me know",
               "fromMe": true,
               "toReceiverId": "58c67eec6e1b455d503d3b55",
               "conversationId": "58d36da86e1b2963a0c42548",
                "Isdeleted": true,                          // This message has been deleted
                "Datedeleted": "2017-08-13T11:55:33.697Z"   // Only if deleted
             },
             {
               "messageid": "598ec4d66e1b1b7474e1e145",
               "created": "2017-08-12T09:05:26.236Z",
               "text": "[DRFI Request]",                    // [ ] indicates special message, like this or [DRFI Response]
               "fromMe": true,
               "toReceiverId": "58c67eec6e1b455d503d3b55",
               "conversationId": "58d36da86e1b2963a0c42548",
               "isread": false,
               "Isdeleted": false,
               "type": "drfi",                      // Special message type, like DRFI
               "jsonFieldsdrfi": "[ ... ]"          // list of DRFI fields encoded as JSON string, internal format
             }
             // ...
           ]
         }
       ],
       ...
     }
                          
                          
## Notes

To get a full list of network members, typically to display the chat UI, call the [Network Members API](network.html), or
use data returned by the next endpoint (Conversation List API).
                         
To operate on a message (using the message operation endpoints below), it is necessary to also specify the conversation containing the message,
and the recipient of the message, which can be obtained from this list.

# GET `/Chat/Conversations/List`

> List all conversations with details

Retrieve the list of all conversations for current user with details, but not including the messages of each conversation.

The details include the member of each conversation, and message statistics.

## Params

None

## Response
 
     {
         "success": true,
         "message": "List 25 conversations",
         "data": [
             {                                             // Details of one conversation
                                                           // Each conversation corresponds to one party (network member)
                 "conversationId": "58cdeb1a6e1b1b3dd01c2a4f",
                 "fromAccountId": "f401327f-e99f-4eb5-8e32-8f412f8a000e",   // Account ID of party 
                 "fromProfile": {                                           // Profile of party
                     "id": "58c8ce2c6e1b1f2dbcdf0159",
                     "displayName": "Angela Simson",
                     "avatar": "/Content/ProfilePictures/58c8ce2c6e1b1f2dbcdf0159_profile_pic_636330375929107650.jpg"
                 },
                 "messagesCount": 3,                        // Total number of messages
                 "incomingMessagesCount": 3,                // Total number of incoming messages (party is sender)
                 "unreadMessagesCount": 1                   // Number of total unread messages (isread=false)
                 "unreadIncomingMessagesCount": 0           // Number of unread incoming messages (isread=false and party is sender)
                 "deletedMessagesCount": 0                  // Number of deleted messages (isdelete=true)
             },
             ...
         [
     }

# GET `/Chat/Conversation`

> Get all messages of a conversation

This call retrieves the full messaging history between current user and the specified user. The other user must be a member
in network.


## Params

Provide either an entity ID or an account ID of the user to query.

    ?withUserId=758cf43cd6e1b1b32388e2f42

or

    ?withAccountId=efbfae6c-b935-43cb-878c-dbea7166c78f

## Response
The format of the messages in the list is the same as that of the previous endpoint, with an exception:
information about DRFI fields is included if any. See [Interaction Details API](interaction-details.html)
for user data format.

     {
         "success": true,
         "message": "Conversation details",
         "data": {
             "id": "58cf43cd6e1b1b32388e2f42",          // Conversation ID
             "from": {                                  // User profile
                 "id": "58c680ed6e1b455d503d3b71",
                 "name": "Lincohn D.",
                 "avatar": "..."
             },
             "userIds": [
                 "58cde4d96e1b1b3dd01c29ee",
                 "58c680ed6e1b455d503d3b71"
             ],
             "messages": [                              // List of messages, same format as previous endpoint (see above)
                 {
                   "messageid": "598ec4d66e1b1b7474e1e145",
                   "created": "2017-08-12T09:05:26.236Z",
                   "text": "[DRFI Request]",            // [ ] indicates DRFI message
                   "fromMe": true,
                   "toReceiverId": "58c67eec6e1b455d503d3b55",
                   "conversationId": "58d36da86e1b2963a0c42548",
                   "isread": false,
                   "Isdeleted": false,
                   "type": "drfi",                      // Special message type, like DRFI, "drfi" is a DRFI request
                                                        // "drfiresponseaccepted" or "drfiresponsedenied" is a DRFI response
                                                        // This and all following properties only present in special case
                   "status": "sent"                     // Status, only for DRFI request, either "sent", "accepted" or "denied"
                   "drfiFields": [ ... ]                // List of DRFI fields, same format as interaction user data
                   "jsonFieldsdrfi": "[ ... ]"          // list of DRFI fields encoded as JSON string, internal format
                 },
                 {
                   ...
                 }
             ]
         }
     }
                          
# POST `/Chat/Message/Add`

> Add new message

Create and send a chat message to another user.

If any active FCM token(s) is found associated with the receiving user, the message is also pushed to FCM server using the token
as target, with data type set to `"message"`, and payload to the message object. See [FCM Messaging](fcm-messaging.html) for details.

## Params

Provide the ID of the enclosing conversation (as can be obtained from the conversation listing), the recipient user ID, and the text content.
All parameters are required.

Optionally indicate a DRFI request by providing the list of fields.

    {
    	conversationId: "58b3dc67689f5e3418c88302", // Conversation ID
    	ToReceiverId: "585a367553bc292828bdac1f",   // Recipient user ID
    	text: "A chat message",
    	type: "drfi"                                // Only in case of DRFI
    	drfiPaths: [                                // List of field paths as string array 
            ".basicInformation.firstName",
            ".contact.mobile",
            ...
            ]
    }

## Response

    {
        "success": true,
        "message": "Message created successfully",
        "data": {
            "conversationId": "58b3dc67689f5e3418c88302",
            "messageId": "5a216e4005100b3d9029dd71",     // ID of the message just created
            "fcmResponse": {                             // Response obtained by FCM Send, if any
                "multicast_id": 8892582981373553481,
                "success": 1,
                "failure": 0,
                "canonical_ids": 0,
                "results": [
                    {
                        "message_id": "0:1515994438299373%e609af1cf9fd7ecd"
                    }
                ]
            }
        }
    }

# POST `/Chat/Message/MarkRead/{messageId }`

> Mark read a message

The frontend client should call this after user has viewed or opened a new message. Only the receiver of the message is allowed.

## Params

Provide the ID of the message to delete, as route parameter. 

## Response

    {
        "success": true,
        "message": "Message marked read",
        "data": {
            "messageId": "5a4db19205100b19c0c56d85",
            "conversationId": "58b3dc67689f5e3418c88302",
            "isRead": true
        }
    }
    
# POST `/Chat/Conversation/MarkRead/{conversationId }`

> Mark read a conversation

Mark read all unread messages of a conversation. Only incoming messages (whose receiver is current user) are allowed.

## Params

Provide the ID of the conversation, as route parameter. 

## Response

    {
        "success": true,
        "message": "Conversation messages marked read",
        "data": {
            "conversationId": "58b3dc67689f5e3418c88302",
            "isRead": true,
            "messagesMarkedRead": 2             // Count of messages marked read as a result of this call
                                                // Can be 0 (if all incoming messages are already read)
        }
    }
    
# POST `/Chat/Message/Delete`

> Delete a message

## Params

Provide the ID of the message to delete. Other parameters are same as on creation, and all required. 

    {
    	conversationId: "58b3dc67689f5e3418c88302",
    	ToReceiverId: "5a216e4005100b3d9029dd71",
    	messageid: "5a21650305100b3f1454a8d1"
    }

## Response

    {
        "success": true,
        "message": "Message deleted successfully",
        "data": {
            "conversationId": "58b3dc67689f5e3418c88302",
            "messageId": "5a21650305100b3f1454a8d1"
        }
    }

# Overview on DRFI

**Dynamic Request For Information** is a special kind of person-to-person interaction
where an individual user asks another user to send back some personal information.
The whole DRFI flow is conduted via chat messaging in form of special messages.

## DRFI Request

The DRFI flow starts with a user creating a request within a conversation with another user.
The DRFI request indicates which information should be included, in form of a list of vault fields. 

To create a DRFI request, the client UI should:

- Display a list of preset fields (or group of fields) allowing user to select which to include in request (see Web app for an example)
- Allow user to search for vault fields to add to the list. Use the [Vault Fields API](vault.html) to get the full list
of available vault fields.

Once approved by user, the client should call the [Add Message API](#post-chatmessageadd) to post the DRFI request as a normal message, 
with these extra steps:

- Set the `type` to `"drfi"`
- Set the `drfiPaths` to the list of field paths as selected by user

See the API params for an example.

Once received by the other user, the DRFI request message will adopt status of `"sent"`.
The message will appear on the list obtained by the recipient via the [Conversation API](#get-chatconversation).
The `drfiFields` property will be set to the list of fields in addition to user data taken from the 
recipient's information vault, in the same exact format as the form data returned by the 
[Interaction Details API](interaction-details.html). The client UI should use this information
as hint to display to the recipient, allowing them to review and possibly change before responding to the request.

## DRFI Response

After reviewing their user data, the recipient of the DRFI request can respond in one of two ways
to resolve the request:

- **Accept** the request: The original request message status becomes `"accepted"`. A special message of type `drfiresponseaccepted` will be added to the conversation,
with `drfiFields` containing the list of fields and the user data submitted by the recipient. 
 The client UI will then display this response together with user data to the other user (sender of request).
- **Deny** the request: The original request message status becomes `"denied"`. A special message of type `drfiresponsedenied` will be added to the conversation,
with no user data. 
The client UI will then display this response to the other user (sender of request).

The DRFI flow thus completes; as the request has been resolved, no further action on it will be possible.
There is no limit to the number of flows that can be initiated at any time in one conversation
(between two certain users).

# POST `/Chat/Drfi/Accept`

> Accept a DRFI request

## Params

Provide ID of the message containing the DRFI request, together with user data, in JSON body.

The format and convention of user data is identical to that used by the [Interaction Register API](interaction-register.html).

The requirement of the data fields is not as limited though: any vault field can be included, even those not present
in the original request, provided all are valid data, and have been selected and approved by the accepting user.

Similar to interaction registration, all data values will be sent back to request owner, 
but only those fields with `source` set to `"newValue"` will be updated into accepting user's information vault.

    {
        "messageId": "5a67f16305100b0504addec9",    // ID of DRFI request message, required
        "fields": [                                 // List of fields with user data, required
            { source: "vaultCurrentValue", path: ".basicInformation.firstName", value: "Chris" },
            { source: "newValue", path: ".contact.mobile", value: "6590909090" },
            { source: "newValue", path: ".address.currentAddress.address", value: "..." },
            ...
        ]
    }
    
## Response

    {
        "success": true,
        "message": "DRFI request accepted",
        "data": {
            "requestMessageId": "5a67f16305100b0504addec9",     // ID of original request message
            "responseMessageId": "5a68987605100b450cef5e73",    // ID of response message sent as result
            "status": "accepted"
        }
    }
        
# POST `/Chat/Drfi/Deny`

> Decline a DRFI request

## Params

Provide ID of the message containing the DRFI request, as URL parameter

    ?messageId=5a67f16305100b0504addec9
    
## Response

    {
        "success": true,
        "message": "DRFI request denied",
        "data": {
            "requestMessageId": "5a67f16305100b0504addec9",     // ID of original request message
            "responseMessageId": "5a68987605100b450cef5e73",    // ID of response message sent as result
            "status": "denied"
        }
    }