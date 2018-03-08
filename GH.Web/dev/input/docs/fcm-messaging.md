Title: FCM Messaging
Description: FCM Messaging Support
---

Regit Web app implements SignalR for real-time communication (push notifications and instant messages). 
To enable mobile clients to achieve the same function, in addition to offline queuing, Regit supports **Firebase Cloud Messaging**. 
All communication content requiring push operations will be sent to FCM cloud server for distribution to all targeted clients.

To allow the API to identify the proper client to deliver content, at time of logging in, 
every mobile client must provide the **FCM Registration Token** obtained for that client. See [Login API](login.html) for details.

On sucessfully login, the server stores that token as part of the user's session data. Every time an app notification is created
by system, or a chat message is added by a user, it will also be sent to the cloud FCM API using the stored FCM token associated with the receiving user
to identify the target client.

Data is posted as JSON, whose format depends on the communication type, which is either **Notification** or **Message**.

# FCM format of app notification

    { 
        "notification": {
            "title": "Notification from {fromUserDisplayName}",
            "body": "..."                                // Text of notification
        },
        "data": {
            "type": "notification",
            "text": "...",                              // Text of notification
            "fromAccountId": "...",                     // Account ID of user that initiated the notification
            "fromDisplayName": "...",                   // Display name of that user
            "fromAvatar": "...",                        // Picture URL of that user
            "json": "{...}"                             // JSON string of the notification object
        }
    }
    
The notification object serialized in the `json` payload is in the same format as returned by the 
[Notifications API](nofifications.html).

# FCM format of chat message

    { 
        "notification": {
            "title": "Messsage from {fromUserDisplayName}",
            "body": "..."                               // Text of message
        },
        "data": {
            "type": "message",
            "text": "...",                              // Text of message
            "fromAccountId": "..."                      // Account ID of user that sent the message
            "fromDisplayName": "...",                   // Display name of that user
            "fromAvatar": "...",                        // Picture URL of that user
            "json": "{...}"                             // JSON string of the message object
        }
    }
    
The message object serialized in the `json` payload is in the same format as returned by the 
[Messaging API](messaging.html).