Title: Interaction Data
Description: Interaction Data Reference
---

The [Interaction Details API](interaciton-details.html) returns information related to a Regit interaction, including form structure
and user data.

# Structure of Interaction Fields

Data are represented in the structure of a **form**, which is a list of **fields**, each associated with a piece of data in the information vault. This list is defined by business user for every interaction
they create. The client application then displays the fields as UI controls that user can change before submitting on registration.

This is the common JSON format of a field, as provided by the `formData` block:

     {
         "path": "{field.path}",
         "title": "{field label}",
         "type": "{field type}",
         "options": {field options},
         "optional": {is field optional},
         "value": {user data}
     }

## Field Path
The path is a string that indicates the hierachical locaton of the piece of data in the information vault.
It is represented by a dot-separated list, listing the hierarchy top down.

    ".{top level name}.{level 1 name}.[{level 3 name}]"

The heading dot represents the top level or _path root_. A field can be 2 or 3 levels deep.

Level names are strings containing only alphabetical characters, no space, and in camel case.

Examples:

    ".basicInforoation.firstName"
    ".governmentID.passportID.cardNumber"

> The path serves as the key identifier for a field, indicating where to store submitted data. 
A form must not have more than one fields with the same path. 
    
## Field Label
The label or title of the field is a string used for UI display. It is normally the default preset, but can be changed by business user.

In the case of Custom Question, the label represents the question asked.

Examples:

    "First Name"
    "What is your favorite color?"
    
## Field Type

The type represents the format of the representation of data. 
Since data can be represented in several types (a single text value, a combo box to pick from, a date picker, etc.), this property
is essential for properly displaying and processing the field.

Examples:

    "textbox"
    "datecombo"
    
Details about every available type is listed below.

## Field Options

One certain field type may have several options to indicate the nature of the data and how to display it. 
For example, a `select` field has as options the list of items to display in the dropdown control.

The options are represented as a string array and usually empty, depending on the type.

Details about available options for every type is listed below.

## Optional Indicator

A field can be optional, whereas it is displayed on the form alongside required fields, but the user can choose whether to submit it or not.

The UI typically allows user to do this by flipping a switch. When the form is submitted, the optional fields that are opted out by user
must be left out from the data.

The `optional` property is a boolean with only two possible values: `true` or `false`. The default is `false` and may be omitted from
the data returned by the API. That is, it is only present when `true`.

## Field Value

The field value represent the actual data obtained from the information, or submitted by user.

In contrast to other properties, this is the only one that is dynamic. That is, it is not statically defined by business creator,
but contains user specific data.

The value can be one of several formats depending on the type. Details about the value format for every type is listed below.

If the value is null then user has no such data in the vault.

# Overview of Interaction Field Types

All available field types are listed below.

**Static Display** specifies how to display the original value, typically on first opening the form. If the value is null,
should leave it blank.

**Edit Control** specifies how to display UI control for user to edit the value, typically when user enters Edit mode. 

**Options** specifies how to interpret the options of the field. 

**Value Format** specifies how to interpret or formulate the data format of the value. _Important_: The API returns this value
in a special format alongside others, wrapped in a polymorphic object for compatibility with type-restricted JSON parsers.

     value: {
        text: "..." // if is string, null otherwise
        list: [...] // if is array, null otherwise
        json: "..." // if is object: JSON string, null otherwise
                    // If all is null then value is null
    }

In case of registration, the value must be submitted in normal format (that is either JSON string, array or object).

**Examples** cites one or more typical fields of the type.  

# Text

> type: "textbox"

The field accepts a single text value. This is by far the most common type.
 
## Static Display
Plain text

## Edit Control
A single line text field (HTML equivalent: `input [type=text]` element)

## Options
Normally there are no options.

One special case is a phone number, whereas options is `"phone"`.

## Value Format
String

A phone number is represented in international format: digits only, no space or symbol, except an optional `+` prefix.

## Examples
    
    path: ".basicInformation.firstName",
    type: "textbox",
    value: "John"    
    
    
    path: ".contact.mobile",
    type: "textbox",
    options: ["phone"],
    value: "6590909090"


# Checkbox

> type: "checkbox"

The field accepts a two-state boolean value.
 
## Static Display
Whatever makes sense, either _True/False_ or _Yes/No_.

## Edit Control
A checkbox button (HTML equivalent: `input [type=checkbox]` element)

## Options
None

## Value Format
Boolean (`true` or `false`)

## Examples

Currently no fields in vault use this type.  
    
    
# Radio

> type: "radio"

The field accepts a single text value picked from multiple options.
 
## Static Display
Plain text

## Edit Control
A radio button group (HTML equivalent: `input [type=radio]` element) or an option group (mutually exclusive buttons)

## Options
Array of choices as text values

## Value Format
String

## Example

    path: ".basicInformation.gender",
    type: "radio",
    options: [ "Male", "Female", "Other" ],
    value: "Male"    
   
   
# Select

> type: "select"

The field accepts a single text value picked from multiple options. This is similar in function to the `radio` but is useful
for long text or long list.
 
## Static Display
Plain text

## Edit Control
A dropdown control (HTML equivalent: `select` element)

## Options
Array of choices as text values

## Value Format
String

## Example

    path: ".governmentID.healthCard.bloodType",
    type: "select",
    options: [ "A", "B", "AB", "O" ],
    value: "A"
       
# Text List

> type: "tagsinput" | "smartinput"

The field accepts a list of one or more text values.

## Static Display
Comma-seperated list as text

## Edit Control
A tag input control that allows user to enter multiple short text

`Smartinput` should auto-suggest a list of commonly available values for user to choose from, during typing.

## Options
None for `tagsinput`. For `smartinput`, the type of value.

## Value Format
String array

## Examples

    path: ".basicInformation.ethnicity",
    type: "tagsinput",
    value: [ "American", "Chinese" ]
    
    
    path: ".others.preference.languages",
    type: "smartinput",
    options: ["language"],
    value: [ "English", "Chinese" ]
    
    
# Date

> type: "date" | "datecombo"

The field accepts a date value.

Currently all date fields in vault ignore the time of day (hours), and timezone information is unnecessary.

## Static Display
Date in proper format

## Options
Normally none. One special case is `indef`, meaning the date can be indefinite.

## Edit Control
A day - month - year group of select or text controls that allows user to enter date.

In case of `indef`, display a switch next to the date control that disables the control when turned on.

## Value Format
String representation of date in `YYYY-MM-DD` format

Submit an indefinite date as an empty string.

## Example

    path: ".governmentID.passportID.expiryDate",
    type: "date",
    options: ["indef"],
    value: "2018-12-24"        
        
# Address

> type: "address"

The field accepts a street address, including area within city.

## Static Display
Plain text

## Edit Control
A text field

## Options
None

## Value Format
String

## Example

    path: ".address.currentAddress.address",
    type: "address",
    value: "12B Ligua St., District 10"  
              
# Location

> type: "location"

The field accepts a pair of country and city.

Country is required while city is optional.

## Static Display
\{city\}, \{country\}

## Edit Control
A pair of controls to allow user to select or type in the city and country. At least for country, the UI should offer
a list of all countries for user to choose from, using a dropdown select or combo box.

## Options
None

## Value Format
An object with two properties, `country` and `city`. `City` can be null.

## Example

    path: ".basicInformation.location",
    type: "location",
    value: { 
        country: "India",
        city: "Mumbai"
    }
              
# Number

> type: "numinput"

The field accepts a numerical value, with possible unit of measure.

## Static Display
\{amount\} \{unit\}

## Options
If unit is required, this specifies one of common types: `length`, `weight`, `currency` and `shoesize`. 

## Edit Control
A pair of controls to allow user to type in the amount and unit.

If unit is required, the UI should offer a list of available options to choose, but still allow user to type in their own.

## Value Format
An object with two properties, `amount` and `unit`. `Unit` can be null.

## Example

    path: ".others.body.height",
    type: "numinput",
    options: ["length"],
    value: { 
        amount: "1.7",
        unit: "meter"
    }

# Document

> type: "doc"

The field accepts one or more files uploaded by user.

## Value Format
An array of document objects, each containing two fields: `filePath` and `fileName`. The file path is relative to server root,
like http://demo.regit.today/. Combine it with file name to get the absolute URL.

## Options
`img` if the file is an image 

## Static Display
If the user has any documents in the vault that match the field, the API returns the list.
 
Display only the first document in the list, as it is the default choice. Show the file name with link to the file. 
If it is an image, should display image instead of name.

## Edit Control
It is important to display the UI for this field type properly. There are two controls to display:

- A list of document where user can select any of them. Initially the first item is selected. Each item can be displayed
in the same way as the static display, next to a selector switch. In case the user does not have any document, this list is empty
and requires user to add one by uploading.

- A button that allows user upload multiple files from their device. The UI depends on specific platforms. For web app, the HTML
equivalent is `input [type=file]` element. Once uploaded, the file is added to the list, and already selected.

When submitting data, only send those selected items, ignore the rest.

## Example

    path: ".governmentID.passportID.photo",
    type: "doc",
    options: ["img"],
    value: [
              {
                "fileName": "a.jpg",
                "filePath": "/Content/vault/documents/f8b00a20-e88d-4a0f-a0f2-3c07f896b2b7"
              },
              ...
           ]

# Custom Question

> type: "qa"

This is a special field. The value is not obtained from the vault, but instead given by user as an answer to a question.

The answer can be a single text value that user enters freely, or a fixed text value that user chooses from a pre-defined list of
possible answers.

This field has a special path that always starts with `Custom.Question`, and grouped in User Information. (The `doc` is also
in that group.)

## Static Display
None. No value is obtained from vault.

## Options
If empty, then it accepts free text answer. If there is a list of string, then populate the answer list. 

## Edit Control
- For free text answer, a single text field
- For pre-defined answers, display it as a dropdown select. No default item should be selected (forcing user to select one).

## Value Format
String

## Example

    path: ".Custom.Question.1",
    title: "What is your favorite color?"
    type: "numinput",
    options: [
               "Red", 
               "Blue",
               ... 
           ]
           
# Static

> type: "static"

This is a very special field. Sometimes when business user creates a form, they want some static information to be merely displayed,
not to ask for value from user, so no interaction nor control is required, and no need to submit data. 

## Static Display
Plain text

## Options
The static text to display beside the label 

## Edit Control
None

## Value Format
No value is needed.

## Example

Currently only one field in vault uses this type.

    path: ".membership.businessName",
    type: "static",
    options: ["Regit"]
    
    
