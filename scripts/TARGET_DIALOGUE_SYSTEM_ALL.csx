// UNDERTALE HORROR — ACTUAL DIALOGUE SYSTEM — ALL TEXT
// Targets the English text-data script instead of blindly editing Data.Strings.
// It discovers UndertaleString references used by gml_Script_textdata_en.
// Every dialogue-looking string referenced there gets a visible [HORROR] marker.
// This is intentionally a SAFE TEST: no GML is compiled, no movement/rooms/sprites are touched.
// BACK UP data.win before running.

using System;
using System.Collections.Generic;
using System.Reflection;
using UndertaleModLib.Models;

EnsureDataLoaded();

UndertaleCode textData = null;
foreach (UndertaleCode code in Data.Code)
{
    if (code?.Name?.Content == "gml_Script_textdata_en")
    {
        textData = code;
        break;
    }
}

if (textData == null)
{
    ScriptError("Could not find gml_Script_textdata_en. This script expects the normal English Undertale text-data system.");
    return;
}

var found = new HashSet<UndertaleString>();

// Walk instruction properties and collect actual UndertaleString operands.
// Reflection makes this tolerant of UMT versions whose instruction operand
// property names differ.
foreach (var instruction in textData.Instructions)
{
    Type t = instruction.GetType();

    foreach (PropertyInfo p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public))
    {
        if (!p.CanRead)
            continue;

        object value = null;
        try { value = p.GetValue(instruction); } catch { }

        if (value is UndertaleString s)
            found.Add(s);
    }

    foreach (FieldInfo f in t.GetFields(BindingFlags.Instance | BindingFlags.Public))
    {
        object value = null;
        try { value = f.GetValue(instruction); } catch { }

        if (value is UndertaleString s)
            found.Add(s);
    }
}

int targeted = 0;
int changed = 0;

foreach (UndertaleString str in found)
{
    if (str == null || string.IsNullOrWhiteSpace(str.Content))
        continue;

    string original = str.Content;

    // Actual Undertale dialogue normally contains a speaker/text marker,
    // control codes, or the dialogue terminator. Avoid changing IDs,
    // function names, and other internal text-data values.
    bool looksLikeDialogue =
        original.Contains("/%%") ||
        original.Contains("/ %") ||
        original.StartsWith("*") ||
        original.StartsWith("\\W") ||
        original.StartsWith("\\E") ||
        original.StartsWith("\\F") ||
        original.Contains("* ") ||
        original.Contains("/\n") ||
        original.Contains("&") ||
        original.Contains("^");

    if (!looksLikeDialogue)
        continue;

    targeted++;

    // Do not stack the marker if the script is run twice.
    if (original.Contains("[HORROR]"))
        continue;

    // Preserve leading Undertale control codes and insert the marker after them.
    int insertAt = 0;
    while (insertAt + 1 < original.Length && original[insertAt] == '\\')
    {
        insertAt += 2;
        while (insertAt < original.Length && char.IsDigit(original[insertAt]))
            insertAt++;
    }

    str.Content = original.Insert(insertAt, "[HORROR] ");
    changed++;
}

ScriptMessage(
    "===== ACTUAL UNDERTALE DIALOGUE SYSTEM — ALL TEXT =====\n\n" +
    "Target: gml_Script_textdata_en\n" +
    "Actual dialogue strings found: " + found.Count + "\n" +
    "Dialogue entries targeted: " + targeted + "\n" +
    "Dialogue entries changed: " + changed + "\n\n" +
    "GML compiler: NOT USED\n" +
    "Player movement: NOT MODIFIED\n" +
    "Rooms: NOT MODIFIED\n" +
    "Sprites: NOT MODIFIED\n" +
    "Attacks: NOT MODIFIED\n\n" +
    "If the game shows [HORROR] in dialogue, we have successfully reached\n" +
    "Undertale's real English dialogue system.\n" +
    "===================================================="
);