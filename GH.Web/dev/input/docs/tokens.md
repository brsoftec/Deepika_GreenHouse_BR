Title: Tokens
Description: Access Token Manager API
---
# GET `/Account/Token/Query`
> Query current access token

## Params
None. The current access token used for authentication is implied.

## Response

Server tracked information about the token is returned in details.

    {
        "tokenId": "5a0d40bb6e1b1d49ac4c9b33",
        "accountId": "c52283cf-c77b-4433-ad69-9e8bc6451aa9",        // Account ID of token owner
        "accessToken": "3cvG8l_OevjiNTKX6J0M_...
                    ... 76Y43R7BO2kotss2JvCVqNug",                   // The access token, same as query
        "issued": "2017-11-16T07:39:34Z",                           // Dates in ISO format
        "expires": "2017-11-17T07:39:34Z",
        "expiresIn": 1428.08,                        // Time left until expiry, in minutes
        "clientInfo":  {   // Client information obtained from original authentication request
            "ip": "27.64.48.235",
            "host": "demo.regit.today",
            "ua": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36"
             }
    }


# POST `/Account/Token/Query`
> [Public]
> Query given access token

## Params
Provide the exact access token string to query against.

        "3cvG8l_OevjiNTKX6J0M_...
         ... 76Y43R7BO2kotss2JvCVqNug"

It must be a valid Regit access token, obtained via a previous call to [Log In API](login.html), though not necessarily owned by the caller. That is, a single user account can request multiple tokens across devices, and any client can use this API to track all tokens shared by other clients.

This endpoint serves the same function as the preceding one, but in contrast it employs POST, instead of GET, as HTTP method, to transmit the required parameter.

__Important__: To prevent error, make sure the parameter is properly encoded:

- Put as text in the HTTP body, not URL or header
- Enclose value in double quotes, leaving no space outsite the quotes
- Post as JSON content (`Content-Type: application/json`), not text

## Response

If an access token is found in authentication log that exactly matches the query, server tracked information about the token is returned, including owner and expiry details.

The result is identical to that of the preceding function (Query current access token), but may represent an expired (inactive) token.

    {
        ...
        "expiresIn": 0,      // 0 means this token has expired
        ...
    }

If no access token is found, a 404 error is returned.
