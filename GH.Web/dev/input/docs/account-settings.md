Title: Account Settings
Description: Account Settings API
---
# GET `/Account/AccessProfile`

> Get current user's access profile

The Access Profile contains account-specific information necessary for verifying and granting access to an authenticated Regit user, including:

- **Primary email address**: serves as _username_ (user identifier), which together with **password** form the credentials to authenticate user (by logging in).
- **Primary phone number**: serves as OTP delivery point (SMS messages used for various 2FA operations are sent to this number.)

## Params
None

## Response

    {
        "success": true,
        "message": "User access profile",
        "data": {
            "accountId": "f8b00a20-e88d-4a0f-a0f2-3c07f896b2b7",
            "email": "user@regit.today",
            "phoneNumber": "+6590909090"
        }
    }
    
## Notes
Though this API call is read-only, user is allowed to change their access profile by calling any of the three next update operations, each acting on one factor. 
Important notes and caution must be taken (applying to all three):

- As extra security measure, all calls are required to provide current password.
- The client should have confirmed the user's ownership of the factor, by a preceding 2FA verification process.
- On successful update, the client should log user out and let them log in again, to refresh the session and re-validate the new credentials.


# POST `/Account/AccessProfile/Email`
> Change current user's primary email address

## Params

Provide the new email address, accompanied by current password, as JSON body

    { 
        email: "user2@regit.today",
        password: "Test@123"
    }

## Response

    {
        "success": true,
        "message": "Account email address changed successfully",
        "data": {
            "accountId": "f8b00a20-e88d-4a0f-a0f2-3c07f896b2b7",
            "email": "user2@regit.today"
        }
    }

# POST `/Account/AccessProfile/PhoneNumber`
> Change current user's primary phone number

## Params

Provide the new phone number, accompanied by current password, as JSON body

    { 
        phoneNumber: "84909090593",
        password: "Test@123"
    }

The client should have already verified the new phone number by a preceding call to [Verify Phone Number API](verify-phone-number.html).

## Response

    {
        "success": true,
        "message": "Account phone number changed successfully",
        "data": {
            "accountId": "f8b00a20-e88d-4a0f-a0f2-3c07f896b2b7",
            "phoneNumber": "+84909090593"
        }
    }

# POST `/Account/AccessProfile/Password`
> Change current user's password

## Params

Provide the current password and new password, as JSON body

    { 
        password: "Test@123",
        newPassword: "Test@234"
    }

## Response

    {
        "success": true,
        "message": "Account password changed successfully",
        "data": {
            "accountId": "f8b00a20-e88d-4a0f-a0f2-3c07f896b2b7"
        }
    }

# GET `/Account/AccessProfile/SecurityQuestions`
> Get current user's security questions and answers

Retrieve the list of three security questions user has selected, and their answers.

## Params
None

## Response

    {
        "success": true,
        "message": "List user security questions and answers",
        "data": [
            {               // Security question 1
                "code": "Q1",       // Question identifying code
                "question": "What was your childhood nickname?",
                "answer": "Jack"    // User's answer
            },
            {               // Security question 2
                "code": "Q3",  
                "question": "Who was your childhood hero?",
                "answer": "Batman"
            },
            {               // Security question 3
                "code": "Q5",        
                "question": "What street did you live on in third grade?",
                "answer": "Broadway"
            }
        ]
    }

# POST `/Account/AccessProfile/SecurityQuestions`
> Change current user's security questions and answers

Update the new list of security questions and answers selected by user.

## Params
Provide the new list of security questions and answers, as JSON body. Use the same format as the GET response, except that only the code is necessary to identify the question,
so the question text is not required and should be omitted. (The full list of pre-defined security questions with corresponding codes can be obtained from the
[Security Questions List API](security-questions.html).)
   
     {   
        "securityQuestions": [
           {                        
               "code": "Q5",
               // "question": "What is the name of your favorite childhood friend?",    // unnecessary, will be ignored
               "answer": "Tom"    // User's answer
           },
            ...     // Other two
       ]
     }
     
All three security questions must be updated at once with this call. That is, the list must include all 3, and no partial
data is acceptable.

## Response

    {
        "success": true,
        "message": "User security questions and answers updated successfully",
        "data": {
            "updatedCount": 3
        }
    }

# POST `/Account/AccessProfile/Close`
> Close account

The account will be locked from further logging in, during a grace period before it is terminated completely. The default period is one month.

## Params

Provide the account password, as JSON body

    {
        password: "Test@123"
    }

## Response

    {
        "success": true,
        "message": "Account closed",
        "data": {
            "accountId": "f8b00a20-e88d-4a0f-a0f2-3c07f896b2b7",
            "status": "closed",
            "willTerminateAt": "2018-03-02T08:00:35.668359Z"        // Time for termination
        }
    }

## Notes
After successful operation, the client should log user out immediately. During the grace period, no further access to the account will be allowed,
and no signup is possible under that username.

# POST `/Account/AccessProfile/Reopen`
> [Public]
> Reopen closed account

The result is temporary; it only re-enables access to the closed account (user can log in and do things as usual),
but the account will still be terminated as scheduled by the last closing operation. The only way for user to
cancel termination is by asking Regit support.

This operation is mainly useful for development testing.

## Params

Provide the email address and account password, as JSON body

    {
        email: "son@regit.today",
        password: "Test@123"
    }

## Response

    {
        "success": true,
        "message": "Account temporarily reopened",
        "data": {
            "accountId": "f8b00a20-e88d-4a0f-a0f2-3c07f896b2b7",
            "status": "reopened"
        }
    }
