Title: Push Interaction
Description: Push Interaction Reference
---
# Overview on Push Interactions

Sometimes business pushes an interaction to a user to invite them to join. The user then sees the invitation coming in their notification list as a special type.

> There are 2 such types: **Handshake** (handshake only) and **SRFI** (all the remaining types of interaction, including SRFI, registration and event)

Format of SRFI notification:

        {
            "Id": "5a7be40105100b3b2076912e",
            "Category": "srfi",
            "Type": "Invited SRFI",
            "FromAccountId": "19ab8a86-6b50-40b2-934c-15fbb42ea4aa",
            "FromUserDisplayName": "Qudy",
            "ToAccountId": "f8b00a20-e88d-4a0f-a0f2-3c07f896b2b7",
            "ToUserDisplayName": "Long Tra",
            "Title": "Qudy has requested you to submit your personal information",
            "Content": "srfi",
            "PreserveBag": "5a4c4b7305100b20109c5c14",
        }
        
Format of handshake notification:
        
       {
           "Id": "5a7be6b705100b3b20769134",
           "Category": "handshake",
           "Type": "Invited Handshake",
           "FromAccountId": "19ab8a86-6b50-40b2-934c-15fbb42ea4aa",
           "FromUserDisplayName": "Qudy",
           "ToAccountId": "f8b00a20-e88d-4a0f-a0f2-3c07f896b2b7",
           "ToUserDisplayName": "Long Tra",
           "Title": "Qudy has invited you to join a handshake.",
           "Content": "handshake",
           "PreserveBag": "5a41ff6f05100b0da0ebc6b4",
       }
       
Pay attention to two keys:

> **Content** is the interaction type (this is optional and may not appear in old notification but it's not important)
> **PreserveBag** is the interaction ID.

Once you receive such notification, the difference ends. You should treat both cases as the same, and implement the same flow.
 
# How to Handle Push Interactions

- Use the interaction ID to query details about the interaction

       /Interactions/Details/5a4c4b7305100b20109c5c14
    
- You now have all the data you need, so open the interaction form, the same way you do when you **open form from the user feed**. No difference whatsoever.

- Handle form, allow user editing exactly the same way. Then register using the same Register API.


    
    