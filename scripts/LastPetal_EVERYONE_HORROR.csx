using System;
using System.Linq;
using System.Collections.Generic;
using UndertaleModLib.Models;
using UndertaleModLib.Compiler;

// LAST PETAL // EVERYONE HORROR
// Compatibility-first: patches existing Create/Step events only.
// Non-graphic horror presentation and reaction states.

EnsureDataLoaded();

UndertaleCode FindEvent(string objectPart, string eventPart)
{
    return Data.Code.FirstOrDefault(c => c?.Name?.Content != null &&
        c.Name.Content.IndexOf(objectPart, StringComparison.OrdinalIgnoreCase) >= 0 &&
        c.Name.Content.IndexOf(eventPart, StringComparison.OrdinalIgnoreCase) >= 0);
}

List<UndertaleCode> FindAllEvents(string objectPart, string eventPart)
{
    return Data.Code.Where(c => c?.Name?.Content != null &&
        c.Name.Content.IndexOf(objectPart, StringComparison.OrdinalIgnoreCase) >= 0 &&
        c.Name.Content.IndexOf(eventPart, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
}

bool Patch(UndertaleCode code, string gml)
{
    if (code == null) return false;
    string old = GetDecompiledText(code);
    if (old.Contains("LAST PETAL EVERYONE HORROR HOOK")) return true;

    var imports = new CodeImportGroup(Data)
    {
        MainThreadAction = MainThreadAction
    };
    imports.QueueReplace(code,
        "// LAST PETAL EVERYONE HORROR HOOK\n" + gml + "\n\n" + old);
    var result = imports.Import();
    if (!result.Successful)
    {
        ScriptError("Could not patch " + code.Name.Content + ":\n" + result.PrintAllErrors(false));
        return false;
    }
    return true;
}

// Horror is deliberately active even on a fresh save so the mod is actually visible.
string init = @"
if (!variable_global_exists(""last_petal_kills"")) global.last_petal_kills = 0;
if (!variable_global_exists(""last_petal_fear"")) global.last_petal_fear = 100;
if (!variable_global_exists(""last_petal_hatred"")) global.last_petal_hatred = 100;
if (!variable_global_exists(""last_petal_horror"")) global.last_petal_horror = 100;
if (!variable_global_exists(""last_petal_phase"")) global.last_petal_phase = 4;
if (!variable_global_exists(""last_petal_route"")) global.last_petal_route = ""horror"";

global.last_petal_fear = 100;
global.last_petal_hatred = 100;
global.last_petal_horror = 100;
global.last_petal_phase = 4;
global.last_petal_route = ""horror"";

// Character reaction flags.
global.last_petal_everyone_afraid = true;
global.last_petal_everyone_hostile = true;
global.last_petal_everyone_horrified = true;
global.last_petal_flowey_scared = true;
global.last_petal_flowey_hates_player = false;
global.last_petal_sans_hostile = true;
global.last_petal_sans_no_mercy = true;
";

string step = @"
if (!variable_global_exists(""last_petal_phase"")) global.last_petal_phase = 4;
if (!variable_global_exists(""last_petal_fear"")) global.last_petal_fear = 100;
if (!variable_global_exists(""last_petal_hatred"")) global.last_petal_hatred = 100;

last_petal_fear = global.last_petal_fear;
last_petal_hatred = global.last_petal_hatred;
last_petal_reaction = ""horrified"";

// Small, unmistakable horror effects rather than gore.
if (irandom(90) == 0)
    image_xscale = -image_xscale;

if (irandom(120) == 0)
    image_alpha = 0.55;
else
    image_alpha = 1;

if (irandom(180) == 0)
    image_angle += choose(-4, 4, -8, 8);

// Persistent state that other existing game code can read.
last_petal_afraid = true;
last_petal_hostile = true;
last_petal_horrified = true;
";

string floweyStep = step + @"
// Flowey is scared of the player, not the hunter.
global.last_petal_flowey_scared = true;
global.last_petal_flowey_hates_player = false;
last_petal_flowey_reaction = ""terrified"";
";

string sansStep = step + @"
// Sans is permanently hostile in this horror version.
global.last_petal_sans_hostile = true;
global.last_petal_sans_no_mercy = true;
last_petal_sans_reaction = ""hostile"";
last_petal_sans_pressure = choose(""watching"", ""warning"", ""relentless"", ""ambush"");
";

var names = new[] {
    "toriel", "papyrus", "undyne", "asgore", "mettaton", "alphys",
    "monsterkid", "monster_kid", "napstablook", "temmie", "doggo",
    "flowey", "sans"
};

int patched = 0;
var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

foreach (var name in names)
{
    foreach (var create in FindAllEvents(name, "Create_0"))
    {
        if (!seen.Add(create.Name.Content)) continue;
        if (Patch(create, init)) patched++;
    }

    foreach (var ev in FindAllEvents(name, "Step_0"))
    {
        if (!seen.Add(ev.Name.Content)) continue;
        string code = step;
        if (name == "flowey") code = floweyStep;
        if (name == "sans") code = sansStep;
        if (Patch(ev, code)) patched++;
    }
}

// If a character has no Create event, its Step still gets the state initialized.
foreach (var name in names)
{
    foreach (var ev in FindAllEvents(name, "Step_0"))
    {
        if (ev == null) continue;
    }
}

ScriptMessage(
    "LAST PETAL EVERYONE HORROR finished.\n\n" +
    "Patched existing character events: " + patched + "\n\n" +
    "Everyone targeted by this pass is flagged as afraid, hostile, and horrified.\n" +
    "Flowey is scared of the player.\n" +
    "Sans is permanently hostile/no-mercy in the mod state.\n" +
    "Characters also get visible non-graphic fake-outs.\n\n" +
    "This script creates NO new scripts.\n" +
    "BACK UP data.win before saving."
);
