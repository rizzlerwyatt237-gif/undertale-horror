using System;
using System.Linq;

EnsureDataLoaded();

// SAFE EXACT STRING REPLACER
// Changes the CONTENT of the existing UndertaleString object in-place.
// This keeps the original string resource/reference and does NOT touch
// the game's text drawing system, dialogue code, Step events, or movement.
// The old text is therefore replaced rather than creating a second string.

string oldText = ScriptInput("OLD TEXT", "Paste the exact original text to replace:");
if (oldText == null)
    return;

string newText = ScriptInput("NEW TEXT", "Enter the new horror text:");
if (newText == null)
    return;

var matches = Data.Strings.Where(s => s.Content == oldText).ToList();

if (matches.Count == 0)
{
    ScriptError("Exact old text was not found. Nothing was changed.\n\nOLD TEXT:\n" + oldText);
    return;
}

if (matches.Count > 1)
{
    if (!ScriptQuestion("Found " + matches.Count + " identical string resources.\n\nReplace all of them?\n\nYES = replace every exact match\nNO = cancel with no changes"))
    {
        ScriptMessage("Cancelled. Nothing was changed.");
        return;
    }
}

foreach (var str in matches)
{
    // Keep this SAME UndertaleString object/resource.
    // Its existing references remain attached to it.
    str.Content = newText;
}

ScriptMessage(
    "EXACT TEXT REPLACEMENT COMPLETE.\n\n" +
    "Old text was replaced in-place.\n" +
    "The old content no longer exists in those string resources.\n" +
    "No global text renderer or Step event was modified.\n\n" +
    "Changed: " + matches.Count + " string resource(s)."
);
