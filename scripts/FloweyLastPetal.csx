using System;
using System.Linq;

// UNDERTALE: FLOWEY // LAST PETAL
// Adaptive horror patch. Flowey remembers the player's behavior through persistent
// global state. Sans and other encounters can be wired into the same director.
// This is game-AI style behavior, not a cloud LLM: it is deterministic and runs offline.

void Log(string text) => Console.WriteLine("[LAST PETAL] " + text);

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
    var original = Decompile(code);
    if (original.Contains("LAST PETAL HOOK")) return;
    code.ReplaceGML("// LAST PETAL HOOK\n" + injected + "\n\n" + original, Data);
}

// Shared adaptive director. It remembers what the player has done when the
// base game exposes the corresponding globals, while remaining safe if a build
// does not contain one of them.
var directorName = "gml_Script_last_petal_ai_director";
var director = Data.Code.FirstOrDefault(c => c?.Name?.Content == directorName);
if (director == null)
{
    director = UndertaleModLib.Models.UndertaleCode.CreateEmptyEntry(Data, directorName);
    director.ReplaceGML(@"
// LAST PETAL AI DIRECTOR
if (!variable_global_exists("last_petal_memory")) global.last_petal_memory = 0;
if (!variable_global_exists("last_petal_kills")) global.last_petal_kills = 0;
if (!variable_global_exists("last_petal_resets")) global.last_petal_resets = 0;
if (!variable_global_exists("last_petal_mercy")) global.last_petal_mercy = 0;
if (!variable_global_exists("last_petal_horror")) global.last_petal_horror = 0;

// Observe common Undertale-style state when available.
if (variable_global_exists("kills")) global.last_petal_kills = max(global.last_petal_kills, kills);
if (variable_global_exists("killcount")) global.last_petal_kills = max(global.last_petal_kills, killcount);
if (variable_global_exists("genocide")) && genocide global.last_petal_memory = max(global.last_petal_memory, 8);

// The director escalates based on behavior, not a chosen route.
global.last_petal_horror = clamp(global.last_petal_memory + global.last_petal_kills + global.last_petal_resets, 0, 20);

// Flowey is the observer. He does not become the executioner.
global.last_petal_flowey_knows = global.last_petal_horror >= 1;

// Monster aggression level. Other encounter scripts can read this value.
global.last_petal_aggression = clamp(global.last_petal_horror, 0, 20);

// Non-graphic horror cue for attacks: harder patterns, less warning, and pursuit.
global.last_petal_hunt = global.last_petal_aggression >= 5;
", Data);
    Data.Code.Add(director);
    Log("Created adaptive AI director.");
}

// Flowey: knows what happened, but is explicitly excluded from the ripping/hunting behavior.
string floweyCreate = FindCodeName("flowey", "Create_0");
string floweyStep = FindCodeName("flowey", "Step_0");
if (floweyCreate != null)
{
    Prepend(Data.Code.ByName(floweyCreate), @"
if (!variable_global_exists("last_petal_horror")) global.last_petal_horror = 0;
if (!variable_global_exists("last_petal_memory")) global.last_petal_memory = 0;
global.last_petal_memory = max(global.last_petal_memory, 1);
global.last_petal_flowey_knows = true;
last_petal_original_xscale = image_xscale;
last_petal_original_yscale = image_yscale;
last_petal_ai_director();
");
    Log("Flowey memory hook installed.");
}
if (floweyStep != null)
{
    Prepend(Data.Code.ByName(floweyStep), @"
last_petal_ai_director();
if (global.last_petal_horror >= 3) image_angle = choose(0, 0, 0, 180);
if (global.last_petal_horror >= 6) { image_alpha = choose(1, 1, 0.82, 0.55); image_blend = choose(c_white, c_white, c_red); }
else { image_alpha = 1; image_blend = c_white; }
");
    Log("Flowey horror escalation installed.");
}

// Sans and other hostile encounters can use these globals to become increasingly
// aggressive. We intentionally keep the presentation non-graphic: attacks can
// feel like a relentless hunt without depicting gore.
var sansCreate = FindCodeName("sans", "Create_0");
var sansStep = FindCodeName("sans", "Step_0");
if (sansCreate != null)
{
    Prepend(Data.Code.ByName(sansCreate), @"
last_petal_ai_director();
// Sans remembers the player's history through the shared director.
last_petal_sans_memory = global.last_petal_horror;
");
    Log("Sans memory hook installed.");
}
if (sansStep != null)
{
    Prepend(Data.Code.ByName(sansStep), @"
last_petal_ai_director();
// Adaptive hunt cue. Battle logic can use this to shorten telegraphs and intensify patterns.
if (global.last_petal_hunt) last_petal_sans_pressure = min(global.last_petal_aggression, 20);
");
    Log("Sans adaptive pressure hook installed.");
}

ScriptMessage("FLOWEY // LAST PETAL AI installed.\n\nFlowey remembers. He knows what you did, but he is NOT the executioner.\n\nThe shared director exposes last_petal_aggression and last_petal_hunt so Sans/monster encounter code can escalate attacks based on the player's history.\n\nThis is offline scripted AI behavior, not an external chatbot. Keep your original data.win backup.");
