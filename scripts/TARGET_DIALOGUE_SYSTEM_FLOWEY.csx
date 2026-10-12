// UNDERTALE HORROR — TARGET ACTUAL DIALOGUE SYSTEM
// Targets gml_Script_textdata_en instead of scanning/replacing arbitrary strings.
// It does NOT compile or replace GML.
// It only edits UndertaleString resources that are actually referenced by
// the English text-data script, preserving the surrounding control codes.
// BACK UP data.win before running.

using System;
using System.Collections.Generic;
using UndertaleModLib.Models;

EnsureDataLoaded();

var textData = Data.Code.ByName("gml_Script_textdata_en");
if (textData == null)
{
    ScriptMessage("TARGET DIALOGUE SYSTEM: gml_Script_textdata_en was not found.\nNo changes made.");
    return;
}

// Only these dialogue fragments are candidates. Because we first collect
// strings referenced by textdata_en, unrelated strings elsewhere in the game
// are ignored.
var replacements = new Dictionary<string, string>(StringComparer.Ordinal)
{
    { "Howdy! I'm FLOWEY", "Oh. It's YOU.\nGood. You're here.\nI've been waiting for you.\nThis time, I'm on your side." },
    { "FLOWEY the FLOWER", "FLOWEY...\nSomething is very wrong here.\nStay close to me." },
    { "I'M FLOWEY", "I'M FLOWEY.\nAnd yes... I remember.\nI'm helping you this time." },
    { "Kill or be killed.", "No. Not this time.\nThere has to be another way." },
    { "In this world, it's kill or be killed.", "They think this world is kill or be killed.\nThey're wrong." }
};

var referenced = new HashSet<UndertaleString>();

foreach (UndertaleInstruction instruction in textData.Instructions)
{
    if (instruction == null)
        continue;

    if (instruction.Type1 == UndertaleInstruction.DataType.String && instruction.ValueString != null)
    {
        UndertaleString resource = instruction.ValueString.Resource;
        if (resource != null)
            referenced.Add(resource);
    }
}

int referencedCount = referenced.Count;
int changed = 0;
var changedExamples = new List<string>();

foreach (UndertaleString str in referenced)
{
    if (str == null || string.IsNullOrEmpty(str.Content))
        continue;

    string original = str.Content;
    string updated = original;

    foreach (var pair in replacements)
    {
        if (updated.Contains(pair.Key))
            updated = updated.Replace(pair.Key, pair.Value.Replace("\\n", "\n"));
    }

    if (updated != original)
    {
        str.Content = updated;
        changed++;
        if (changedExamples.Count < 12)
            changedExamples.Add(original.Substring(0, Math.Min(original.Length, 80)));
    }
}

string examples = changedExamples.Count == 0
    ? "(none)"
    : string.Join("\n- ", changedExamples);

ScriptMessage(
    "===== TARGETED UNDERTALE DIALOGUE SYSTEM =====\n\n" +
    "Target: gml_Script_textdata_en\n" +
    "Dialogue strings referenced by target: " + referencedCount + "\n" +
    "Targeted strings changed: " + changed + "\n\n" +
    "GML compiler: NOT USED\n" +
    "Player movement: NOT MODIFIED\n" +
    "Rooms/objects/sprites: NOT MODIFIED\n\n" +
    "Changed examples:\n- " + examples + "\n\n" +
    "This patch only touched strings directly referenced by the English text-data script.\n" +
    "=============================================="
);
