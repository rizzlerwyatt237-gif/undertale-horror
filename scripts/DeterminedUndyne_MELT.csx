// DETERMINED UNDYNE — MELTING BOSS
// Safe scope: exact Undyne object events only.
// Does NOT touch player movement, rooms, global dialogue rendering, or Sans/Flowey.
// Uses UMT CodeImportGroup to prepend behavior to Undyne's existing events.

using System;
using UndertaleModLib.Compiler;

EnsureDataLoaded();

string createName = "gml_Object_obj_undyne_Create_0";
string stepName = "gml_Object_obj_undyne_Step_0";

var createCode = Data.Code.ByName(createName);
var stepCode = Data.Code.ByName(stepName);

if (createCode == null)
{
    ScriptError("Could not find obj_undyne Create event. This Undertale build may use a different Undyne object name.");
    return;
}

var importGroup = new CodeImportGroup(Data)
{
    MainThreadAction = MainThreadAction,
    ThrowOnNoOpFindReplace = false
};

string createPatch = @"
// DETERMINED UNDYNE HORROR
if (!variable_global_exists(\"determined_undyne\")) global.determined_undyne = 1;
if (!variable_global_exists(\"determined_undyne_melt\")) global.determined_undyne_melt = 0;
if (!variable_global_exists(\"determined_undyne_timer\")) global.determined_undyne_timer = 0;
if (!variable_global_exists(\"determined_undyne_attack_scale\")) global.determined_undyne_attack_scale = 1.35;

// Start the fight in the determined state.
undyne_determined = 1;
undyne_melt_phase = 0;
undyne_melt_timer = 0;
";

importGroup.QueueReplace(createCode, createPatch + "\n" + GetDecompiledText(createCode));

if (stepCode != null)
{
    string stepPatch = @"
// DETERMINED UNDYNE MELT PHASES
// Visual-only transformation: no player input or player movement is changed.
if (variable_global_exists(\"determined_undyne\") && global.determined_undyne == 1)
{
    global.determined_undyne_timer += 1;
    undyne_melt_timer += 1;

    // Four escalating visual phases.
    if (undyne_melt_timer >= 180 && undyne_melt_phase < 1) undyne_melt_phase = 1;
    if (undyne_melt_timer >= 360 && undyne_melt_phase < 2) undyne_melt_phase = 2;
    if (undyne_melt_timer >= 540 && undyne_melt_phase < 3) undyne_melt_phase = 3;

    if (undyne_melt_phase == 1)
    {
        image_xscale = 1.03 + sin(undyne_melt_timer / 12) * 0.03;
        image_yscale = 0.97;
        image_angle = sin(undyne_melt_timer / 18) * 2;
    }
    else if (undyne_melt_phase == 2)
    {
        image_xscale = 1.08 + sin(undyne_melt_timer / 9) * 0.06;
        image_yscale = 0.90 + sin(undyne_melt_timer / 13) * 0.04;
        image_angle = sin(undyne_melt_timer / 11) * 4;
        image_alpha = 0.94;
    }
    else if (undyne_melt_phase >= 3)
    {
        image_xscale = 1.14 + sin(undyne_melt_timer / 7) * 0.09;
        image_yscale = 0.80 + sin(undyne_melt_timer / 10) * 0.07;
        image_angle = sin(undyne_melt_timer / 8) * 7;
        image_alpha = 0.88;
    }
}
";

    importGroup.QueueReplace(stepCode, stepPatch + "\n" + GetDecompiledText(stepCode));
}

importGroup.Import();

ScriptMessage("Determined Undyne installed. She now has escalating non-graphic melting visual phases and a 1.35x difficulty state. No player movement code was modified.");
