using System;
using UndertaleModLib.Models;
using UndertaleModLib.Compiler;

// LAST PETAL - SAFE FLOWEY TEXT
// Patches ONLY an existing Flowey Create event.
// Does not touch Step events, movement, draw code, or the global text renderer.
// Creates adaptive state for later targeted dialogue work.

EnsureDataLoaded();

var candidates = Data.Code.FindAll(c => c?.Name?.Content != null &&
    c.Name.Content.IndexOf("flowey", StringComparison.OrdinalIgnoreCase) >= 0 &&
    c.Name.Content.EndsWith("Create_0", StringComparison.OrdinalIgnoreCase));

if (candidates.Count == 0)
{
    ScriptError("SAFE FLOWEY TEXT: no existing Flowey Create event was found. Nothing was changed.");
    return;
}

var target = candidates[0];
string existing = GetDecompiledText(target);
const string marker = "// LAST PETAL SAFE FLOWEY TEXT STATE";

if (existing.Contains(marker, StringComparison.Ordinal))
{
    ScriptMessage("SAFE FLOWEY TEXT is already installed. Nothing was changed.");
    return;
}

string patch = @"

// LAST PETAL SAFE FLOWEY TEXT STATE
if (!variable_global_exists(""last_petal_memory"")) global.last_petal_memory = 0;
if (!variable_global_exists(""last_petal_flowey_mood"")) global.last_petal_flowey_mood = 0;
if (!variable_global_exists(""last_petal_flowey_seen"")) global.last_petal_flowey_seen = 0;
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
