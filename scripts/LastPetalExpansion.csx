using System;
using System.Linq;

// UNDERTALE: FLOWEY // LAST PETAL EXPANSION
// Extra adaptive-horror systems for the Last Petal mod.
// These systems are offline/scripted: no cloud AI is required.
// Keep a backup of data.win before running any mod script.

void Log(string text)
{
    Console.WriteLine("[LAST PETAL EXPANSION] " + text);
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
    if (original.Contains("LAST PETAL EXPANSION HOOK")) return;
    code.ReplaceGML("// LAST PETAL EXPANSION HOOK\n" + injected + "\n\n" + original, Data);
}

// -----------------------------------------------------------------------------
// 1. Adaptive phase director
// -----------------------------------------------------------------------------
var directorName = "gml_Script_last_petal_expansion_tick";
var director = Data.Code.FirstOrDefault(c => c?.Name?.Content == directorName);

if (director == null)
{
    director = UndertaleModLib.Models.UndertaleCode.CreateEmptyEntry(Data, directorName);
    director.ReplaceGML(@"
// LAST PETAL EXPANSION TICK
if (!variable_global_exists(""last_petal_horror"")) global.last_petal_horror = 0;
if (!variable_global_exists(""last_petal_memory"")) global.last_petal_memory = 0;
if (!variable_global_exists(""last_petal_kills"")) global.last_petal_kills = 0;
if (!variable_global_exists(""last_petal_mercy"")) global.last_petal_mercy = 0;
if (!variable_global_exists(""last_petal_phase"")) global.last_petal_phase = 0;
if (!variable_global_exists(""last_petal_tension"")) global.last_petal_tension = 0;
if (!variable_global_exists(""last_petal_fakeout"")) global.last_petal_fakeout = 0;
if (!variable_global_exists(""last_petal_last_room"")) global.last_petal_last_room = -1;

if (variable_global_exists(""kills"")) global.last_petal_kills = max(global.last_petal_kills, kills);
if (variable_global_exists(""killcount"")) global.last_petal_kills = max(global.last_petal_kills, killcount);
if (variable_global_exists(""mercy"")) global.last_petal_mercy = max(global.last_petal_mercy, mercy);
if (variable_global_exists(""spare"")) global.last_petal_mercy = max(global.last_petal_mercy, spare);

if (global.last_petal_kills > 0) global.last_petal_memory = max(global.last_petal_memory, 2);
if (global.last_petal_mercy > 0) global.last_petal_memory = max(global.last_petal_memory, 1);

global.last_petal_horror = clamp(global.last_petal_memory + global.last_petal_kills, 0, 20);
global.last_petal_phase = 0;
if (global.last_petal_horror >= 5) global.last_petal_phase = 1;
if (global.last_petal_horror >= 10) global.last_petal_phase = 2;
if (global.last_petal_horror >= 15) global.last_petal_phase = 3;

global.last_petal_tension = clamp(global.last_petal_horror * 5, 0, 100);

// Fake-outs are a state value that encounter code can use for harmless scares,
// pauses, dialogue changes, or sudden visual/audio events.
if (global.last_petal_phase >= 1 && irandom(179) == 0) {
    global.last_petal_fakeout = 1;
} else {
    global.last_petal_fakeout = 0;
}
", Data);
    Data.Code.Add(director);
    Log("Created adaptive phase director.");
}

// -----------------------------------------------------------------------------
// 2. Helper: current horror phase (0..3)
// -----------------------------------------------------------------------------
var phaseName = "gml_Script_last_petal_phase";
var phase = Data.Code.FirstOrDefault(c => c?.Name?.Content == phaseName);
if (phase == null)
{
    phase = UndertaleModLib.Models.UndertaleCode.CreateEmptyEntry(Data, phaseName);
    phase.ReplaceGML(@"
if (!variable_global_exists(""last_petal_phase"")) global.last_petal_phase = 0;
return clamp(global.last_petal_phase, 0, 3);
", Data);
    Data.Code.Add(phase);
    Log("Created phase helper.");
}

// -----------------------------------------------------------------------------
// 3. Helper: adaptive tactic selector
// Returns a small text label that battle code can use to choose behavior.
// -----------------------------------------------------------------------------
var tacticName = "gml_Script_last_petal_tactic";
var tactic = Data.Code.FirstOrDefault(c => c?.Name?.Content == tacticName);
if (tactic == null)
{
    tactic = UndertaleModLib.Models.UndertaleCode.CreateEmptyEntry(Data, tacticName);
    tactic.ReplaceGML(@"
if (!variable_global_exists(""last_petal_phase"")) global.last_petal_phase = 0;
if (global.last_petal_phase <= 0) return ""observe"";
if (global.last_petal_phase == 1) return choose(""observe"", ""fakeout"", ""pressure"");
if (global.last_petal_phase == 2) return choose(""fakeout"", ""pressure"", ""relentless"");
return choose(""pressure"", ""relentless"", ""ambush"");
", Data);
    Data.Code.Add(tactic);
    Log("Created adaptive tactic helper.");
}

// -----------------------------------------------------------------------------
// 4. Flowey gets more reactive visual behavior, but never becomes the attacker.
// -----------------------------------------------------------------------------
string floweyStep = FindCodeName("flowey", "Step_0");
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
    Log("Expanded Flowey reaction behavior.");
}

// -----------------------------------------------------------------------------
// 5. Sans gets a stronger adaptive pressure signal for battle code.
// -----------------------------------------------------------------------------
string sansStep = FindCodeName("sans", "Step_0");
if (sansStep != null)
{
    Prepend(Data.Code.ByName(sansStep), @"
last_petal_expansion_tick();
last_petal_sans_phase = global.last_petal_phase;
last_petal_sans_tension = global.last_petal_tension;
last_petal_sans_fakeout = global.last_petal_fakeout;
last_petal_sans_tactic = last_petal_tactic();
");
    Log("Expanded Sans adaptive state.");
}

// -----------------------------------------------------------------------------
// 6. Generic encounter helper for any monster/battle script.
// -----------------------------------------------------------------------------
var stateName = "gml_Script_last_petal_state";
var state = Data.Code.FirstOrDefault(c => c?.Name?.Content == stateName);
if (state == null)
{
    state = UndertaleModLib.Models.UndertaleCode.CreateEmptyEntry(Data, stateName);
    state.ReplaceGML(@"
last_petal_expansion_tick();
return {
    phase: global.last_petal_phase,
    tension: global.last_petal_tension,
    fakeout: global.last_petal_fakeout,
    horror: global.last_petal_horror,
    tactic: last_petal_tactic()
};
", Data);
    Data.Code.Add(state);
    Log("Created shared encounter-state helper.");
}

ScriptMessage("FLOWEY // LAST PETAL EXPANSION installed.\n\nAdded:\n- 4 adaptive horror phases\n- tension meter\n- random fake-out state\n- adaptive tactic selector\n- stronger Flowey reactions\n- stronger Sans pressure state\n- shared encounter-state helper\n\nThese are non-graphic horror systems; battle code can use the shared state to vary encounters.\n\nKeep your original data.win backup.\n");
