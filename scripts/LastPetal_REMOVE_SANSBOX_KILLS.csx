using System;
using UndertaleModLib.Models;
using UndertaleModLib.Compiler;

// LAST PETAL - REMOVE SANSBOX KILLS CRASH
// Removes the exact bad code that referenced an instance `kills` variable
// inside obj_sansbox Create. Does not touch movement, sprites, dialogue,
// rooms, or any other object.

EnsureDataLoaded();

UndertaleCode target = Data.Code.ByName("gml_Object_obj_sansbox_Create_0");

if (target == null)
{
    ScriptError("SANSBOX FIX: gml_Object_obj_sansbox_Create_0 was not found. Nothing was changed.");
    return;
}

string existing = GetDecompiledText(target);
string cleaned = existing;

string[] badLines = new[]
{
    "if (variable_global_exists(\"kills\")) global.last_petal_kills = max(global.last_petal_kills, kills);",
    "if (variable_global_exists(\"kills\")) global.last_petal_kills = max(global.last_petal_kills, kills)"
};

int removed = 0;

foreach (string badLine in badLines)
{
    while (cleaned.Contains(badLine, StringComparison.Ordinal))
    {
        cleaned = cleaned.Replace(badLine, "", StringComparison.Ordinal);
        removed++;
    }
}

if (removed == 0)
{
    if (cleaned.Contains("// LAST PETAL SAFE MASTER V2 STATE", StringComparison.Ordinal))
    {
        ScriptMessage("SANSBOX FIX: the old marker is present, but the bad kills line was not found. No changes made.");
    }
    else
    {
        ScriptMessage("SANSBOX FIX: no known bad kills reference was found. No changes made.");
    }
    return;
}

var imports = new CodeImportGroup(Data)
{
    MainThreadAction = MainThreadAction
};

imports.QueueReplace(target, cleaned);
var result = imports.Import();

if (!result.Successful)
{
    ScriptError("SANSBOX FIX aborted.\n\n" + result.PrintAllErrors(false));
    return;
}

ChangeSelection(target);
ScriptMessage(
    "SANSBOX KILLS FIX COMPLETE.\n\n" +
    "Removed bad instance-kills references: " + removed + "\n\n" +
    "Only obj_sansbox Create was changed.\n" +
    "Movement, sprites, dialogue, rooms, and other objects were not changed.\n\n" +
    "Save a backup before saving data.win."
);
