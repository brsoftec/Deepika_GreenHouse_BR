Title: Invitation
Description: Invitation API
---

# POST `/Invite/Bulk`
> Bulk Invite

Send network invitations by email to multiple recipients

## Params
Provide the list of email addresses in JSON body

    {
        emails: [                   // List of email addresses
             "a@bc",
             "son@regit.today", 
             "real@person.com"
        ],
        type: "network",            // Type of invitation (optional) 
                                    // Default is "network", currently the only avaiable type
        message: "..."              // Optional message to include in invitation text
    }

## Response

    {
        "success": true,
        "message": "Some invitations sent",
        "data": {
            "sentEmails": [         // List of succesfully sent emails
                "real@person.com"
            ],
            "invalidEmails": [      // List of email addresses rejected as invalid
                "a@bc"
            ],
            "existingEmails": [     // List of email addresses found existing with a Regit user
                "son@regit.today"
            ],
            "unsentEmails": []      // List of failed invitations (email sending error)
        }
    }