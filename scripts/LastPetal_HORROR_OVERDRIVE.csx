using System;
using System.Linq;
using System.Collections.Generic;
using UndertaleModLib.Models;
using UndertaleModLib.Compiler;

// FLOWEY // LAST PETAL HORROR OVERDRIVE
// Compatibility-first: patches existing events only.
// Non-graphic horror: fear, hostility, fake-outs, distorted presentation.

EnsureDataLoaded();

void HorrorLog(string text)
{
    Console.WriteLine("[LAST PETAL HORROR] " + text);
}

UndertaleCode FindHorrorEvent(string objectPart, string eventPart)
{
    return Data.Code.FirstOrDefault(c => c?.Name?.Content != null &&
        c.Name.Content.IndexOf(objectPart, StringComparison.OrdinalIgnoreCase) >= 0 &&
        c.Name.Content.IndexOf(eventPart, StringComparison.OrdinalIgnoreCase) >= 0);
}

bool PatchHorrorEvent(UndertaleCode code, string injected)
{
    if (code == null)
        return false;

    string original = GetDecompiledText(code);
    if (original.Contains("LAST PETAL HORROR OVERDRIVE HOOK"))
    {
        HorrorLog("Already patched: " + code.Name.Content);
        return true;
    }

    var imports = new CodeImportGroup(Data)
    {
        MainThreadAction = MainThreadAction
    };

    imports.QueueReplace(code,
        "// LAST PETAL HORROR OVERDRIVE HOOK\n" + injected + "\n\n" + original);

    var result = imports.Import();
    if (!result.Successful)
    {
        ScriptError("Could not patch " + code.Name.Content + ":\n" +
            result.PrintAllErrors(false));
        return false;
    }

    HorrorLog("Patched: " + code.Name.Content);
    return true;
}

string sharedCreate = @"
if (!variable_global_exists(""last_petal_memory"")) global.last_petal_memory = 0;
if (!variable_global_exists(""last_petal_kills"")) global.last_petal_kills = 0;
if (!variable_global_exists(""last_petal_horror"")) global.last_petal_horror = 0;
if (!variable_global_exists(""last_petal_phase"")) global.last_petal_phase = 0;
if (!variable_global_exists(""last_petal_tension"")) global.last_petal_tension = 0;
if (!variable_global_exists(""last_petal_route"")) global.last_petal_route = ""mercy"";
if (!variable_global_exists(""last_petal_fear"")) global.last_petal_fear = 0;
if (!variable_global_exists(""last_petal_hatred"")) global.last_petal_hatred = 0;

if (variable_global_exists(""kills""))
    global.last_petal_kills = max(global.last_petal_kills, kills);
if (variable_global_exists(""killcount""))
    global.last_petal_kills = max(global.last_petal_kills, killcount);

global.last_petal_horror = clamp(global.last_petal_memory + global.last_petal_kills, 0, 50);
global.last_petal_phase = 0;
if (global.last_petal_kills >= 1) global.last_petal_phase = 1;
if (global.last_petal_kills >= 5) global.last_petal_phase = 2;
if (global.last_petal_kills >= 15) global.last_petal_phase = 3;
if (global.last_petal_kills >= 30) global.last_petal_phase = 4;

global.last_petal_route = ""mercy"";
if (global.last_petal_kills >= 1) global.last_petal_route = ""violent"";
if (global.last_petal_kills >= 15) global.last_petal_route = ""genocide"";

global.last_petal_fear = clamp(global.last_petal_kills * 4, 0, 100);
global.last_petal_hatred = clamp(global.last_petal_kills * 5, 0, 100);
";

string floweyCreate = sharedCreate + @"
global.last_petal_flowey_scared = (global.last_petal_kills >= 1);
global.last_petal_flowey_hates_player = false;
";

string floweyStep = @"
if (!variable_global_exists(""last_petal_kills"")) global.last_petal_kills = 0;
if (!variable_global_exists(""last_petal_phase"")) global.last_petal_phase = 0;
if (!variable_global_exists(""last_petal_tension"")) global.last_petal_tension = 0;

global.last_petal_flowey_scared = (global.last_petal_kills >= 1);
global.last_petal_flowey_hates_player = false;

// Flowey is frightened rather than the hunter.
if (global.last_petal_kills >= 1 && irandom(179) == 0)
    global.last_petal_flowey_fakeout = 1;
else
    global.last_petal_flowey_fakeout = 0;

if (global.last_petal_kills >= 5 && irandom(120) == 0)
    image_xscale = -image_xscale;

if (global.last_petal_kills >= 15 && irandom(150) == 0)
    image_yscale = -abs(image_yscale);

if (global.last_petal_kills >= 30 && irandom(180) == 0)
    image_angle += choose(-4, 4, -8, 8);
";

string sansCreate = sharedCreate + @"
global.last_petal_sans_hostile = (global.last_petal_kills >= 1);
global.last_petal_sans_no_mercy = (global.last_petal_kills >= 15);
";

string sansStep = @"
if (!variable_global_exists(""last_petal_kills"")) global.last_petal_kills = 0;
if (!variable_global_exists(""last_petal_phase"")) global.last_petal_phase = 0;

last_petal_sans_hostile = (global.last_petal_kills >= 1);
last_petal_sans_no_mercy = (global.last_petal_kills >= 15);
last_petal_sans_pressure = ""watching"";

if (global.last_petal_kills >= 1)
    last_petal_sans_pressure = choose(""watching"", ""warning"", ""hostile"");
if (global.last_petal_kills >= 15)
    last_petal_sans_pressure = choose(""hostile"", ""relentless"", ""no_mercy"");

if (global.last_petal_kills >= 15 && irandom(160) == 0)
    image_xscale = -image_xscale;

if (global.last_petal_kills >= 30 && irandom(180) == 0)
    image_angle += choose(-3, 3, -6, 6);
";

string genericCreate = @"
if (!variable_global_exists(""last_petal_kills"")) global.last_petal_kills = 0;
if (!variable_global_exists(""last_petal_fear"")) global.last_petal_fear = 0;
if (!variable_global_exists(""last_petal_hatred"")) global.last_petal_hatred = 0;

last_petal_fear = global.last_petal_fear;
last_petal_hatred = global.last_petal_hatred;
last_petal_reaction = ""normal"";

if (global.last_petal_kills >= 1)
    last_petal_reaction = ""afraid"";
if (global.last_petal_kills >= 5)
    last_petal_reaction = ""hostile"";
if (global.last_petal_kills >= 15)
    last_petal_reaction = ""horrified"";
";

string genericStep = @"
if (!variable_global_exists(""last_petal_kills"")) global.last_petal_kills = 0;
if (!variable_global_exists(""last_petal_phase"")) global.last_petal_phase = 0;

last_petal_reaction = ""normal"";

if (global.last_petal_kills >= 1)
    last_petal_reaction = ""afraid"";
if (global.last_petal_kills >= 5)
    last_petal_reaction = ""hostile"";
if (global.last_petal_kills >= 15)
    last_petal_reaction = ""horrified"";

if (global.last_petal_phase >= 2 && irandom(220) == 0)
    image_alpha = 0.65;
else
    image_alpha = 1;

if (global.last_petal_phase >= 3 && irandom(240) == 0)
    image_xscale = -image_xscale;
";

var characterNames = new[] {
    "toriel", "papyrus", "undyne", "asgore", "mettaton",
    "alphys", "monsterkid", "napstablook", "temmie", "doggo",
    "flowey", "sans"
};

int patched = 0;
var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

foreach (var name in characterNames)
{
    var create = FindHorrorEvent(name, "Create_0");
    var step = FindHorrorEvent(name, "Step_0");

    if (create != null && seen.Add(create.Name.Content))
    {
        string patch = genericCreate;
        if (name == "flowey") patch = floweyCreate;
        if (name == "sans") patch = sansCreate;
        if (PatchHorrorEvent(create, patch)) patched++;
    }

    if (step != null && seen.Add(step.Name.Content))
    {
        string patch = genericStep;
        if (name == "flowey") patch = floweyStep;
        if (name == "sans") patch = sansStep;
        if (PatchHorrorEvent(step, patch)) patched++;
    }
}

ScriptMessage(
    "LAST PETAL HORROR OVERDRIVE finished.\n\n" +
    "Patched existing character events: " + patched + "\n\n" +
    "Route behavior:\n" +
    "- Mercy: characters mostly normal, with subtle unease.\n" +
    "- Violent: characters become afraid/hostile.\n" +
    "- Genocide: Sans becomes extremely hostile; Flowey becomes frightened.\n" +
    "- Other characters get fear/hostility/horror reactions.\n\n" +
    "No graphic gore is added, and no new scripts are created.\n\n" +
    "BACK UP data.win before saving."
);
