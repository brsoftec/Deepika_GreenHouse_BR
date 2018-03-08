Title: User Created Business
Description: UCB API
---

User Created Business (UCB) is a special type of business on Regit that is:

- Submitted by user and approved by Regit
- Featured with a profile of submitted public information
- Assigned no user account
- Available for claiming ownership by business owner

# GET `/Ucb/Profile/{id }`

> [Public]    
> Get profile of a business

## Params
Provide ID of the User Created Business

## Response

    {
        "success": true,
        "message": "Found businesses",
        "data": {
            "id": "59c39ab16e1b3009040621fa",
            "name": "Ano",
            "industry": "Computer Software",
            "country": "Viet Nam",
            "city": "Ho Chi Minh",
            "address": "90 Thang Long",
            "phone": "+84979888227",
            "email": "vu.nh79@yahoo.com",
            "website": "http://google.com",
            "avatar": null,
            "description": "Software",
            "status": "approved"
        }
    }

## Notes

To retrieve a list of User Created Businesses, use the [Public Business Search API](search.html).

# POST `/Ucb/Claim`

> [Public]    
> Claim a business

Send a claim to Regit staff for processing

## Params
    { 
        "ucbId": "59c39ab16e1b3009040621fa",    // ID of the business
        "ucbName": "Ano",                       // Name of the business
        "name": "Business Owner",               // Name of claimer
        "phone": "6590909090",                  // Phone number of claimer
        "email": "owner@business.com",          // Email address of claimer
        "message": "I want to claim this business"  // Message entered by claimer
    } 

## Response
    
    {
        "success": true,
        "message": "Business claim sent",
        "data": {
            "ucbId": "59c39ab16e1b3009040621fa"
        }
    }

