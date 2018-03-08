Title: Business Profile
Description: Business Profile API
---
# GET `/Users/BusinessProfile/{businessId }`

> Get public profile of a business

Use this API typically to display information on the profile page of a business

## Params:
Entity ID of business user account

## Response
    {
        "success": true,
        "message": "Business user profile",
        "data": {
            "id": "58c6720b6e1b455d503d3b3e",
            "accountId": "e335a9e1-0abd-488a-b962-b9600d154eb2",
            "displayName": "Indoma Media",
            "avatar": "/Content/ProfilePictures/76639415431145a8abede1f36fe4c0de_profile_pic_636250259313464294.jpeg",
            "publicPhone": "+6591234567",
            "publicEmail": "media@regit.today",
            "description": "We are a media business",
            "industry": "Design,Marketing and Advertising,Motion Pictures and Film",
            "website": "www.media.today",
            "workTimeString": "07:00AM - 07:00PM : Tue\n07:10AM - 06:00PM : Wed Thu\n07:00AM - 06:00PM : Mon Fri Sat Sun",
            "workTime": {   ...              // Work hours data, for web app only. Use above instead
            },
            "pictureAlbum": [
                "/Content/BusinessAlbumPictures/58c6720b6e1b455d503d3b3e_picture_album_636362624252898287_1.png",
                "/Content/BusinessAlbumPictures/58c6720b6e1b455d503d3b3e_picture_album_636362624252908274_2.png"
            ],
            "location": {
                "country": "Philippines",
                "city": null,
                "street": "789 Solota",
                "zipCode": null
            }
        }
    }
    
# GET `/Interaction/List/Srfi`

> Get list of active SRFI's from a business

**SRFI** (_Static Request for Information_) is a special type of interaction not distributable to newsfeed, but can be
initiated by individual user on demand. A list of SRFI's is typically shown on the business profile page, where
visitor can select one from the list to start the interaction.

## Params:
Provide ID of business to query from. Either Entity ID or Account ID is accepted.

    ?businessId=58c6622b6e1b435d50a853a7
    
or

    ?businessAccountId=25d6bc5d-8931-43f0-8ba2-87c2ed798eab

## Response
    
    {
        "success": true,
        "message": "Found 20 active SRFIs",
        "data": [
            {
                "id": "598579b46e1b1b2cc0b663c4",
                "businessAccountId": "25d6bc5d-8931-43f0-8ba2-87c2ed798eab",
                "name": "Sep 2017 Start - Newsletter",
                "description": ""
            },
            {
                "id": "5987e5d06e1b1b73f438a02e",
                "businessAccountId": "25d6bc5d-8931-43f0-8ba2-87c2ed798eab",
                "name": "East Asia Economic Student Club",
                "description": "Sign up for the East Asia Student Club"
            },
            {
                "id": "5987ea496e1b1b73f438a03b",
                "businessAccountId": "25d6bc5d-8931-43f0-8ba2-87c2ed798eab",
                "name": "East Asia Mountain Climbing Club",
                "description": "Eliminate your fear of height and stay active with the East Asia Mountain Climbing Club"
            },
            {
                "id": "598899b26e1b755150dc7473",
                "businessAccountId": "25d6bc5d-8931-43f0-8ba2-87c2ed798eab",
                "name": "Swimming Club 2017",
                "description": "Swimming club so you can swim as fast as lightning"
            },
            {
                "id": "598c40066e1b1e63b4bb9c28",
                "businessAccountId": "25d6bc5d-8931-43f0-8ba2-87c2ed798eab",
                "name": "Mount Everest Climb Expedition",
                "description": ""
            },
            ...
        ]
    }

## Implementation Notes

Use the `name` as label to populate the list to open when user typically clicks a "_Submit Information_" button. Then on user selecting one in the list, use the 
interaction ID to call the [Interaction Details API](interaction-details.html) for data to show on the opening form, just like what happens when user clicks "_Register_"
on a feed interaction.