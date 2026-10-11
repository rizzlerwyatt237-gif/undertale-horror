using System;
using System.Linq;
using UndertaleModLib.Models;
using UndertaleModLib.Compiler;

// FLOWEY // LAST PETAL
// Compatibility-first version: patches existing events only.
// It deliberately does not create new scripts or use EnsureScript.

EnsureDataLoaded();

void Log(string text)
{
    Console.WriteLine("[LAST PETAL] " + text);
}

UndertaleCode FindEvent(string objectPart, string eventPart)
{
    return Data.Code.FirstOrDefault(c => c?.Name?.Content != null &&
        c.Name.Content.IndexOf(objectPart, StringComparison.OrdinalIgnoreCase) >= 0 &&
        c.Name.Content.IndexOf(eventPart, StringComparison.OrdinalIgnoreCase) >= 0);
}

bool PatchEvent(UndertaleCode code, string injected)
{
    if (code == null)
        return false;

    string original = GetDecompiledText(code);
    if (original.Contains("LAST PETAL 2026 HOOK"))
    {
        Log("Already patched: " + code.Name.Content);
        return true;
    }

    var imports = new CodeImportGroup(Data)
    {
        MainThreadAction = MainThreadAction
    };

    imports.QueueReplace(code,
        "// LAST PETAL 2026 HOOK\n" + injected + "\n\n" + original);

    var result = imports.Import();
    if (!result.Successful)
    {
        ScriptError("Could not patch " + code.Name.Content + ":\n" + result.PrintAllErrors(false));
        return false;
    }

    Log("Patched: " + code.Name.Content);
    return true;
}

var floweyCreate = FindEvent("flowey", "Create_0");
var floweyStep = FindEvent("flowey", "Step_0");
var sansCreate = FindEvent("sans", "Create_0");
var sansStep = FindEvent("sans", "Step_0");

string floweyCreatePatch = @"
if (!variable_global_exists(""last_petal_memory"")) global.last_petal_memory = 0;
if (!variable_global_exists(""last_petal_kills"")) global.last_petal_kills = 0;
if (!variable_global_exists(""last_petal_horror"")) global.last_petal_horror = 0;
if (!variable_global_exists(""last_petal_phase"")) global.last_petal_phase = 0;
if (!variable_global_exists(""last_petal_tension"")) global.last_petal_tension = 0;
if (!variable_global_exists(""last_petal_fakeout"")) global.last_petal_fakeout = 0;

global.last_petal_memory = max(global.last_petal_memory, 1);
if (variable_global_exists(""kills"")) global.last_petal_kills = max(global.last_petal_kills, kills);
if (variable_global_exists(""killcount"")) global.last_petal_kills = max(global.last_petal_kills, killcount);

global.last_petal_horror = clamp(global.last_petal_memory + global.last_petal_kills, 0, 20);
global.last_petal_phase = 0;
if (global.last_petal_horror >= 5) global.last_petal_phase = 1;
if (global.last_petal_horror >= 10) global.last_petal_phase = 2;
if (global.last_petal_horror >= 15) global.last_petal_phase = 3;
global.last_petal_tension = clamp(global.last_petal_horror * 5, 0, 100);
global.last_petal_flowey_knows = true;
";

string floweyStepPatch = @"
if (!variable_global_exists(""last_petal_horror"")) global.last_petal_horror = 0;
if (!variable_global_exists(""last_petal_phase"")) global.last_petal_phase = 0;
if (!variable_global_exists(""last_petal_fakeout"")) global.last_petal_fakeout = 0;

global.last_petal_fakeout = 0;
if (global.last_petal_phase >= 1 && irandom(179) == 0) global.last_petal_fakeout = 1;

if (global.last_petal_phase >= 1 && irandom(120) == 0)
    image_xscale = -image_xscale;

if (global.last_petal_phase >= 2 && irandom(150) == 0)
    image_yscale = -abs(image_yscale);

if (global.last_petal_phase >= 3 && irandom(180) == 0)
    image_angle += choose(-3, 3, -6, 6);

last_petal_flowey_phase = global.last_petal_phase;
last_petal_flowey_tension = global.last_petal_tension;
last_petal_flowey_fakeout = global.last_petal_fakeout;
";

string sansCreatePatch = @"
if (!variable_global_exists(""last_petal_horror"")) global.last_petal_horror = 0;
if (!variable_global_exists(""last_petal_phase"")) global.last_petal_phase = 0;
if (!variable_global_exists(""last_petal_tension"")) global.last_petal_tension = 0;
last_petal_sans_memory = global.last_petal_horror;
last_petal_sans_phase = global.last_petal_phase;
";

string sansStepPatch = @"
if (!variable_global_exists(""last_petal_horror"")) global.last_petal_horror = 0;
if (!variable_global_exists(""last_petal_phase"")) global.last_petal_phase = 0;
if (!variable_global_exists(""last_petal_tension"")) global.last_petal_tension = 0;

last_petal_sans_memory = global.last_petal_horror;
last_petal_sans_phase = global.last_petal_phase;
last_petal_sans_tension = global.last_petal_tension;
last_petal_sans_pressure = choose(""observe"", ""fakeout"", ""pressure"");
if (global.last_petal_phase >= 2)
    last_petal_sans_pressure = choose(""fakeout"", ""pressure"", ""relentless"");
if (global.last_petal_phase >= 3)
    last_petal_sans_pressure = choose(""pressure"", ""relentless"", ""ambush"");
";

int patched = 0;
if (PatchEvent(floweyCreate, floweyCreatePatch)) patched++;
if (PatchEvent(floweyStep, floweyStepPatch)) patched++;
if (PatchEvent(sansCreate, sansCreatePatch)) patched++;
if (PatchEvent(sansStep, sansStepPatch)) patched++;

ScriptMessage("LAST PETAL compatibility patch finished.\n\nPatched existing Flowey/Sans events: " + patched + "\n\nThis version creates NO new scripts and contains NO EnsureScript helper, so it avoids the compile error you just got.\n\nMake a backup of data.win before saving.");
