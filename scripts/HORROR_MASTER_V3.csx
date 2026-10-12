using System;
using UndertaleModLib.Models;
using UndertaleModLib.Compiler;

EnsureDataLoaded();

// LAST PETAL HORROR MASTER V3
// Compatibility-first version.
// Does NOT hook the player's Step event or movement code.
// Adds shared horror state, adaptive flags, and Determined Undyne visuals.

var importGroup = new CodeImportGroup(Data)
{
    MainThreadAction = MainThreadAction,
    ThrowOnNoOpFindReplace = false
};

int patched = 0;

// SHARED HORROR STATE
var gameStart = Data.Scripts.ByName("SCR_GAMESTART", true)?.Code as UndertaleCode;
if (gameStart != null)
{
    string existing = GetDecompiledText(gameStart);
    if (!existing.Contains("LAST_PETAL_HORROR_MASTER_V3", StringComparison.Ordinal))
    {
        string startPatch =
            "// LAST_PETAL_HORROR_MASTER_V3\n" +
            "if (!variable_global_exists(\"horror_phase\")) global.horror_phase = 0;\n" +
            "if (!variable_global_exists(\"horror_tension\")) global.horror_tension = 0;\n" +
            "if (!variable_global_exists(\"horror_player_memory\")) global.horror_player_memory = 0;\n" +
            "if (!variable_global_exists(\"horror_genocide\")) global.horror_genocide = 0;\n" +
            "if (!variable_global_exists(\"horror_flowey_afraid\")) global.horror_flowey_afraid = 0;\n" +
            "if (!variable_global_exists(\"horror_sans_hostile\")) global.horror_sans_hostile = 0;\n" +
            "if (!variable_global_exists(\"horror_everyone_scared\")) global.horror_everyone_scared = 1;\n" +
            "if (!variable_global_exists(\"horror_ai_mode\")) global.horror_ai_mode = 1;\n" +
            "if (!variable_global_exists(\"determined_undyne_attack_scale\")) global.determined_undyne_attack_scale = 1.35;\n" +
            "global.horror_master_v3 = 1;\n\n";

        importGroup.QueueReplace(gameStart, startPatch + existing);
        patched++;
    }
}

// DETERMINED UNDYNE CREATE
var undyneCreate = Data.Code.ByName("gml_Object_obj_undyne_Create_0");
if (undyneCreate != null)
{
    string existing = GetDecompiledText(undyneCreate);
    if (!existing.Contains("DETERMINED_UNDYNE_V3", StringComparison.Ordinal))
    {
        string patch =
            "// DETERMINED_UNDYNE_V3\n" +
            "if (!variable_global_exists(\"determined_undyne_attack_scale\")) global.determined_undyne_attack_scale = 1.35;\n" +
            "if (!variable_global_exists(\"determined_undyne_active\")) global.determined_undyne_active = 1;\n" +
            "determined_undyne = 1;\n" +
            "undyne_melt_phase = 0;\n" +
            "undyne_melt_timer = 0;\n" +
            "undyne_base_xscale = image_xscale;\n" +
            "undyne_base_yscale = image_yscale;\n" +
            "undyne_base_alpha = image_alpha;\n";

        importGroup.QueueReplace(undyneCreate, patch + "\n" + existing);
        patched++;
    }
}
else
{
    ScriptMessage("obj_undyne Create event was not found; shared horror state was still installed.");
}

// DETERMINED UNDYNE STEP
// Only Undyne's Step event is touched. Player movement is untouched.
var undyneStep = Data.Code.ByName("gml_Object_obj_undyne_Step_0");
if (undyneStep != null)
{
    string existing = GetDecompiledText(undyneStep);
    if (!existing.Contains("DETERMINED_UNDYNE_MELT_V3", StringComparison.Ordinal))
    {
        string patch =
            "// DETERMINED_UNDYNE_MELT_V3\n" +
            "if (variable_global_exists(\"determined_undyne_active\") && global.determined_undyne_active == 1)\n" +
            "{\n" +
            "    undyne_melt_timer += 1;\n" +
            "    global.horror_tension = min(100, global.horror_tension + 0.01);\n" +
            "\n" +
            "    if (undyne_melt_timer >= 180) undyne_melt_phase = 1;\n" +
            "    if (undyne_melt_timer >= 360) undyne_melt_phase = 2;\n" +
            "    if (undyne_melt_timer >= 540) undyne_melt_phase = 3;\n" +
            "\n" +
            "    if (undyne_melt_phase == 0)\n" +
            "    {\n" +
            "        image_xscale = undyne_base_xscale;\n" +
            "        image_yscale = undyne_base_yscale;\n" +
            "        image_alpha = undyne_base_alpha;\n" +
            "    }\n" +
            "    else if (undyne_melt_phase == 1)\n" +
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
            "    else\n" +
            "    {\n" +
            "        image_xscale = undyne_base_xscale * (1.14 + sin(undyne_melt_timer / 7) * 0.09);\n" +
            "        image_yscale = undyne_base_yscale * (0.80 + sin(undyne_melt_timer / 10) * 0.07);\n" +
            "        image_angle = sin(undyne_melt_timer / 8) * 7;\n" +
            "        image_alpha = undyne_base_alpha * 0.88;\n" +
            "    }\n" +
            "}\n";

        importGroup.QueueReplace(undyneStep, patch + "\n" + existing);
        patched++;
    }
}

importGroup.Import();

ScriptMessage("LAST PETAL HORROR MASTER V3 installed. Patched " + patched + " compatible events. Player movement was not modified. Shared horror state + Determined Undyne phases are active.");
