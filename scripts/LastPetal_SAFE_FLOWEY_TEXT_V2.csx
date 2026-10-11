using System;
using UndertaleModLib.Models;
using UndertaleModLib.Compiler;

// LAST PETAL - SAFE FLOWEY TEXT V2
// Safe foundation only. Does not touch Step events, movement, draw code,
// dialogue rendering, sprites, rooms, or global text replacement.

EnsureDataLoaded();

UndertaleCode target = null;

foreach (UndertaleCode code in Data.Code)
{
    if (code == null || code.Name == null || code.Name.Content == null)
        continue;

    string name = code.Name.Content;

    if (name.IndexOf("flowey", StringComparison.OrdinalIgnoreCase) >= 0 &&
        name.EndsWith("Create_0", StringComparison.OrdinalIgnoreCase))
    {
        target = code;
        break;
    }
}

if (target == null)
{
    ScriptError("SAFE FLOWEY TEXT V2: no Flowey Create event was found. Nothing was changed.");
    return;
}

string existing = GetDecompiledText(target);
const string marker = "// LAST PETAL SAFE FLOWEY TEXT V2 STATE";

if (existing.Contains(marker, StringComparison.Ordinal))
{
    ScriptMessage("SAFE FLOWEY TEXT V2 is already installed. Nothing was changed.");
    return;
}

string patch = "\n\n// LAST PETAL SAFE FLOWEY TEXT V2 STATE\n" +
               "if (!variable_global_exists(\"last_petal_memory\")) global.last_petal_memory = 0;\n" +
               "if (!variable_global_exists(\"last_petal_flowey_mood\")) global.last_petal_flowey_mood = 0;\n" +
               "if (!variable_global_exists(\"last_petal_flowey_seen\")) global.last_petal_flowey_seen = 0;\n" +
               "global.last_petal_flowey_seen += 1;\n";

CodeImportGroup imports = new(Data)
{
    MainThreadAction = MainThreadAction
};

imports.QueueReplace(target, existing + patch);
var result = imports.Import();

if (!result.Successful)
{
    ScriptError("SAFE FLOWEY TEXT V2 aborted:\n" + result.PrintAllErrors(false));
    return;
}

ChangeSelection(target);
ScriptMessage("SAFE FLOWEY TEXT V2 installed successfully.\n\nOnly an existing Flowey Create event was changed.\nNo Step events, movement, sprites, rooms, or text renderer were modified.");
