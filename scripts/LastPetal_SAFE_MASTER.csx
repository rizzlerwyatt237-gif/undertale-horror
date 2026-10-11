using System;
using System.Collections.Generic;
using UndertaleModLib.Models;
using UndertaleModLib.Compiler;

// LAST PETAL SAFE MASTER
// Rebuild of the non-text horror systems.
// This version deliberately avoids Step events, movement, draw code,
// dialogue rendering, sprites, rooms, and global string replacement.

EnsureDataLoaded();

var targets = new List<UndertaleCode>();

foreach (UndertaleCode code in Data.Code)
{
    if (code == null || code.Name == null || code.Name.Content == null)
        continue;

    string name = code.Name.Content;

    bool wanted =
        name.IndexOf("flowey", StringComparison.OrdinalIgnoreCase) >= 0 ||
        name.IndexOf("sans", StringComparison.OrdinalIgnoreCase) >= 0 ||
        name.IndexOf("toriel", StringComparison.OrdinalIgnoreCase) >= 0 ||
        name.IndexOf("papyrus", StringComparison.OrdinalIgnoreCase) >= 0 ||
        name.IndexOf("undyne", StringComparison.OrdinalIgnoreCase) >= 0 ||
        name.IndexOf("asgore", StringComparison.OrdinalIgnoreCase) >= 0 ||
        name.IndexOf("mettaton", StringComparison.OrdinalIgnoreCase) >= 0 ||
        name.IndexOf("alphys", StringComparison.OrdinalIgnoreCase) >= 0 ||
        name.IndexOf("monsterkid", StringComparison.OrdinalIgnoreCase) >= 0 ||
        name.IndexOf("napstablook", StringComparison.OrdinalIgnoreCase) >= 0 ||
        name.IndexOf("temmie", StringComparison.OrdinalIgnoreCase) >= 0 ||
        name.IndexOf("doggo", StringComparison.OrdinalIgnoreCase) >= 0;

    if (wanted && name.EndsWith("Create_0", StringComparison.OrdinalIgnoreCase))
        targets.Add(code);
}

if (targets.Count == 0)
{
    ScriptError("SAFE MASTER: no supported character Create events were found. Nothing was changed.");
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

    if (existing.Contains("// LAST PETAL SAFE MASTER STATE"))
        continue;

    string patch = "\n\n// LAST PETAL SAFE MASTER STATE\n" +
                   "if (!variable_global_exists(\"last_petal_memory\")) global.last_petal_memory = 0;\n" +
                   "if (!variable_global_exists(\"last_petal_kills\")) global.last_petal_kills = 0;\n" +
                   "if (!variable_global_exists(\"last_petal_horror\")) global.last_petal_horror = 0;\n" +
                   "if (!variable_global_exists(\"last_petal_phase\")) global.last_petal_phase = 0;\n" +
                   "if (!variable_global_exists(\"last_petal_tension\")) global.last_petal_tension = 0;\n" +
                   "if (!variable_global_exists(\"last_petal_route\")) global.last_petal_route = 0;\n" +
                   "if (!variable_global_exists(\"last_petal_fear\")) global.last_petal_fear = 0;\n" +
                   "if (!variable_global_exists(\"last_petal_hatred\")) global.last_petal_hatred = 0;\n" +
                   "if (!variable_global_exists(\"last_petal_flowey_mood\")) global.last_petal_flowey_mood = 0;\n" +
                   "if (!variable_global_exists(\"last_petal_sans_hostile\")) global.last_petal_sans_hostile = 0;\n" +
                   "if (variable_global_exists(\"kills\")) global.last_petal_kills = global.kills;\n" +
                   "global.last_petal_horror = global.last_petal_kills > 0;\n" +
                   "global.last_petal_phase = (global.last_petal_kills >= 30) ? 4 : ((global.last_petal_kills >= 15) ? 3 : ((global.last_petal_kills >= 5) ? 2 : ((global.last_petal_kills >= 1) ? 1 : 0));\n" +
                   "global.last_petal_tension = global.last_petal_kills;\n" +
                   "global.last_petal_fear = global.last_petal_kills > 0;\n" +
                   "global.last_petal_hatred = global.last_petal_kills >= 15;\n" +
                   "global.last_petal_flowey_mood = global.last_petal_kills > 0 ? -1 : 0;\n" +
                   "global.last_petal_sans_hostile = global.last_petal_kills > 0 ? 1 : 0;\n" +
                   "global.last_petal_memory += 1;\n";

    imports.QueueReplace(target, existing + patch);
    queued++;
}

if (queued == 0)
{
    ScriptMessage("SAFE MASTER is already installed on the supported Create events. Nothing was changed.");
    return;
}

var result = imports.Import();

if (!result.Successful)
{
    ScriptError("SAFE MASTER aborted. No new horror behavior was installed.\n\n" + result.PrintAllErrors(false));
    return;
}

ScriptMessage(
    "LAST PETAL SAFE MASTER installed.\n\n" +
    "Create events changed: " + queued + "\n\n" +
    "Rebuilt: shared memory, kill/route state, horror phases, tension, fear, hatred, Flowey mood, and Sans hostility state.\n\n" +
    "NOT touched: Step events, movement, draw code, dialogue renderer, sprites, rooms, or global text replacement.\n\n" +
    "Save a backup of data.win before testing."
);
