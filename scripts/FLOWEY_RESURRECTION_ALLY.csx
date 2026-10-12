using System;
using System.Linq;
using UndertaleModLib.Models;
using UndertaleModLib.Compiler;

// FLOWEY RESURRECTION // ALLY
// Restores the mod's Flowey ally state whenever an existing Flowey instance is created.
// This does not alter player movement, Step events, rooms, sprites, or the text renderer.
// It also clears the mod's Flowey-scared/hates-player state so Flowey is on the player's side.
// NOTE: this patches the existing Flowey object; it does not invent a missing Flowey object.

EnsureDataLoaded();

var targets = Data.Code.Where(c => c?.Name?.Content != null &&
    c.Name.Content.IndexOf("flowey", StringComparison.OrdinalIgnoreCase) >= 0 &&
    c.Name.Content.EndsWith("Create_0", StringComparison.OrdinalIgnoreCase)).ToList();

if (targets.Count == 0)
{
    ScriptError("FLOWEY RESURRECTION: no existing Flowey Create event was found. Nothing was changed.");
    return;
}

var imports = new CodeImportGroup(Data)
{
    MainThreadAction = MainThreadAction
};

int queued = 0;

foreach (UndertaleCode target in targets)
{
    string existing = GetDecompiledText(target);
    const string marker = "// FLOWEY RESURRECTION ALLY STATE";

    if (existing.Contains(marker, StringComparison.Ordinal))
        continue;

    string patch =
        "// FLOWEY RESURRECTION ALLY STATE\n" +
        "if (!variable_global_exists(\"last_petal_flowey_returned\")) global.last_petal_flowey_returned = false;\n" +
        "if (!variable_global_exists(\"last_petal_flowey_hates_player\")) global.last_petal_flowey_hates_player = false;\n" +
        "if (!variable_global_exists(\"last_petal_flowey_scared\")) global.last_petal_flowey_scared = false;\n" +
        "if (!variable_global_exists(\"last_petal_flowey_ally\")) global.last_petal_flowey_ally = false;\n" +
        "global.last_petal_flowey_returned = true;\n" +
        "global.last_petal_flowey_hates_player = false;\n" +
        "global.last_petal_flowey_scared = false;\n" +
        "global.last_petal_flowey_ally = true;\n" +
        "last_petal_flowey_reaction = \"ally_returned\";\n";

    imports.QueueReplace(target, patch + "\n" + existing);
    queued++;
}

if (queued == 0)
{
    ScriptMessage("FLOWEY RESURRECTION is already installed. Nothing was changed.");
    return;
}

var result = imports.Import();

if (!result.Successful)
{
    ScriptError("FLOWEY RESURRECTION aborted:\n\n" + result.PrintAllErrors(false));
    return;
}

ScriptMessage(
    "FLOWEY RESURRECTION // ALLY installed.\n\n" +
    "Existing Flowey Create events patched: " + queued + "\n\n" +
    "Flowey returns as an ally when his existing object is created.\n" +
    "His mod horror flags are reset so he does not hate or fear the player.\n\n" +
    "No player movement, Step events, rooms, sprites, or text renderer were changed.\n\n" +
    "BACK UP data.win before saving."
);
