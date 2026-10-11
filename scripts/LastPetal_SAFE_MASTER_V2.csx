using System;
using System.Collections.Generic;
using UndertaleModLib.Models;
using UndertaleModLib.Compiler;

// LAST PETAL SAFE MASTER V2 - FIXED
// Rebuilds the old horror-state systems without the old Step-event hooks.
// Never reads an unqualified `kills` variable.

EnsureDataLoaded();

var characterNames = new[] {
    "flowey", "sans", "toriel", "papyrus", "undyne", "asgore",
    "mettaton", "alphys", "monsterkid", "napstablook", "temmie", "doggo"
};

var targets = new List<UndertaleCode>();

foreach (UndertaleCode code in Data.Code)
{
    if (code == null || code.Name == null || code.Name.Content == null)
        continue;

    string name = code.Name.Content;
    if (!name.EndsWith("Create_0", StringComparison.OrdinalIgnoreCase))
        continue;

    foreach (string character in characterNames)
    {
        if (name.IndexOf(character, StringComparison.OrdinalIgnoreCase) >= 0)
        {
            targets.Add(code);
            break;
        }
    }
}

if (targets.Count == 0)
{
    ScriptError("SAFE MASTER V2 FIXED: no supported character Create events were found. Nothing was changed.");
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

    if (existing.Contains("// LAST PETAL SAFE MASTER V2 FIXED STATE"))
        continue;

    string patch =
        "\n\n// LAST PETAL SAFE MASTER V2 FIXED STATE\n" +
        "if (!variable_global_exists(\"last_petal_memory\")) global.last_petal_memory = 0;\n" +
        "if (!variable_global_exists(\"last_petal_kills\")) global.last_petal_kills = 0;\n" +
        "if (!variable_global_exists(\"last_petal_horror\")) global.last_petal_horror = 0;\n" +
        "if (!variable_global_exists(\"last_petal_phase\")) global.last_petal_phase = 0;\n" +
        "if (!variable_global_exists(\"last_petal_tension\")) global.last_petal_tension = 0;\n" +
        "if (!variable_global_exists(\"last_petal_fear\")) global.last_petal_fear = 0;\n" +
        "if (!variable_global_exists(\"last_petal_hatred\")) global.last_petal_hatred = 0;\n" +
        "if (!variable_global_exists(\"last_petal_route\")) global.last_petal_route = \"mercy\";\n" +
        "if (!variable_global_exists(\"last_petal_everyone_horrified\")) global.last_petal_everyone_horrified = false;\n" +
        "if (!variable_global_exists(\"last_petal_flowey_scared\")) global.last_petal_flowey_scared = false;\n" +
        "if (!variable_global_exists(\"last_petal_flowey_hates_player\")) global.last_petal_flowey_hates_player = false;\n" +
        "if (!variable_global_exists(\"last_petal_sans_hostile\")) global.last_petal_sans_hostile = false;\n" +
        "if (!variable_global_exists(\"last_petal_sans_no_mercy\")) global.last_petal_sans_no_mercy = false;\n" +
        "if (!variable_global_exists(\"last_petal_chara_awareness\")) global.last_petal_chara_awareness = 0;\n" +
        "if (!variable_global_exists(\"last_petal_chara_mood\")) global.last_petal_chara_mood = 0;\n" +
        "\n" +
        "// Only read the global kill counter when it exists.\n" +
        "if (variable_global_exists(\"killcount\")) global.last_petal_kills = max(global.last_petal_kills, global.killcount);\n" +
        "\n" +
        "global.last_petal_memory += 1;\n" +
        "global.last_petal_horror = clamp(global.last_petal_memory + global.last_petal_kills, 0, 100);\n" +
        "global.last_petal_phase = 0;\n" +
        "if (global.last_petal_kills >= 1) global.last_petal_phase = 1;\n" +
        "if (global.last_petal_kills >= 5) global.last_petal_phase = 2;\n" +
        "if (global.last_petal_kills >= 15) global.last_petal_phase = 3;\n" +
        "if (global.last_petal_kills >= 30) global.last_petal_phase = 4;\n" +
        "\n" +
        "global.last_petal_route = \"mercy\";\n" +
        "if (global.last_petal_kills >= 1) global.last_petal_route = \"violent\";\n" +
        "if (global.last_petal_kills >= 15) global.last_petal_route = \"genocide\";\n" +
        "\n" +
        "global.last_petal_fear = clamp(global.last_petal_kills * 4, 0, 100);\n" +
        "global.last_petal_hatred = clamp(global.last_petal_kills * 5, 0, 100);\n" +
        "global.last_petal_everyone_horrified = (global.last_petal_kills >= 15);\n" +
        "global.last_petal_flowey_scared = (global.last_petal_kills >= 1);\n" +
        "global.last_petal_flowey_hates_player = false;\n" +
        "global.last_petal_sans_hostile = (global.last_petal_kills >= 1);\n" +
        "global.last_petal_sans_no_mercy = (global.last_petal_kills >= 15);\n" +
        "global.last_petal_chara_awareness = global.last_petal_memory + global.last_petal_kills;\n" +
        "global.last_petal_chara_mood = global.last_petal_kills >= 15 ? 3 : (global.last_petal_kills >= 1 ? 1 : 0);\n" +
        "\n" +
        "last_petal_reaction = \"normal\";\n" +
        "if (global.last_petal_kills >= 1) last_petal_reaction = \"afraid\";\n" +
        "if (global.last_petal_kills >= 5) last_petal_reaction = \"hostile\";\n" +
        "if (global.last_petal_kills >= 15) last_petal_reaction = \"horrified\";\n";

    imports.QueueReplace(target, existing + patch);
    queued++;
}

if (queued == 0)
{
    ScriptMessage("SAFE MASTER V2 FIXED is already installed. Nothing was changed.");
    return;
}

var result = imports.Import();

if (!result.Successful)
{
    ScriptError("SAFE MASTER V2 FIXED aborted.\n\n" + result.PrintAllErrors(false));
    return;
}

ScriptMessage(
    "LAST PETAL SAFE MASTER V2 FIXED installed.\n\n" +
    "Character Create events changed: " + queued + "\n\n" +
    "Fixed the obj_sansbox undefined kills crash.\n" +
    "No Step events, movement, dialogue rendering, sprites, or rooms were changed.\n\n" +
    "Back up data.win before saving."
);
