Title: Delegation
Description: Delegation API
---
# GET `/Delegation/List/In`

> List inbound delegations for current user

Inbound delegations are those initiated by another user, who delegates information to you.

## Params
None

## Response

    {
        "success": true,
        "message": "List inbound delegations",
        "data": [
            {
                "delegationId": "594cd3c26e1b251b98507825",
                "delegationRole": "Normal",         // Or "Super", "Custom", "Emergency"
                "direction": "DelegationIn",        // Inbound
                "fromAccountId": "3a3e3b6e-36e2-49d9-8268-0cdd092bd297",    // Delegator account Id
                "fromProfile": {                    // Delegator profile
                    "id": "585a367553bc292828bdac1f",   
                    "displayName": "John Doe",
                    "avatar": "/Content/ProfilePictures/efbfae6c-b935-43cb-878c-dbea7166c78f_profile_pic_636306474463863309.png"
                },
                "toAccountId": "c52283cf-c77b-4433-ad69-9e8bc6451aa9",      // Delegatee (current user)
                "status": "accepted",               // or "pending" (see next item)
                "begins": "2017-06-23",             // Effective date
                "expires": "Indefinite",            // or an expiry date
                "permissions": null                 // not applicable to Normal or Super role
            },    
            {
                "delegationId": "5a2512f405100b4180b1824c",
                "delegationRole": "Custom",         
                "direction": "DelegationIn",         
                "fromAccountId": "3a3e3b6e-36e2-49d9-8268-0cdd092bd297",    
                "toAccountId": "c52283cf-c77b-4433-ad69-9e8bc6451aa9",      
                "status": "pending",                 // Delegation request
                "begins": "2017-11-23",
                "expires": "2018-01-01",
                "permissions": [                    // Only applicable to Custom or Emergency role
                    {
                        "name": "Basic Information",
                        "read": true,
                        "write": false,
                        "jsonpath": "basicInformation"
                    },
                    {
                        "name": "Contact",
                        "read": true,
                        "write": true,
                        "jsonpath": "contact"
                    },
                    {
                        "name": "Address",
                        "read": false,
                        "write": false,
                        "jsonpath": "address"
                    },
                    {
                        "name": "Govenment ID",
                        "read": false,
                        "write": true,
                        "jsonpath": "governmentID"
                    },
                    {
                        "name": "Education",
                        "read": false,
                        "write": false,
                        "jsonpath": "education"
                    },
                    {
                        "name": "Employment",
                        "read": true,
                        "write": false,
                        "jsonpath": "employment"
                    },
                    {
                        "name": "Family",
                        "read": false,
                        "write": false,
                        "jsonpath": "family"
                    },
                    {
                        "name": "Membership",
                        "read": true,
                        "write": true,
                        "jsonpath": "membership"
                    },
                    {
                        "name": "Financial",
                        "read": false,
                        "write": false,
                        "jsonpath": "financial"
                    },
                    {
                        "name": "Other",
                        "read": false,
                        "write": false,
                        "jsonpath": "others"
                    }
                ]
            },
            {
                "delegationId": "594cd3c26e1b251b98507825",
                "delegationRole": "Emergency",
                "direction": "DelegationIn",
                "fromAccountId": "3a3e3b6e-36e2-49d9-8268-0cdd092bd297",
                "fromProfile": { ... },
                "toAccountId": "c52283cf-c77b-4433-ad69-9e8bc6451aa9",
                "status": "accepted",
                "begins": "2017-06-23",
                "expires": "2017-07-01",
                "permissions": [
                    {
                        "name": "Basic Information",
                        "read": true,
                        "write": false,
                        "jsonpath": null,
                        "jsonpaths": [          // Only applicable for Emergency role, the list of permission paths
                            "basicInformation.firstName",
                            "basicInformation.lastName",
                            "basicInformation.dob"
                        ]
                    },
                    ...
                ]
            }
        ]
    }
    

# GET `/Delegation/Details/{delegationId }`
> Get details about a delegation

## Params
Provide ID of the delegation. Current user must be a member of this relationship (either delegatee or delegator).

## Response
Same format as that of the delegation listing; user profiles of both delegator and delegatee are included

    {
        "success": true,
        "message": "Delegation details",
        "data": {
                "delegationId": "594cd3c26e1b251b98507825",
                "delegationRole": "Normal",   
                "direction": "DelegationIn",     
                "fromAccountId": "3a3e3b6e-36e2-49d9-8268-0cdd092bd297",
                "fromProfile": {                    // Delegator profile
                    "id": "585a367553bc292828bdac1f",   
                    "displayName": "John Doe",
                    "avatar": "/Content/ProfilePictures/efbfae6c-b935-43cb-878c-dbea7166c78f_profile_pic_636306474463863309.png"
                },
                "toAccountId": "c52283cf-c77b-4433-ad69-9e8bc6451aa9",
                "toProfile": {                      // Delegatee profile
                    "id": "58ae58b3689f5e2f84b32635",
                    "displayName": "Jane Doe",
                    "avatar": "/Content/ProfilePictures/f8b00a20-e88d-4a0f-a0f2-3c07f896b2b7_profile_pic_636509367350487761.jpg"
                },    
                "status": "accepted",              
                "begins": "2017-06-23",      
                "expires": "Indefinite",        
                "permissions": null                 
        }
    }

# POST `/Delegation/Accept`
> Accept a delegation request

## Params:
Provide ID of the delegation, URL encoded

    ?delegationId=5a250a3905100b2bf0ec3cb7

## Response
    {
        "success": true,
        "message": "Delegation accepted",
        "data": {
            "delegationId": "5a250a3905100b2bf0ec3cb7",
            "status": "accepted"
        }
    }
    
# POST `/Delegation/Deny`
> Deny a delegation request

The delegation will be removed as a result.

## Params:
Provide ID of the delegation, URL encoded

    ?delegationId=5a250a3905100b2bf0ec3cb7

## Response
    {
        "success": true,
        "message": "Delegation denied",
        "data": {
            "delegationId": "5a250a3905100b2bf0ec3cb7",
            "status": "removed"
        }
    }