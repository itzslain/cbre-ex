# CBRE-EX Custom Entity Guide

This is a guide on how to write your own custom entities! Buckle up and enjoy the ride.

## Structure
* `Name`: The name of the entity.
* `Description`: The description of the entity.
* `Sprite`: The path to the entity's sprite. Relative to CBRE-EX's root folder. Must not
contain a file extension (.png, etc).
* `UseModelRendering`: Tells CBRE-EX to render this entity with the model defined in the
`file` property. If the model is an invalid file, CBRE-EX will fall back to the `Sprite`.
* `Properties`: An array of properties. Their structure is as follows:
   * `Name`: The name of the property.
   * `Type`: The type of the entity. Read [this](#property-types) for more information about
possible types.
   * `SmartEditName`: The name of the property displayed when Smart Edit is turned on.
   * `DefaultValue`: The default value of the property.
   * `HelpText`: The text that will be displayed in the help box when you select the property.

## Property Types
* `Bool`: **True** or **False** property.
* `Color255`: Property that contains an RGB color, with each component ranging between 0-255.
* `Float`: Property that contains a decimal number.
* `Integer`: Similar to **Float**, but it can only contain a number with no decimals in it.
* `String`: Property that contains a sequence of characters, text if you will.
* `Vector`: Property that contains a vector with 3 components.

# Example Entity
```json
{
  "Name": "template",
  "Description": "Template item description.",
  "Sprite": "sprites/mysprite",
  "UseModelRendering": true,
  "Properties": [
    {
      "Name": "file",
      "Type": "String",
      "SmartEditName": "File",
      "DefaultValue": "",
      "HelpText": "The model this entity will display with in the editor!"
    },
    {
      "Name": "scale",
      "Type": "Vector",
      "SmartEditName": "Scale",
      "DefaultValue": "1 1 1",
      "HelpText": "The scale of the model!"
    },
    {
      "Name": "example",
      "Type": "String",
      "SmartEditName": "Example Property",
      "DefaultValue": "This is an example.",
      "HelpText": "This is a test of the help box!\r\nTesting newlines!"
    },
    {
      "Name": "exampletwo",
      "Type": "Color255",
      "SmartEditName": "Example Property Two",
      "DefaultValue": "255 255 255",
      "HelpText": "This is ANOTHER test of the help box! This one uses a Color instead of a string."
    }
  ]
}
```
