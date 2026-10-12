using System;
using System.Collections.Generic;
using System.Linq;
using UndertaleModLib.Models;
using UndertaleModLib.Compiler;

// ULTIMATE HORROR MASTER
// Shared horror state, Flowey ally state, horror text, Sans state,
// Determined Undyne visual phases, and an opening AI credit screen.
// Player movement is not modified.
// BACK UP data.win before running.

EnsureDataLoaded();

var importGroup = new CodeImportGroup(Data)
{
    MainThreadAction = MainThreadAction,
    ThrowOnNoOpFindReplace = false
};

int queued = 0;
int textChanged = 0;
int floweyPatched = 0;
int undynePatched = 0;
int sansCreateCount = 0;

// ============================================================
// 1. STARTUP CREDIT + SHARED HORROR STATE
// ============================================================
var gameStart = Data.Scripts.ByName("SCR_GAMESTART", true)?.Code as UndertaleCode;
if (gameStart != null)
{
    string existing = GetDecompiledText(gameStart);

    if (!existing.Contains("ULTIMATE_HORROR_MASTER_START", StringComparison.Ordinal))
    {
        string patch =
            "// ULTIMATE_HORROR_MASTER_START\n" +
            "show_message(\"UNDERTALE HORROR\\n\\nMOD MADE BY AI\\n\\nPress OK to continue.\");\n" +
            "// ULTIMATE_HORROR_MASTER_STATE\n" +
            "if (!variable_global_exists(\"horror_phase\")) global.horror_phase = 0;\n" +
            "if (!variable_global_exists(\"horror_tension\")) global.horror_tension = 0;\n" +
            "if (!variable_global_exists(\"horror_player_memory\")) global.horror_player_memory = 0;\n" +
            "if (!variable_global_exists(\"horror_genocide\")) global.horror_genocide = 0;\n" +
            "if (!variable_global_exists(\"horror_everyone_scared\")) global.horror_everyone_scared = 1;\n" +
            "if (!variable_global_exists(\"horror_sans_hostile\")) global.horror_sans_hostile = 0;\n" +
            "if (!variable_global_exists(\"horror_flowey_ally\")) global.horror_flowey_ally = true;\n" +
            "if (!variable_global_exists(\"last_petal_flowey_returned\")) global.last_petal_flowey_returned = false;\n" +
            "if (!variable_global_exists(\"last_petal_flowey_hates_player\")) global.last_petal_flowey_hates_player = false;\n" +
            "if (!variable_global_exists(\"last_petal_flowey_scared\")) global.last_petal_flowey_scared = false;\n" +
            "global.horror_master_active = 1;\n" +
            "global.horror_tension = min(100, global.horror_tension + 5);\n\n";

        importGroup.QueueReplace(gameStart, patch + existing);
        queued++;
    }
}
else
{
    ScriptError("Could not find SCR_GAMESTART. The opening AI credit was not installed.");
    return;
}

// ============================================================
// 2. FLOWEY ALLY / RETURN STATE
// ============================================================
var floweyCreates = Data.Code
    .Where(c => c?.Name?.Content != null &&
                c.Name.Content.IndexOf("flowey", StringComparison.OrdinalIgnoreCase) >= 0 &&
                c.Name.Content.EndsWith("Create_0", StringComparison.OrdinalIgnoreCase))
    .ToList();

foreach (UndertaleCode floweyCreate in floweyCreates)
{
    string existing = GetDecompiledText(floweyCreate);
    if (existing.Contains("ULTIMATE_FLOWEY_ALLY", StringComparison.Ordinal))
        continue;

    string patch =
        "// ULTIMATE_FLOWEY_ALLY\n" +
        "if (!variable_global_exists(\"last_petal_flowey_returned\")) global.last_petal_flowey_returned = false;\n" +
        "if (!variable_global_exists(\"last_petal_flowey_hates_player\")) global.last_petal_flowey_hates_player = false;\n" +
        "if (!variable_global_exists(\"last_petal_flowey_scared\")) global.last_petal_flowey_scared = false;\n" +
        "if (!variable_global_exists(\"horror_flowey_ally\")) global.horror_flowey_ally = true;\n" +
        "global.last_petal_flowey_returned = true;\n" +
        "global.last_petal_flowey_hates_player = false;\n" +
        "global.last_petal_flowey_scared = false;\n" +
        "global.horror_flowey_ally = true;\n" +
        "last_petal_flowey_reaction = \"ally_returned\";\n";

    importGroup.QueueReplace(floweyCreate, patch + "\n" + existing);
    floweyPatched++;
    queued++;
}

// ============================================================
// 3. DETERMINED UNDYNE
// ============================================================
var undyneCreate = Data.Code.ByName("gml_Object_obj_undyne_Create_0");
if (undyneCreate != null)
{
    string existing = GetDecompiledText(undyneCreate);
    if (!existing.Contains("ULTIMATE_UNDYNE_CREATE", StringComparison.Ordinal))
    {
        string patch =
            "// ULTIMATE_UNDYNE_CREATE\n" +
            "if (!variable_global_exists(\"determined_undyne_attack_scale\")) global.determined_undyne_attack_scale = 1.35;\n" +
            "if (!variable_global_exists(\"determined_undyne_active\")) global.determined_undyne_active = 1;\n" +
            "determined_undyne = 1;\n" +
            "undyne_melt_phase = 0;\n" +
            "undyne_melt_timer = 0;\n" +
            "undyne_base_xscale = image_xscale;\n" +
            "undyne_base_yscale = image_yscale;\n" +
            "undyne_base_alpha = image_alpha;\n";
        importGroup.QueueReplace(undyneCreate, patch + "\n" + existing);
        undynePatched++;
        queued++;
    }
}

var undyneStep = Data.Code.ByName("gml_Object_obj_undyne_Step_0");
if (undyneStep != null)
{
    string existing = GetDecompiledText(undyneStep);
    if (!existing.Contains("ULTIMATE_UNDYNE_MELT", StringComparison.Ordinal))
    {
        string patch =
            "// ULTIMATE_UNDYNE_MELT\n" +
            "if (variable_global_exists(\"determined_undyne_active\") && global.determined_undyne_active == 1)\n" +
            "{\n" +
            "    undyne_melt_timer += 1;\n" +
            "    global.horror_tension = min(100, global.horror_tension + 0.01);\n" +
            "    if (undyne_melt_timer >= 180) undyne_melt_phase = 1;\n" +
            "    if (undyne_melt_timer >= 360) undyne_melt_phase = 2;\n" +
            "    if (undyne_melt_timer >= 540) undyne_melt_phase = 3;\n" +
            "    if (undyne_melt_phase == 1)\n" +
            "    {\n" +
            "        image_xscale = undyne_base_xscale * (1.03 + sin(undyne_melt_timer / 12) * 0.03);\n" +
            "        image_yscale = undyne_base_yscale * 0.97;\n" +
            "        image_angle = sin(undyne_melt_timer / 18) * 2;\n" +
            "    }\n" +
            "    else if (undyne_melt_phase == 2)\n" +
            "    {\n" +
            "        image_xscale = undyne_base_xscale * (1.08 + sin(undyne_melt_timer / 9) * 0.06);\n" +
            "        image_yscale = undyne_base_yscale * (0.90 + sin(undyne_melt_timer / 13) * 0.04);\n" +
            "        image_angle = sin(undyne_melt_timer / 11) * 4;\n" +
            "        image_alpha = undyne_base_alpha * 0.94;\n" +
            "    }\n" +
            "    else if (undyne_melt_phase >= 3)\n" +
            "    {\n" +
            "        image_xscale = undyne_base_xscale * (1.14 + sin(undyne_melt_timer / 7) * 0.09);\n" +
            "        image_yscale = undyne_base_yscale * (0.80 + sin(undyne_melt_timer / 10) * 0.07);\n" +
            "        image_angle = sin(undyne_melt_timer / 8) * 7;\n" +
            "        image_alpha = undyne_base_alpha * 0.88;\n" +
            "    }\n" +
            "}\n";
        importGroup.QueueReplace(undyneStep, patch + "\n" + existing);
        undynePatched++;
        queued++;
    }
}

// ============================================================
// 4. HORROR TEXT
// ============================================================
var replacements = new Dictionary<string, string>(StringComparer.Ordinal)
{
    { "Howdy! I'm FLOWEY. FLOWEY the FLOWER!", "Oh. It's YOU.\nGood. You're here.\nI've been waiting for you.\nThis time, I'm on your side." },
    { "I'M FLOWEY! FLOWEY THE FLOWER!", "I'M FLOWEY.\nAnd before you ask... yes.\nI remember.\nI'm helping you this time." },
    { "You IDIOT!", "Hey! Don't listen to them.\nThey want you scared.\nStay with me. I've got you." },
    { "Kill or be killed.", "No. Not this time.\nStay with me.\nI'll help you find another way." },
    { "In this world, it's kill or be killed.", "They think this world is kill or be killed.\nThey're wrong.\nAnd I'll prove it to you." },
    { "hey. what's up?", "heh.\nyou again.\nflowey's with you now?\n...great. just great." },
    { "it's a beautiful day outside.", "it's a quiet day outside.\ntoo quiet.\neveryone heard what you did." },
    { "NYEH HEH HEH!", "NYEH...\nHEH...\nI AM NOT AFRAID!\n...I AM A LITTLE AFRAID." },
    { "I WILL CAPTURE YOU!", "I... WILL STOP YOU.\nI HAVE TO PROTECT EVERYONE." },
    { "Hello, my child.", "Hello, my child...\nPlease.\nI don't know what you have become." },
    { "YOU'RE GONNA HAVE A BAD TIME!", "YOU'RE GOING TO ANSWER FOR WHAT YOU'VE DONE.\nI'M NOT BACKING DOWN." },
    { "U-um...", "U-um...\nI-I know you're there.\nPlease don't come any closer." },
    { "Greetings. I am ASGORE DREEMURR, King of the Monsters.", "Greetings...\nI am ASGORE DREEMURR.\nI had hoped this meeting would be different." }
};

foreach (var str in Data.Strings)
{
    if (str == null || string.IsNullOrEmpty(str.Content)) continue;
    if (replacements.TryGetValue(str.Content, out string replacement))
    {
        str.Content = replacement;
        textChanged++;
    }
}

// ============================================================
// 5. SANS HORROR STATE
// ============================================================
foreach (UndertaleCode code in Data.Code.Where(c => c?.Name?.Content != null))
{
    string name = code.Name.Content;
    if (name.IndexOf("sans", StringComparison.OrdinalIgnoreCase) < 0 ||
        !name.EndsWith("Create_0", StringComparison.OrdinalIgnoreCase)) continue;

    string existing = GetDecompiledText(code);
    if (existing.Contains("ULTIMATE_SANS_STATE", StringComparison.Ordinal)) continue;

    string patch =
        "// ULTIMATE_SANS_STATE\n" +
        "if (!variable_global_exists(\"horror_sans_hostile\")) global.horror_sans_hostile = 0;\n" +
        "if (!variable_global_exists(\"horror_player_memory\")) global.horror_player_memory = 0;\n" +
        "if (global.horror_player_memory >= 3) global.horror_sans_hostile = 1;\n";

    importGroup.QueueReplace(code, patch + "\n" + existing);
    sansCreateCount++;
    queued++;
}

var result = importGroup.Import();
if (!result.Successful)
{
    ScriptError("ULTIMATE HORROR MASTER aborted:\n\n" + result.PrintAllErrors(false));
    return;
}

ScriptMessage(
    "===== ULTIMATE HORROR MASTER =====\n\n" +
    "Installed successfully.\n\n" +
    "Opening credit: MOD MADE BY AI\n" +
    "GML events queued: " + queued + "\n" +
    "Flowey Create events patched: " + floweyPatched + "\n" +
    "Undyne systems patched: " + undynePatched + "\n" +
    "Sans Create events patched: " + sansCreateCount + "\n" +
    "Horror strings changed: " + textChanged + "\n\n" +
    "Player movement: NOT MODIFIED\n" +
    "BACK UP data.win before saving.\n" +
    "================================="
);