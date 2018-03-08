Title: Sign Up
Description: Sign Up (Registration) API
---
# POST `/Account/Register`
> [Public]    
> Register new individual account

## Params
Provide user submitted information for registration, JSON encoded in body

    {
        Account: {
            FirstName: "Test",
            LastName: "User",
            Email: "test@regit.today",
            PhoneNumber: "90909090",
            PhoneNumberCountryCallingCode: "65",
            Country: "Singapore",
            City: "Singapore",
            // Avatar: "/Content/ProfilePictures/....jpg", => this is obsolete
            Avatar: "a4v77d..."     // Base64 encoded raw data of image
            Password: "Test@123"
        },
        Authentication: {
            RequestId: '58a9c6b50ed84bb6986d84e1fa226585',  // OTP Request ID obtained by previous OTP authentication 
            PIN: "1234",    // User entered PIN
        },
        SecurityQuestionsAnswers: [                 // Optional: security questions and answers
            { ... },
            { ... },
            { ... }
        ]
     }

Optionally provide the list of security questions and user's answers, in the same format as required by the 
[Security Questions Update API](account-settings.html#post-accountaccessprofilesecurityquestions).

## Response
       {
         success: true,
         message: "User signed up successfully",
         data: {
            accountId: "9d8aaf98-dedb-4756-8675-b2402fa64226"    // Id of the created account
            warning: "Security answers not updated: {errorMessage}"     // If error setting security questions
         }
       }

## Notes
Before calling this, make sure to complete these steps:
* Check if account already exists by calling [Check Account Exists API](check-account-exists.html)
* OTP authenticate using the [Verify Phone Number API](verify-phone-number.html)

Sometimes Signup OTP check is disabled on development site (demo.regit.today). In that case the authentication information is ignored.