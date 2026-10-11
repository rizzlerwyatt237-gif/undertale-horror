using System;
using System.Linq;

// UNDERTALE: FLOWEY // LAST PETAL EXPANSION - COMPATIBLE
// Uses the current UndertaleModTool CodeImportGroup API instead of ReplaceGML.
// Offline/scripted adaptive horror. Keep a backup of data.win.

void Log(string text)
{
    Console.WriteLine("[LAST PETAL] " + text);
}

string FindCodeName(string a, string b)
{
    var code = Data.Code.FirstOrDefault(c => c?.Name?.Content != null &&
        c.Name.Content.IndexOf(a, StringComparison.OrdinalIgnoreCase) >= 0 &&
        c.Name.Content.IndexOf(b, StringComparison.OrdinalIgnoreCase) >= 0);
    return code?.Name?.Content;
}

string Decompile(UndertaleModLib.Models.UndertaleCode code)
{
    return GetDecompiledText(code);
}

bool ImportCode(UndertaleModLib.Models.UndertaleCode code, string gml)
{
    var group = new UndertaleModLib.Compiler.CodeImportGroup(Data)
    {
        MainThreadAction = MainThreadAction
    };
    group.QueueReplace(code, gml);
    var result = group.Import();
    if (!result.Successful)
    {
        ScriptMessage("GML import failed:\n" + result.PrintAllErrors(false));
        return false;
    }
    return true;
}

void Prepend(UndertaleModLib.Models.UndertaleCode code, string injected)
{
    if (code == null) return;
    string original = Decompile(code);
    if (original.Contains("LAST PETAL COMPAT HOOK")) return;
    ImportCode(code, "// LAST PETAL COMPAT HOOK\n" + injected + "\n\n" + original);
}

UndertaleModLib.Models.UndertaleCode EnsureScript(string name, string gml)
{
    var code = Data.Code.FirstOrDefault(c => c?.Name?.Content == name);
    if (code != null) return code;

    code = UndertaleModLib.Models.UndertaleCode.CreateEmptyEntry(Data, name);
    Data.Code.Add(code);
    if (!ImportCode(code, gml)) return null;
    return code;
}

// Adaptive director.
var director = EnsureScript("gml_Script_last_petal_expansion_tick", @"
if (!variable_global_exists(""last_petal_horror"")) global.last_petal_horror = 0;
if (!variable_global_exists(""last_petal_memory"")) global.last_petal_memory = 0;
if (!variable_global_exists(""last_petal_kills"")) global.last_petal_kills = 0;
if (!variable_global_exists(""last_petal_mercy"")) global.last_petal_mercy = 0;
if (!variable_global_exists(""last_petal_phase"")) global.last_petal_phase = 0;
if (!variable_global_exists(""last_petal_tension"")) global.last_petal_tension = 0;
if (!variable_global_exists(""last_petal_fakeout"")) global.last_petal_fakeout = 0;

if (variable_global_exists(""kills"")) global.last_petal_kills = max(global.last_petal_kills, kills);
if (variable_global_exists(""killcount"")) global.last_petal_kills = max(global.last_petal_kills, killcount);
if (global.last_petal_kills > 0) global.last_petal_memory = max(global.last_petal_memory, 2);

global.last_petal_horror = clamp(global.last_petal_memory + global.last_petal_kills, 0, 20);
global.last_petal_phase = 0;
if (global.last_petal_horror >= 5) global.last_petal_phase = 1;
if (global.last_petal_horror >= 10) global.last_petal_phase = 2;
if (global.last_petal_horror >= 15) global.last_petal_phase = 3;
global.last_petal_tension = clamp(global.last_petal_horror * 5, 0, 100);

global.last_petal_fakeout = 0;
if (global.last_petal_phase >= 1 && irandom(179) == 0) global.last_petal_fakeout = 1;
", Data);

if (director == null)
{
    ScriptMessage("Could not create the Last Petal director.");
    return;
}

var phase = EnsureScript("gml_Script_last_petal_phase", @"
if (!variable_global_exists(""last_petal_phase"")) global.last_petal_phase = 0;
return clamp(global.last_petal_phase, 0, 3);
");

var tactic = EnsureScript("gml_Script_last_petal_tactic", @"
if (!variable_global_exists(""last_petal_phase"")) global.last_petal_phase = 0;
if (global.last_petal_phase <= 0) return ""observe"";
if (global.last_petal_phase == 1) return choose(""observe"", ""fakeout"", ""pressure"");
if (global.last_petal_phase == 2) return choose(""fakeout"", ""pressure"", ""relentless"");
return choose(""pressure"", ""relentless"", ""ambush"");
");

var pressure = EnsureScript("gml_Script_last_petal_encounter_pressure", @"
if (!variable_global_exists(""last_petal_horror"")) global.last_petal_horror = 0;
return clamp(global.last_petal_horror, 0, 20);
");

// Flowey remembers and reacts, but does not become the attacker.
string floweyCreate = FindCodeName("flowey", "Create_0");
string floweyStep = FindCodeName("flowey", "Step_0");

if (floweyCreate != null)
{
    Prepend(Data.Code.ByName(floweyCreate), @"
if (!variable_global_exists(""last_petal_memory"")) global.last_petal_memory = 0;
global.last_petal_memory = max(global.last_petal_memory, 1);
global.last_petal_flowey_knows = true;
last_petal_expansion_tick();
");
    Log("Flowey memory installed.");
}

if (floweyStep != null)
{
    Prepend(Data.Code.ByName(floweyStep), @"
last_petal_expansion_tick();
if (global.last_petal_phase >= 1 && irandom(89) == 0) image_xscale = -image_xscale;
if (global.last_petal_phase >= 2 && irandom(59) == 0) image_yscale = -abs(image_yscale);
if (global.last_petal_phase >= 3 && irandom(39) == 0) image_angle += choose(-3, 3, 6, -6);
last_petal_flowey_phase = global.last_petal_phase;
last_petal_flowey_fakeout = global.last_petal_fakeout;
");
    Log("Flowey reaction system installed.");
}
else
{
    Log("Flowey Step event was not found in this Undertale build.");
}

// Sans receives the adaptive state for compatible encounter code.
string sansCreate = FindCodeName("sans", "Create_0");
string sansStep = FindCodeName("sans", "Step_0");

if (sansCreate != null)
{
    Prepend(Data.Code.ByName(sansCreate), @"
last_petal_expansion_tick();
last_petal_sans_memory = global.last_petal_horror;
");
    Log("Sans memory installed.");
}

if (sansStep != null)
{
    Prepend(Data.Code.ByName(sansStep), @"
last_petal_expansion_tick();
last_petal_sans_phase = global.last_petal_phase;
last_petal_sans_tension = global.last_petal_tension;
last_petal_sans_fakeout = global.last_petal_fakeout;
last_petal_sans_tactic = last_petal_tactic();
");
    Log("Sans adaptive state installed.");
}
else
{
    Log("Sans Step event was not found in this Undertale build.");
}

ScriptMessage("FLOWEY // LAST PETAL EXPANSION installed successfully.\n\nThis version uses CodeImportGroup, which is the current UndertaleModTool import API.\n\nAdded adaptive phases, tension, fake-outs, Flowey reactions, and Sans state.\n\nIf another error appears, send me its screenshot and I will fix the script for your exact UMT build.\n");
