// UNDERTALE HORROR — ACTUAL DIALOGUE SYSTEM — ALL TEXT V2
// Targets the actual English text-data script instead of blindly editing Data.Strings.
// IMPORTANT: UndertaleInstruction string operands are stored through ValueString.Resource.
// The old reflection-only version could miss those resource wrappers.
// This version collects every string operand actually used by gml_Script_textdata_en.
// SAFE TEST: no GML is compiled; movement, rooms, sprites, and attacks are untouched.
// BACK UP data.win before running.

using System;
using System.Collections.Generic;
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
    ScriptError("Could not find gml_Script_textdata_en.");
    return;
}

var found = new HashSet<UndertaleString>();

// UMT stores a string Push operand as an UndertaleResourceById wrapper
// in instruction.ValueString, with the actual UndertaleString in .Resource.
foreach (UndertaleInstruction instruction in textData.Instructions)
{
    if (instruction == null)
        continue;

    if (instruction.Kind != UndertaleInstruction.Opcode.Push)
        continue;

    if (instruction.Type1 != UndertaleInstruction.DataType.String)
        continue;

    if (instruction.ValueString == null)
        continue;

    UndertaleString str = instruction.ValueString.Resource;
    if (str != null)
        found.Add(str);
}

int targeted = 0;
int changed = 0;

foreach (UndertaleString str in found)
{
    if (str == null || string.IsNullOrWhiteSpace(str.Content))
        continue;

    string original = str.Content;

    // Since these strings came directly from the English text-data script,
    // do not apply the old heuristic that rejected many valid dialogue entries.
    // We only skip obviously non-text bookkeeping strings.
    if (original == "[HORROR]" || original.Contains("[HORROR]"))
        continue;

    targeted++;

    // Put the marker after leading Undertale backslash control codes.
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
    "===== ACTUAL UNDERTALE DIALOGUE SYSTEM — ALL TEXT V2 =====\n\n" +
    "Target: gml_Script_textdata_en\n" +
    "String operands found: " + found.Count + "\n" +
    "Text-data entries targeted: " + targeted + "\n" +
    "Text-data entries changed: " + changed + "\n\n" +
    "String operand source: ValueString.Resource\n" +
    "GML compiler: NOT USED\n" +
    "Player movement: NOT MODIFIED\n" +
    "Rooms: NOT MODIFIED\n" +
    "Sprites: NOT MODIFIED\n" +
    "Attacks: NOT MODIFIED\n\n" +
    "If [HORROR] appears in Undertale dialogue, this confirms the actual\n" +
    "English text-data system is being modified.\n" +
    "===================================================="
);