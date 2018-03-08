Title: Notifications
Description: User Notifications API
---
# GET `/Notifications/List`

 > Get notifications for current user

 ## Params
 Provide starting offset and list size, URL encoded

     ?start=0&take=5

 Both parameters are optional, if absent default to start=0 & take=10

 ## Response

 JSON encoded list of notifications as limited by the parameters, sorted by descending date (latest first)

    {
        "success": true,
        "message": "List 5 items",
        "data": [
          {
             "Id": "59f315896e1b1f1b783c16cf",
             "Category": "handshake",
             "Type": "Acknowledge Handshake",
             "DateTime": "2017-10-27T11:16:25.3956579Z",
             "FromAccountId": "25d6bc5d-8931-43f0-8ba2-87c2ed798eab",
             "FromUserDisplayName": "East Asia School",
             "FromProfile": {
                 "id": "58c6622b6e1b435d50a853a7",
                 "displayName": "East Asia School",
                 "avatar": "/Content/ProfilePictures/58c6622b6e1b435d50a853a7_profile_pic_636250322947178559.jpg"
             },
             "ToAccountId": "c52283cf-c77b-4433-ad69-9e8bc6451aa9",
             "ToUserDisplayName": "Yamada Takai",
             "Title": "East Asia School has acknowledged your update",
             "Content": null,
             "Options": null,
             "PreserveBag": "59f2650d6e1b1b2938995ff1",
             "Payload": "",
             "Read": true,
             "BlockDetail": false
         },
           {
             "Id": "59f3131b6e1b1f1b783c16c0",
             "Category": "handshake",
             "Type": "Invited Handshake",
             "DateTime": "2017-10-27T11:06:03.8242474Z",
             "FromAccountId": "1473fb9a-a384-41ec-b142-8debf07b334c",
             "FromUserDisplayName": "Student Club",
             "FromProfile": { ... },    
             "ToAccountId": "c52283cf-c77b-4433-ad69-9e8bc6451aa9",
             "ToUserDisplayName": "Yamada Takai",
             "Title": "Student Club has invited you to join a handshake.",
             "Content": "",
             "Options": null,
             "PreserveBag": "59f312f56e1b1f1b783c16bc",
             "Payload": "",
             "Read": true,
             "BlockDetail": false
         },
           {
             "Id": "59f264aa6e1b1b2938995fe4",
             "Category": "srfi",
             "Type": "Invited SRFI",
             "DateTime": "2017-10-26T22:41:46.8755645Z",
             "FromAccountId": "25d6bc5d-8931-43f0-8ba2-87c2ed798eab",
             "FromUserDisplayName": "East Asia School",
             "FromProfile": { ... },    
             "ToAccountId": "c52283cf-c77b-4433-ad69-9e8bc6451aa9",
             "ToUserDisplayName": null,
             "Title": "East Asia School has requested you to submit your personal information",
             "Content": null,
             "Options": null,
             "PreserveBag": "59f264826e1b1b2938995fdf",
             "Payload": "",
             "Read": true,
             "BlockDetail": false
         },
         {
             "Id": "59c382376e1b1b090497f173",
             "Category": "network",
             "Type": "Accept Friend",
             "DateTime": "2017-09-21T09:11:19.5330804Z",
             "FromAccountId": "52209b9e-26ad-494f-9218-734367ae411e",
             "FromUserDisplayName": "Duc Le",
             "FromProfile": { ... },  
             "ToAccountId": "c52283cf-c77b-4433-ad69-9e8bc6451aa9",
             "ToUserDisplayName": "Yamada Takai",
             "Title": "Duc Le has accepted you to their network.",
             "Content": "You accepted Yamada Takai join your network",
             "Options": null,
             "PreserveBag": "59c3822c6e1b1b090497f16e",
             "Payload": "",
             "Read": true,
             "BlockDetail": false
         },
         {
             "Id": "59c381746e1b1b090497f166",
             "Category": "network",
             "Type": "Accept Friend",
             "DateTime": "2017-09-21T09:08:04.4911610Z",
             "FromAccountId": "52209b9e-26ad-494f-9218-734367ae411e",
             "FromUserDisplayName": "Duc Le",
             "FromProfile": { ... },   
            "ToAccountId": "c52283cf-c77b-4433-ad69-9e8bc6451aa9",
             "ToUserDisplayName": "Yamada Takai",
             "Title": "Duc Le has accepted you to their network.",
             "Content": "You accepted Yamada Takai join your network",
             "Options": null,
             "PreserveBag": "59c381696e1b1b090497f161",
             "Payload": "",
             "Read": true,
             "BlockDetail": false
         }
       ]
    }

 ## Notes
 Calling this API involves pull-based notifications. For live update of latest notifications, use [FCM support](fcm-messaging.html).
 
 # POST `/Notifications/MarkRead/{notificationId }`
 
 > Mark a notification as read
  
 Typically do this to signify that an action associated with the notification has been resolved successfully.
 
 ## Params
 Provide ID of the notification
  
 ## Response
    
    {
        "success": true,
        "message": "Notificiation marked read",
        "data": {
            "notificationId": "5a225bb905100b1a9ccbb3b0",
            "isRead": true
        }
    }