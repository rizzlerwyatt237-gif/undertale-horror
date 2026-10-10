using System;
using System.Linq;

// UNDERTALE: FLOWEY // LAST PETAL
// One-run adaptive horror installer for UndertaleModTool.
// Flowey remembers. Sans and other encounters can read shared horror state.
// This is offline scripted game AI, not a cloud LLM.

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
    var ctx = new GlobalDecompileContext(Data);
    return new Underanalyzer.Decompiler.DecompileContext(ctx, code, Data.ToolInfo.DecompilerSettings).DecompileToString();
}

void Prepend(UndertaleModLib.Models.UndertaleCode code, string injected)
{
    if (code == null) return;
    var original = Decompile(code);
    if (original.Contains("LAST PETAL HOOK")) return;
    code.ReplaceGML("// LAST PETAL HOOK\n" + injected + "\n\n" + original, Data);
}

var directorName = "gml_Script_last_petal_ai_director";
var director = Data.Code.FirstOrDefault(c => c?.Name?.Content == directorName);

if (director == null)
{
    director = UndertaleModLib.Models.UndertaleCode.CreateEmptyEntry(Data, directorName);
    director.ReplaceGML(@"
// LAST PETAL AI DIRECTOR
if (!variable_global_exists(""last_petal_memory"")) global.last_petal_memory = 0;
if (!variable_global_exists(""last_petal_kills"")) global.last_petal_kills = 0;
if (!variable_global_exists(""last_petal_resets"")) global.last_petal_resets = 0;
if (!variable_global_exists(""last_petal_mercy"")) global.last_petal_mercy = 0;
if (!variable_global_exists(""last_petal_horror"")) global.last_petal_horror = 0;
if (!variable_global_exists(""last_petal_aggression"")) global.last_petal_aggression = 0;
if (!variable_global_exists(""last_petal_hunt"")) global.last_petal_hunt = false;

if (variable_global_exists(""kills"")) global.last_petal_kills = max(global.last_petal_kills, kills);
if (variable_global_exists(""killcount"")) global.last_petal_kills = max(global.last_petal_kills, killcount);
if (variable_global_exists(""genocide"")) {
    if (genocide) global.last_petal_memory = max(global.last_petal_memory, 8);
}

global.last_petal_horror = clamp(global.last_petal_memory + global.last_petal_kills + global.last_petal_resets, 0, 20);
global.last_petal_flowey_knows = global.last_petal_horror >= 1;
global.last_petal_aggression = clamp(global.last_petal_horror, 0, 20);
global.last_petal_hunt = global.last_petal_aggression >= 5;
", Data);
    Data.Code.Add(director);
    Log("Created adaptive AI director.");
}

// Flowey remembers but is NOT an executioner.
string floweyCreate = FindCodeName("flowey", "Create_0");
string floweyStep = FindCodeName("flowey", "Step_0");

if (floweyCreate != null)
{
    Prepend(Data.Code.ByName(floweyCreate), @"
if (!variable_global_exists(""last_petal_horror"")) global.last_petal_horror = 0;
if (!variable_global_exists(""last_petal_memory"")) global.last_petal_memory = 0;
global.last_petal_memory = max(global.last_petal_memory, 1);
global.last_petal_flowey_knows = true;
last_petal_ai_director();
");
    Log("Flowey memory hook installed.");
}

if (floweyStep != null)
{
    Prepend(Data.Code.ByName(floweyStep), @"
last_petal_ai_director();
if (global.last_petal_horror >= 3) image_angle = choose(0, 0, 0, 180);
if (global.last_petal_horror >= 6) {
    image_alpha = choose(1, 1, 0.82, 0.55);
    image_blend = choose(c_white, c_white, c_red);
} else {
    image_alpha = 1;
    image_blend = c_white;
}
");
    Log("Flowey horror escalation installed.");
}

// Sans gets the shared memory and a pressure value. This is deliberately non-graphic:
// the director exposes aggression/hunt state for encounter code to use.
string sansCreate = FindCodeName("sans", "Create_0");
string sansStep = FindCodeName("sans", "Step_0");

if (sansCreate != null)
{
    Prepend(Data.Code.ByName(sansCreate), @"
last_petal_ai_director();
last_petal_sans_memory = global.last_petal_horror;
");
    Log("Sans memory hook installed.");
}

if (sansStep != null)
{
    Prepend(Data.Code.ByName(sansStep), @"
last_petal_ai_director();
last_petal_sans_pressure = 0;
if (global.last_petal_hunt) last_petal_sans_pressure = min(global.last_petal_aggression, 20);
");
    Log("Sans adaptive pressure hook installed.");
}

// Generic encounter helper. Other battle code can call this script to obtain a
// normalized 0..20 aggression level without making Flowey an attacker.
var pressureName = "gml_Script_last_petal_encounter_pressure";
var pressure = Data.Code.FirstOrDefault(c => c?.Name?.Content == pressureName);
if (pressure == null)
{
    pressure = UndertaleModLib.Models.UndertaleCode.CreateEmptyEntry(Data, pressureName);
    pressure.ReplaceGML(@"
// LAST PETAL ENCOUNTER PRESSURE
if (!variable_global_exists(""last_petal_aggression"")) global.last_petal_aggression = 0;
return clamp(global.last_petal_aggression, 0, 20);
", Data);
    Data.Code.Add(pressure);
    Log("Created generic encounter-pressure helper.");
}

ScriptMessage("FLOWEY // LAST PETAL installed.\n\nFlowey remembers what happened, but he is NOT the executioner.\n\nSans receives the shared horror state, and a generic encounter-pressure helper is installed for compatible monster/battle code.\n\nKeep your original data.win backup. If your UndertaleModTool build reports that ReplaceGML is missing, use an older compatible UndertaleModTool build or the tool's ImportGML workflow.\n");
