Title: A. API Overview
Description: General information and instructions on using Regit API for mobile application
----
## Conventions
All context dependent variables/placeholders listed in this documentation, are enclosed in braces.

Example: `{id}` => substitute `id` with actual value

In this documentation, API parameters and response are listed using example JSON encoded values, annotated by comment where necessary.
## Request
All API endpoints listed in this documentation are relative path to host root. The host used for mobile development is _demo.regit.today_.

An endpoint URL usually contains four parts:
* API prefix, always `/Api/...`
* Module identifier, eg. `Network/...`
* Action: the function to call
* Parameters

Parameters are transmitted as name-value pairs, and depending on context (endpoint specific), can be either:
* _Route based_: appended to URL, eg: `../{action}/{id}`
* _Query-based_: URL encoded &-separated list in the query part of the URL, eg: `.../{action}?{name}={value}&...`
* _Form values_: Encoded as form values in HTTP body (`Content-Type: application/x-www-form-urlencoded`). This method only applies to POST requests.
* _Body embedded_: Encoded as JSON in HTTP body (`Content-Type: application/json`). This method only applies to POST requests.

Every endpoint listing is by convention:
* Prefixed by the HTTP method to use for the request (mostly GET or POST for this API). An omitted method implies GET.
* Omits the `/Api` prefix for brevity, unless noted otherwise

Example: For an endpoint listed as `POST /Network/Invite` => implement by sending a POST request to `http://demo.regit.today/Api/Network/Invite`

## Response
* Successful results returned by the API are JSON encoded in HTTP body.
* Error is returned by standard HTTP status code, and/or when possible, JSON encoded in body (endpoint specific).
* Consult documentation of each endpoint for specific result/error format.

## Authentication
All API calls, unless marked as _[Public]_, must be authenticated by standard method of **Bearer Token**. 

**How to authenticate**: put the access token in an entry of HTTP header in this format:

     Authorization: Bearer {access_token}

Where `{access_token}` is the token acquired by a previous call to Login API.

The access token is short-lived, only valid for one session, which expires by default in 24 hours.

All authenticated API calls are executed in context of the session, and on behalf of the **current user**, who is owner of the account that obtained the token on logging in.

Query status of any given token, including expiry and owner ID, by using the [Token Query API](tokens.html).

## Implementation Notes
* During a session, store the token for re-use on every call, possibly using an HTTP interceptor.
* Postman tip: the authentication can be automatically simulated for every call by setting Authorization type to Bearer Token, and fill in the access token obtained from this API.
