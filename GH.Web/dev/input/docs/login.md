---
Title: Log In
Description: Log In API
---
# POST `/token`
> [Public]  
> Log in, creating new user session

Call without `/Api` prefix, eg. `http://demo.regit.today/token`

## Params
Provide credentials for the user logging in, encoded in body as HTTP form values (`Content-Type: application/x-www-form-urlencoded`)

    ?username = {username}
    &password = {password} 
    &grant_type = password
    
Also provide the FCM registration token to identify the client as recipient of FCM push notifications and messages
(see [FCM Messaging](fcm-messaging.html) for details).

    &fcm_token = "fwHEY938V6Q:APA9..."

## Response:

Authentication information, and for convenience, user profile is included in reponse.

     {
         "access_token": "4Uf8VSxvV0ZJZ_...3wiGxlRLs83",	// Token for this session
         "token_type": "bearer",
         "expires_in": 86399,	// Session expiry in seconds. Default 24 hours
         "success": true,
         "message": "Login successful",
         "userName": "son@regit.today",
         "accountId": "c52283cf-c77b-4433-ad69-9e8bc6451aa9",
         "profile": {
             "firstName": "Yamada",
             "lastName": "Takai",
             "displayName": "Yamada Takai",
             "avatar": "/Content/ProfilePictures/58cde4d96e1b1b3dd01c29ee_profile_pic_636265670361150494.jpg",
             "gender": "Male",
             "dob": "1965/10/29",
             "country": "Singapore",
             "city": "Sengkang"
         }
     }

The access token must be stored and used for subsequent API calls in this session. See more details about [Authentication](overview.html).

Query status of this token anytime later by using the [Token Query API](tokens.html).

If provided credentials are incorrect, an error is returned.

    {
        "success": false,
        "message": "Incorrect username or password",
        "error": "invalid_grant"
    }

