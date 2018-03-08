Title: Search
Description: Search API
---
# GET `/Search/People`

> Search for individual users

## Params
Provide query string to search, URL encoded

    ?query={query}
    
All users will be searched for this string against their profile (names and email address).

Provide optional extra parameters to limit results (also URL encoded): starting offset (0-based), and page size. Default values are below:

    &start=0&take=10

## Response

JSON encoded list of results matching query

    {
        "success": true,
        "message": "Found 2 items",
        "data": [
            {
                "Userid": "58c67a256e1b455d503d3b47",
                "UserAcccountid": "1699e8c3-17d2-447f-abe7-ba369b732c23",
                "PhotoUrl": "",
                "DisplayName": "Bob Lee",
                "Email": "quaydo@gmail.com",
                "Status": "I love Regit",
                "Description": "",
                "FirstName": "Bobby",
                "LastName": "Lee",
                "StatusFriend": "normal"
            },
            {
                "Userid": "58c67eec6e1b455d503d3b55",
                "UserAcccountid": "3a3e3b6e-36e2-49d9-8268-0cdd092bd297",
                "PhotoUrl": "/Content/ProfilePictures/58c67eec6e1b455d503d3b55_profile_pic_636265657958560938.jpg",
                "DisplayName": "Cindy Nguyen",
                "Email": "wife@regit.today",
                "Status": "I love design ...",
                "Description": "",
                "FirstName": "Kim",
                "LastName": "Nguyen",
                "StatusFriend": "trusted"
            }
        ]
    }

# GET `Users/Search`

> Search for individual users with email option

This function is similar to the previous endpoint, but returns more compact data. In addition, it allows to search individually, either for name only or email address only.

This is useful for UI to display a quick user search control with auto-complete feature (ie. querying the API continuously on every keystroke). The UI should allow user to select the search option.

## Params
Provide query string to search, URL encoded

    ?query={query}
    
Optionally, provide the property to search, either by name or email:

    ?query=john&by=name     // "name" is assumed if omitted
    ?query=son@regit.today&by=email
    
Unlike partial search for name query, the email query must match exactly the email address to search for.

Provide optional extra parameters to limit results: starting offset (0-based), and page size. Default values are below:

    &start=0&take=20

## Response

JSON encoded list of results matching query

    {
        "success": true,
        "message": "Found 2 items",
        "data": [
            {
                "Userid": "58c67a256e1b455d503d3b47",
                "UserAcccountid": "1699e8c3-17d2-447f-abe7-ba369b732c23",
                "PhotoUrl": "",
                "DisplayName": "Bob Lee",
                "Email": "quaydo@gmail.com",
                "Status": "I love Regit",
                "Description": "",
                "FirstName": "Bobby",
                "LastName": "Lee",
                "StatusFriend": "normal"
            },
            {
                "Userid": "58c67eec6e1b455d503d3b55",
                "UserAcccountid": "3a3e3b6e-36e2-49d9-8268-0cdd092bd297",
                "PhotoUrl": "/Content/ProfilePictures/58c67eec6e1b455d503d3b55_profile_pic_636265657958560938.jpg",
                "DisplayName": "Cindy Nguyen",
                "Email": "wife@regit.today",
                "Status": "I love design ...",
                "Description": "",
                "FirstName": "Kim",
                "LastName": "Nguyen",
                "StatusFriend": "trusted"
            }
        ]
    }

# GET `/Search/Business`

> Search for business users

## Params
Same as People Search

## Response

    {
        "data": [
            {
                "Userid": "58c6622b6e1b435d50a853a7",
                "UserAcccountid": "25d6bc5d-8931-43f0-8ba2-87c2ed798eab",
                "PhotoUrl": "/Content/ProfilePictures/58c6622b6e1b435d50a853a7_profile_pic_636250322947178559.jpg",
                "DisplayName": "East Asia School",
                "Email": "school@regit.today",
                "Status": null,
                "Description": "This is the official site of East Asia School",
                "FirstName": "School",
                "LastName": "East Asian",
                "StatusFriend": "Followed"
            },
            {
                "Userid": "58c665446e1b435d50a853af",
                "UserAcccountid": "1c5b4703-9635-4cbf-910f-13f8c4b08668",
                "PhotoUrl": "/Content/ProfilePictures/58c665446e1b435d50a853af_profile_pic_636312147818402454.png",
                "DisplayName": "ACB Bank",
                "Email": "info@acb.com",
                "Status": null,
                "Description": "The Winnie State Bank began construction in the summer of 1907, however before completion the bank and building was purchased by H.B. Kleiwer under the name of Alfalfa County National Bank.\r\n\r\nThe bank opened for business on January 29, 1908.\r\n\r\nIn February 1977 the charter was changed to Alfalfa County Bank.\r\n\r\nOn June 14, 2004, the bank relocated to 323 S. Grand into a brand new building and a new charter under the name of ACB Bank.\r\n\r\nThe bank is locally owned and operated and continues to be a strong asset to the community.\r\n\r\nIn 2006, we acquired branches in Waukomis and Garber. The strength of the bank is indicated by its tremendous growth.",
                "FirstName": "Bank",
                "LastName": "ACB",
                "StatusFriend": "Followed"
            }
        ]
     }
     
# GET `/Search/PublicBusiness`

> Search for user created businesses

See more on [User Created Business](ucb.html)

## Params
Same as Business Search. Default limit parameters as follow:

    ?start=0&take=5

## Response
    
    {
        "success": true,
        "message": "Found 5 businesses",
        "data": [
            {
                "id": "59b7b46fc73b2e048892e8e7",
                "name": "Association of International Accountants, Singapore Branch",
                "industry": "Association",
                "country": "Singapore",
                "city": " Singapore (City)",
                "address": " Singapore (Address)",
                "phone": "+6564253761",
                "email": "aiasin@aiasingapore.com.sg",
                "website": "www.aiaworldwide.com",
                "avatar": "/Content/UploadImages/_ucb_profile_1505991275657.png",
                "description": "",
                "status": "approved"
            },
            ...
        ]
    }
    
  
# GET `/Search/Interactions`

> Search for interactions

Search in all targeted interactions for current user (same as individual feed)

## Params
Same as User Search. 

## Response
JSON encoded list of results, listing interactions that match query. 

    {
        "data": [
        
            {
                "CampaignId": "59ecd2d76e1b1b378ce44644",
                "CampaignType": "Registration",
                "Name": "New Registration for Jan 2018 Class",
                "BusinessId": "58cde4d96e1b1b3dd01c29ee",
                "BusinessUserId": "25d6bc5d-8931-43f0-8ba2-87c2ed798eab",
                "BusinessName": "East Asia School",
                "BusinessImageUrl": "/Content/ProfilePictures/58c6622b6e1b435d50a853a7_profile_pic_636250322947178559.jpg",
                "UserName": "East Asia School",
                "termsAndConditionsFile": null,
                "usercodetype": "Free",
                "usercode": "",
                "usercodecurrentcy": "",
                "UserId": null,
                "Description": "",
                "Image": "/Content/UploadImages/25d6bc5d-8931-43f0-8ba2-87c2ed798eabCampaign20171023011612.jpg",
                "TargetLink": "www.regit.today",
                "Verb": "accept",
                "Following": true,
                "Participants": 1,
            },
            {
                "CampaignId": "59e47fa66e1b28256053413d",
                "CampaignType": "Registration",
                "Name": "Registration - New Newsletter",
                "BusinessId": "58cde4d96e1b1b3dd01c29ee",
                "BusinessUserId": "25d6bc5d-8931-43f0-8ba2-87c2ed798eab",
                "BusinessName": "East Asia School",
                "BusinessImageUrl": "/Content/ProfilePictures/58c6622b6e1b435d50a853a7_profile_pic_636250322947178559.jpg",
                "UserName": "East Asia School",
                "termsAndConditionsFile": null,
                "usercodetype": "Free",
                "usercode": "",
                "usercodecurrentcy": "",
                "Image": "",
                "TargetLink": "www.regit.today",
                "Verb": "register",
                "Following": true,
                "Participants": 1,
            }
        ]
    }

Some of the fields returned are obsolete, which are omitted from the example above. Use only those needed.