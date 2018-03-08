Title: Profile
Description: User Profile API
---
# GET `/Users/BasicProfile`

> Get current user's basic profile information

## Params
None
## Response

    {
        "id": "58cde4d96e1b1b3dd01c29ee",   // Entity ID
        "accountId": "c52283cf-c77b-4433-ad69-9e8bc6451aa9",   // Identity ID
        "displayName": "Yamada Takai",
        "avatar": "/Content/ProfilePictures/58cde4d96e1b1b3dd01c29ee_profile_pic_636265670361150494.jpg"
    }

## Notes
Every account has two associated unique IDs:
* __Entity ID__: database record ID
* __Identity ID__: .NET Identity account ID

Identifying an account requires either ID, depending on context. By convention, Entity ID is usually represented as `id` or `userId`. Identity ID is usually represented as `accountId`.

# GET `/Users/Profile`
> Get current user's main profile information

## Params
None
## Response

    {
        "success": true,
        "message": "Current user profile",
        "data":    {
            "id": "58cde4d96e1b1b3dd01c29ee",
            "accountId": "c52283cf-c77b-4433-ad69-9e8bc6451aa9",
            "displayName": "Yamada Takai",
            "firstName": "Yamada",
            "lastName": "Takai",
            "avatar": "/Content/ProfilePictures/58cde4d96e1b1b3dd01c29ee_profile_pic_636265670361150494.jpg",
            "description": "I grew up in Hiroshima, where US had dropped the atomic bomb during WWII. Grown-ups would often tell us about the war.",
            "dob": "10/29/1965 4:04:53 PM",
            "gender": "Male",
            "location": {
                "country": "Singapore",
                "city": "Sengkang",
                "street": null,
                "zipCode": null
            }
            }
       }
     }

# GET `/AccountSettings/Profile`
> Get current user's full profile information

## Params
None
## Response
Omitting unused/obsolete fields

    {
        "success": true,
        "message": "Current user full profile",
        "data": {
            "IsAdmin": false,
            "Id": "58cde4d96e1b1b3dd01c29ee",
            "AccountId": "c52283cf-c77b-4433-ad69-9e8bc6451aa9",
            "DisplayName": "Yamada Takai",
            "FirstName": "Yamada",
            "LastName": "Takai",
            "Email": "son@regit.today",
            "PhotoUrl": "/Content/ProfilePictures/58cde4d96e1b1b3dd01c29ee_profile_pic_636265670361150494.jpg",
            "Status": "I grew up in Hiroshima, where US had dropped the atomic bomb during WWII. Grown-ups would often tell us about the war.",
            "Country": "Singapore",
            "City": "Sengkang",
            "Street": null,
            "Region": null,
            "ZipPostalCode": null,
            "Description": null,
            "AccountType": "Personal",
            "Birthdate": "10/29/1965",
            "Phone": null,
            "WebsiteURL": null,
           "AnswerSercurityQuestions": {
            "Question1": {
                "QuestionId": "5818a3e9e66cc104cc5fb826",
                "Question": "What was your childhood nickname?",
                "Answer": "John"
            },
            "Question2": {
                "QuestionId": "5818a3e9e66cc104cc5fb82b",
                "Question": "What was the name of your elementary / primary school?",
                "Answer": "Lacas"
            },
            "Question3": {
                "QuestionId": "5818a3e9e66cc104cc5fb82d",
                "Question": "In what city did you meet your spouse/significant other?",
                "Answer": "Singapore"
            }
        }
    }



# GET `/Users/BasicProfile/{userId }`

> Get a specific user's basic profile information

## Params
Entity ID of user account
## Response
Same as current user's basic profile

# GET `/Users/BasicProfile/`

> Get a specific user's basic profile information, using account ID as key

## Params:
Account ID of user account, URL encoded

    &accountId=c52283cf-c77b-4433-ad69-9e8bc6451aa9
    
## Response
Same as preceding endpoint which queries by entity ID

# GET `/Users/Profile/{userId }`

> Get specific user's main profile information

This API can query business account profile as well; see [Business Profile](business-profile.html).

## Params
Entity ID of user account
## Response
Same as current user's main profile

# POST `/Profile/Set`

> Update current user's profile

## Params
Provide the profile property to set, as key-value pair, in JSON body.

    { 
        key: "firstname",       // Property key
        value: "John"           // String value
    } 

The property key is case-insensitive, and should be one of the following:

- **DisplayName**, **FirstName**, **MiddleName**, **LastName**, **Description**
- **Gender**: value must be either _"Male"_, _"Female"_, or _"Other"_.
- **DOB**: value must be string in `YYYY-MM-DD` format.
- **Country**, **City**, **Address**, **ZipCode**

## Response

    {
        "success": true,
        "message": "Profile updated successfully",
        "data": {
            "key": "firstname",
            "status": "updated"
        }
    }

# POST `/Profile/SetMany`

> Update multiple properties current user's profile

## Params
Provide the profile properties to set, as array of key-value pairs, in JSON body.

    {
        properties: [
            { 
                key: "firstname",
                value: "John"
            },
            { 
                key: "lasttname"
                value: "Doe"
            } 
        ]
    }

Each property is in the same format as the previous endpoint (Set profile single property).

## Response

    {
        "success": true,
        "message": "Profile updated successfully",
        "data": {
            "key": "firstname",
            "status": "updated"
        }
    }

# POST `/Profile/Picture`

> Update current user's profile picture

## Params
Provide the content of the image file, encoded in **multipart/form-data** format 
(same as that required by the [Upload API for Registration](interaction-register.html)).

## Response

    {
        "success": true,
        "message": "Profile picture updated",
        "data": {
            "pctureUrl": "/Content/ProfilePictures/f8b00a20-e88d-4a0f-a0f2-3c07f896b2b7_profile_pic_636509367350487761.jpg",
            "status": "updated"
        }
    }

