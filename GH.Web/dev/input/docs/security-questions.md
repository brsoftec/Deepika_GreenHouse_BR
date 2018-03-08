Title: Security Questions List
Description: List security questions
Category: Account Helpers
---
# GET `/Account/SecurityQuestions`
> [Public]    
> List security questions

Retrieve full list of pre-defined security questions and codes.

Use this list to populate UI controls allowing user to select security questions.

## Params
None

## Response
    [
        {
            "Id": "5818a3e9e66cc104cc5fb826",               // Question ID (internal use only)
            "Code": "Q1",                                   // Code to indentify question
            "Question": "What was your childhood nickname?" // Question text
        },
        {
            "Id": "5818a3e9e66cc104cc5fb827",
            "Code": "Q2",
            "Question": "When you were young, what did you want to be when you grew up?"
        },
        ...             // 16 more questions
        
    ]