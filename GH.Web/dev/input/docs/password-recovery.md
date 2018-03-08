Title: Password Recovery
Description: Password Recovery API
---
The password recovery process consists of three steps, in the following order:
1. Request to reset password
2. Verify security code
3. Reset (change) password

Corresponding to the following API endpoints:

# POST `/Account/RecoverPassword`

> [Public]    
> Request to reset password, sending a 2FA PIN

## Params
Provide email address associated with account, and the desired delivery method

        {
            Option: 0,   // Verification method: 0 = Email (default), 1 = SMS
            VerifyInfo: {
                Email: "son@regit.today"
            }
        }

## Response
Success result includes the __2FA Request ID__

    {
        "success": true,
        "message": "Verification message sent to phone",    // or "email" depending on method
        "data": {
            "requestId": "b75af096-fccd-4f18-ab3a-d7f81938c033",
            "expires": "2017-11-11T13:38:00.7232386+07:00",
            "phoneLast4": "9090"    // Last 4 digits of phone number for hinting user
        }
    }

Use this one ID for subsequent calls to the next endpoints below. Note the imposed expiry, which is 10 minutes by default. 
Make sure to proceed to next call within this period.

## Notes
This can be called repeatedly to initiate a new request flow (re-sending new PIN). The new request will replace the outstanding one.
Make sure to wait at least 60 seconds between such attempts, to prevent unexpected throttling issue. 

# POST `/Account/RecoverPasswordCheck`

> [Public]    
> Verify password recovery token

## Params
Provide the Recovery Request ID obtained by previous call to Request Reset Password API, and the user-entered security code (token)

     {
        Email: "son@regit.today",
        RequestId: "b75af096-fccd-4f18-ab3a-d7f81938c033"
        Token: "1234"     // The code user received via email or SMS
     }


## Response

    {
        "success": true,
        "message": "Token verified successfully",
        "data": {
            "requestId": "b75af096-fccd-4f18-ab3a-d7f81938c033",
            "expires": "2017-11-11T13:38:00.7232386+07:00"
        }
    }

Keep the Request ID and token for subsequent call to Reset Password API next below. Make sure to do this
before expiry.

Be careful to check for unsuccessful result that may be caused by:

- Request ID not found or invalid
- Incorrect token
- The token has expired 
- The token has been cancelled, typically due to too many failed retries

# POST `/Account/ResetPassword`

> [Public]    
> Reset password

## Params
Provide the new user-entered (or app generated) password, accompanied by the final 2FA resolution (Request ID and token obtained by previous calls). 
The token must have been verified by the previous call and has not expired.

     {
        Email: "son@regit.today",
        Password: "Test@234",    // New password
        RequestId: "bb75af096-fccd-4f18-ab3a-d7f81938c033",
        Token: "1234"
     }

## Response

    {
        "success": true,
        "message": "Password changed successfully",
        "data": {
            "accountId": "f52283cf-c77b-4433-ad69-9e8bc6451aa9"
        }
    }
    
On success as shown above, it's important to log user out and in again to refresh the authenticated session.