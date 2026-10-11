using System;
using System.Linq;
using UndertaleModLib.Models;
using UndertaleModLib.Compiler;

// LAST PETAL - SAFE FLOWEY TEXT
// Patches ONLY an existing Flowey Create event.
// Does not touch Step events, movement, draw code, or the global text renderer.
// Creates adaptive state for later targeted dialogue work.

EnsureDataLoaded();

UndertaleCode target = null;

foreach (var code in Data.Code)
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
    ScriptError("SAFE FLOWEY TEXT: no existing Flowey Create event was found. Nothing was changed.");
    return;
}

string existing = GetDecompiledText(target);
const string marker = "// LAST PETAL SAFE FLOWEY TEXT STATE";

if (existing.Contains(marker, StringComparison.Ordinal))
{
    ScriptMessage("SAFE FLOWEY TEXT is already installed. Nothing was changed.");
    return;
}

string patch = @"

// LAST PETAL SAFE FLOWEY TEXT STATE
if (!variable_global_exists(\"last_petal_memory\")) global.last_petal_memory = 0;
if (!variable_global_exists(\"last_petal_flowey_mood\")) global.last_petal_flowey_mood = 0;
if (!variable_global_exists(\"last_petal_flowey_seen\")) global.last_petal_flowey_seen = 0;
global.last_petal_flowey_seen += 1;
";

var imports = new CodeImportGroup(Data)
{
    MainThreadAction = MainThreadAction
};

imports.QueueReplace(target, existing + patch);
var result = imports.Import();

if (!result.Successful)
{
    ScriptError("SAFE FLOWEY TEXT aborted:\n" + result.PrintAllErrors(false));
    return;
}

ChangeSelection(target);
ScriptMessage("SAFE FLOWEY TEXT installed.\n\nOnly one existing Flowey Create event was changed.\nNo Step events, movement, sprite transforms, or global text rendering were modified.");