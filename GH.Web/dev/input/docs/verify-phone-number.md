Title: Verify Phone Number
Description: Verify phone number via OTP
Category: Account Helpers
---
# POST `/Account/Otp/VerifyPhoneNumber`
> Verify phone number by sending an OTP PIN

## Params
Provide the phone number to verify, URL encoded

    ?phoneNumber=6590909090

The phone number must be in international format, including country code, in plain digits, without any space or symbol, as in example.

The OTP Verification module will send a random 4-digit PIN to the provided phone number, via several attempts, by SMS and/or voice call. The process will continue
for a validity period of **5 minutes**, during which the request is considered **pending** and will forbid new OTP requests to be initiated for the same phone number.

The running request will automatically expire after such period, rendering new requests available. Until then, if you want to create a new request wihout having to wait
(typically when user asks to resend PIN), it is necessary to cancel the pending request first before attempting a new one. Both functions can be done with one call to this API, by appending another parameter to the URL:

    &cancelRequest={requestId}
    
Where `{requestId}` is the ID of the outstanding OTP request, recorded from the previous initiation call. This request will be cancelled first, before proceeding with the OTP verification (sending a new PIN).

Even when a correct ID is provided, this operation does not always succeed. An OTP request cannot be cancelled when:

1. It is new (less than 30 seconds after creation).
2. Too many attempts have been made to deliver the PIN (typically when a voice call has been made).

It is important to check the response for this status, possibly waiting and retrying before proceeding accordingly.

## Response

Successful result includes an **OTP Request ID**

    {
        "success": true,
        "message": "PIN sent to phone number",
        "data": {
            "phoneNumber": "+6590909090",
            "requestId": "bd768a52df1840a6bfe53c054fadadfd"
        }
    }

Store the ID for use with the user-entered PIN in subsequent call to [Signup API](signup.html), or possibly to cancel this request.

Unsuccessful result may report one of the following errors: 

If the phone number is found to be invalid:

    {
        "success": false,
        "message": "Invalid phone number"
        "error": "otp_invalid_number"
    }

If there is an outstanding OTP request associated with this phone number:

    {
        "success": false,
        "message": "Pending request for phone number +6590909090",
        "error": "otp_pending_request"
    }

 
## Response with Request Cancellation

If the given request is ongoing but uncancellable, the call returns the error immediately, without proceeding.

    {
        "success": false,
        "message": "Request cannot be cancelled",
        "error": "otp_uncancellable_request"
    }

Otherwise even if it fails, the OTP verification still proceeds, and the cancellation status is included in the final success response.

    "data": {
        ...
        "requestCancellation": {
            "success": true,
            "message": "Request cancelled successfully: bbb1d144b0dd4a9991a406231c398adc"
        }
        // or on failure
        "requestCancellation": {
            "success": false,
            "message": "Error cancelling request"
        }
     ]

## Notes
Sometimes OTP is disabled on development site (demo.regit.today). In that case this call always returns request ID of `123456`. Use it with any 4 digit PIN.

