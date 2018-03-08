Title: Interaction Details
Description: Interaction Details API
---
# GET `/Interaction/Details/{interactionId }`

> Get details of an interaction

The details include:
- General information about the interaction and owning business, similar to newsfeed items
- Participation details (whether current user has registered)
- Form data for user to review and register

## Params:
Provide ID of the interaction to query, as can be found in the newfeed

## Response
     {
         "success": true,
         "message": "Interaction details",
         "data": {
            "id": "59d451f56e1b1b364095e252",
            "type": "event",
            "name": "Charity Golf Tournament 2017",
            "description": "Singapore Chairty’s Charity Golf Tournament is an annual event where we combine the love of golf with giving back to the community. Participants will enjoy a whole day of golfing in the name of charity, and all proceeds from the tournament will go towards funding our six core programmes.\n\nCome par-tee with us at the Charity Golf Tournament!\n\nTAKE PART:\n- Per Player inclusive of dinner S$500\n- Only dinner S$100",
            "business": {          // Basic profile of owning business
                "id": "58c694156e1b455d503d3c5a",
                "accountId": "0926c61d-d46c-4f26-b0ff-1d2cb0d80d92",
                "name": "Singapore Charity",
                "avatar": "/Content/ProfilePictures/58c694156e1b455d503d3c5a_profile_pic_636320118266791355.jpg",
                "following": true
             },
            "image": "/Content/UploadImages/0926c61d-d46c-4f26-b0ff-1d2cb0d80d92Campaign20171004111222.jpg",
            "targetUrl": "",                // Link to open when clicking interaction
            "termsType": "url", 
            "termsUrl": "www.regit.today",  // URL for terms and conditions
            "paid": true,
            "price": "500.00",
            "priceCurrency": "SGD",
            "verb": "participate",
            "socialShare": "all",
            "until": null,                 // Expiry date. Null = indefinite 
            "eventInfo": {                 // Event information, only applicable if type is event
                "fromDate": "2017-10-13",  // Event schedule
                "fromTime": "02:00",
                "toDate": "2017-10-13",
                "toTime": "10:00",
                "location": "Laguna National Golf & Country Club",
                "theme": null
             },
             "participated": true,      // Only present if interaction is participated (current user has registered)
             "participations": [        // Participation details, only if participated
                 {                      // Thanks to delegation, user can register with different roles each time
                     "actor": "self",   // self = registered yourself 
                                        // for = registered for someone else (delegator)
                                        // by = registered by someone on your behalf (delegatee)
                     "actorName": "",
                     "delegationId": null,
                     "participated": "2017-10-27T17:30:14.4509799+08:00"    // Time of registration
                 },                 
                 {
                     "actor": "for",
                     "actorName": "John D.",
                     "delegationId": "5961e6f305100b09a0522425",
                     "participated": "2017-10-30T17:30:14.4509799+08:00"
                 }
             ],
             "form": {                  // Form structure and data, see notes
                "fields": [
                    {
                        "path": ".basicInformation.title",
                        "displayName": "Title",
                        "type": "textbox",
                        "options": "",
                        "optional": false,
                        "group": "basicInformation"
                    },
                    {
                        "path": ".basicInformation.firstName",
                        "displayName": "First Name",
                        "type": "textbox",
                        "options": "",
                        "optional": false,
                        "group": "basicInformation"
                    },
                    {
                        "path": ".basicInformation.lastName",
                        "displayName": "Last Name",
                        "type": "textbox",
                        "options": "",
                        "optional": false,
                        "group": "basicInformation"
                    },
                    {
                        "path": ".basicInformation.dob",
                        "displayName": "Date of Birth",
                        "type": "datecombo",
                        "options": "",
                        "optional": false,
                        "group": "basicInformation"
                    },
                    {
                        "path": ".basicInformation.gender",
                        "displayName": "Gender",
                        "type": "radio",
                        "options": [
                            "Male",
                            "Female",
                            "Other"
                        ],
                        "optional": false,
                        "group": "basicInformation"
                    },
                    {
                        "path": ".contact.mobile",
                        "displayName": "Mobile Phone",
                        "type": "textbox",
                        "options": "phone",
                        "optional": false,
                        "group": "contact"
                    },
                    {
                        "path": ".contact.email",
                        "displayName": "Personal Email",
                        "type": "textbox",
                        "options": "",
                        "optional": false,
                        "group": "contact"
                    },
                    {
                        "path": ".address.currentAddress.address",
                        "displayName": "Address",
                        "type": "address",
                        "options": "",
                        "optional": false,
                        "group": "currentAddress"
                    },
                    {
                        "path": ".governmentID.nationalID.nationalID",
                        "displayName": "NRIC / National ID",
                        "type": "textbox",
                        "options": "",
                        "optional": false,
                        "group": "nationalID"
                    }
                ],
                "groups": [
                    {
                        "name": "basicInformation",
                        "displayName": "Basic Information"
                    },
                    {
                        "name": "contact",
                        "displayName": "Contact"
                    },
                    {
                        "name": "currentAddress",
                        "displayName": "Current Address"
                    },
                    {
                        "name": "nationalID",
                        "displayName": "National ID"
                    }
                ],
                "userData": [
                  {
                      "source": "vaultCurrentValue",
                      "path": ".basicInformation.title",
                      "value": "Sir"
                  },
                  {
                      "source": "vaultCurrentValue",
                      "path": ".basicInformation.firstName",
                      "value": "son son"
                  },
                  {
                      "source": "vaultCurrentValue",
                      "path": ".basicInformation.lastName",
                      "value": "beo"
                  },
                  {
                      "source": "vaultCurrentValue",
                      "path": ".basicInformation.dob",
                      "value": "1966-12-26"
                  },
                  {
                      "source": "vaultCurrentValue",
                      "path": ".basicInformation.gender",
                      "value": "Female"
                  },
                  {
                      "source": "vaultCurrentValue",
                      "path": ".contact.mobile",
                      "value": "+6590909090"
                  },
                  {
                      "source": "vaultCurrentValue",
                      "path": ".contact.email",
                      "value": "son@regit.today"
                  },
                  {
                      "source": "vaultCurrentValue",
                      "path": ".address.currentAddress.address",
                      "value": null
                  },
                  {
                      "source": "vaultCurrentValue",
                      "path": ".governmentID.nationalID.nationalID",
                      "value": null
                  }
                ]
             },
             "formData": [      // Form structure and data, packaged format
                 {              // Each item in list is a group of fields
                     "heading": "Basic Information",
                     "fields": [
                         {
                             "path": ".basicInformation.title",
                             "title": "Title",
                             "type": "textbox",
                             "options": [],
                             "optional": true,  // Present only if field is optional
                             "value": {         // Value in polymorphic form
                                 text: "..."    // if is string, null otherwise
                                 list: [...]    // if is array, null otherwise
                                 json: "..."    // if is object: JSON string, null otherwise
                                                // If all is null then value is null
                             }
                         },
                        ...
                     ]
                 },
                 ...
             }
         }
     }

## Notes
Form data includes:
- Form structure as defined by business (list of fields with options and grouping)
- Current user data obtained from information vault 