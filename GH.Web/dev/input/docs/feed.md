Title: Feed
Description: User Feed API
---
# GET `/Interaction/Newsfeed`

> Retrieve individual feed for current user

This is by current definition a list of latest active interactions targeted for the user, requiring any such interaction:
* is of feed-distributed type (either Broadcast, Registration or Event)
* comes from a followed business
* has publishing criteria matching the user's profile

## Params
Provide starting page offset (1-based) URL encoded

    ?page=1

This parameter is optional, if absent default to 1.

## Response

JSON encoded list of targeted interactions, sorted by descending date (latest first).

The size of this list is always limited to 20. Use the `page `parameter to navigate through the full list.

    {   
        "success": true,
        "message": "List 3 items",
        "data": 
        [
            {
            "id": "59ecd2d76e1b1b378ce44644",
            "status": "Active",
            "type": "registration",
            "name": "New Registration for Jan 2018 Class",
            "description": "",
            "business": {
                "id": "58c6622b6e1b435d50a853a7",
                "accountId": "25d6bc5d-8931-43f0-8ba2-87c2ed798eab",
                "name": "East Asia School",
                "avatar": "/Content/ProfilePictures/58c6622b6e1b435d50a853a7_profile_pic_636250322947178559.jpg",
                "following": true
            },
            "indefinite": true,
            "termsUrl": "www.regit.today",
            "from": "2017-10-23T01:15:20.211+08:00",
            "until": "2017-10-23T01:19:31.282+08:00",
            "image": "/Content/UploadImages/25d6bc5d-8931-43f0-8ba2-87c2ed798eabCampaign20171023011612.jpg",
            "targetUrl": "www.regit.today",
            "verb": "register",
            "socialShare": "all",
            "paid": false,          // If interaction is paid? then following price and currency apply.
            "price": "0",
            "priceCurrency": "",
            "eventInfo": null,      // Only present if interaction is event type, see the next item
            "participated": true,   // Only present if interaction is participated (current user has registered)
            "participants": 2,
            "expired": false,
            "participations": null
        },
        {
            "id": "59d451f56e1b1b364095e252",
            "status": "Active",
            "type": "event",
            "name": "Charity Golf Tournament 2017",
            "description": "Singapore Chairty’s Charity Golf Tournament is an annual event where we combine the love of golf with giving back to the community. Participants will enjoy a whole day of golfing in the name of charity, and all proceeds from the tournament will go towards funding our six core programmes.\n\nCome par-tee with us at the Charity Golf Tournament!\n\nTAKE PART:\n- Per Player inclusive of dinner S$500\n- Only dinner S$100",
            "business": {
                "id": "58c694156e1b455d503d3c5a",
                "accountId": "0926c61d-d46c-4f26-b0ff-1d2cb0d80d92",
                "name": "Singapore Charity",
                "avatar": "/Content/ProfilePictures/58c694156e1b455d503d3c5a_profile_pic_636320118266791355.jpg",
                "following": true
            },
            "indefinite": true,
            "termsUrl": "www.regit.today",
            "from": "2017-10-04T11:01:28.318+08:00",
            "until": "2017-10-04T11:14:07.302+08:00",
            "image": "/Content/UploadImages/0926c61d-d46c-4f26-b0ff-1d2cb0d80d92Campaign20171004111222.jpg",
            "targetUrl": "",
            "verb": "participate",
            "socialShare": "all",
            "paid": true,
            "price": "500.00",
            "priceCurrency": "SGD",
            "eventInfo": {
                "fromDate": "2017-10-13T16:00:00.000Z",
                "fromTime": "1970-01-01T02:00:00.000Z",
                "toDate": "2017-10-13T16:00:00.000Z",
                "toTime": "1970-01-01T10:00:00.000Z",
                "location": "Laguna National Golf & Country Club",
                "theme": null
            },
            "participants": 2,
            "expired": false,
            "participations": null
        },
        {
            "id": "5991131b6e1b1b1e94710f8d",
            "status": "Active",
            "type": "broadcast",
            "name": "RELOCATING THANH NAM SUB-BRANCH",
            "description": "Asia Commercial Bank is pleased to announce Thanh Nam Sub-branch shall be relocated as follows:\n\nPrevious address: No. 83, Thanh Chung Street, Cua Bac Ward, Nam Dinh City, Nam Dinh Province\nNew address: No. 38-40, Thanh Chung Street, Ba Trieu Ward, Nam Dinh City, Nam Dinh Province",
            "business": {
                "id": "58c665446e1b435d50a853af",
                "accountId": "1c5b4703-9635-4cbf-910f-13f8c4b08668",
                "name": "ACB Bank",
                "avatar": "/Content/ProfilePictures/58c665446e1b435d50a853af_profile_pic_636312147818402454.png",
                "following": true
            },
            "indefinite": true,
            "termsUrl": null,
            "from": "2017-08-14T10:58:57.586+08:00",
            "until": "2017-08-16T15:10:49.285+08:00",
            "image": "/Content/UploadImages/1c5b4703-9635-4cbf-910f-13f8c4b08668Campaign20170814110304.jpg",
            "targetUrl": "http://acb.com.vn/en/about-en/media-news/latest-announcements/relocating-thanh-nam-sub-branch",
            "verb": "register",
            "socialShare": "all",
            "paid": false,
            "price": null,
            "priceCurrency": null,
            "eventInfo": null,
            "participants": 0,
            "expired": false,
            "participations": null
          },
        ]
    }
    
## Notes
For convenience, each interaction entry includes profile information of the owning business.

# GET `/Interaction/Feed`

> Retrieve interaction feed from a business

This is similar to individual feed, but lists interactions from only one business. All active interactions are included, \
not just those targeted for user.

This business feed is typically shown on business profile page.

## Params
Provide the account ID of the business, URL encoded:

    ?businessAccountId=19ab8a86-6b50-40b2-934c-15fbb42ea4aa
    
Other paging parameters same as individual feed

## Response
Same as individual feed, but limited to only the desired business