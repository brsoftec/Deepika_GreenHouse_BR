Title: Following
Description: Following API
---
# GET `/Interaction/IsFollowing/{businessId }`
> Check if current user is following a business

## Params
Entity ID of the business to check

## Response

    {
        "success": true,
        "message": "Following business",
        "data": {
            "following": true,      // or false
            "businessId": "58c6720b6e1b455d503d3b3e"
        }
    }


# POST `/Interaction/Follow/{businessId }`
> Follow a business

## Params
Entity ID of the business to follow

## Response
    
    {
        "success": true,
        "message": "Followed business",
        "data": {
            "following": true,
            "businessId": "58c6720b6e1b455d503d3b3e"
        }
    }

# POST `/Interaction/Unfollow/{businessId }`
> Unfollow a business

## Params
Entity ID of the business to unfollow

## Response
    {
        "success": true,
        "message": "Unfollowed business",
        "data": {
            "following": false,
            "businessId": "58c6720b6e1b455d503d3b3e"
        }
    }