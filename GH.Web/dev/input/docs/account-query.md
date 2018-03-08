Title: Query Account
Description: Account Query API
Category: Account Helpers
---
# GET `/Account/Exists`
> [Public]
> Check if account already exists, based on email address

## Params
Provide the email address to check, URL encoded

      ?email=son@regit.today

## Response
If an account associated with provided email address exists:

    {
        "success": true,
        "message": "Found account with that email",
        "accountEmail": "son@regit.today"
    }

If no account is found associated with provided email address:

    {
        "success": false,
        "message": "Found no account with that email",
        "email": "nobody@regit.today"
    }

# GET `/Account/Query`
> [Public]
> Query an account based on email address

Returns more details about the account than the previous endpoint. Useful for account recovery.

## Params
Provide the email address to check, URL encoded

      ?email=long@regit.today

## Response
If an account associated with provided email address exists:

    {
        "success": true,
        "message": "Account exists",
        "data": {
            "id": "58c8ce2c6e1b1f2dbcdf0159",
            "accountId": "f401327f-e99f-4eb5-8e32-8f412f8a000e",
            "displayName": "Angela Simson",
            "avatar": "/Content/ProfilePictures/58c8ce2c6e1b1f2dbcdf0159_profile_pic_636330375929107650.jpg",
            "country": "Italy",
            "city": "Acerra",
            "phoneLast4": "1298",
            "unverified": false
        }
    }

If no account is found associated with provided email address:

    {
        "success": false,
        "message": "Account not found",
        "error": "account.notFound"
    }


