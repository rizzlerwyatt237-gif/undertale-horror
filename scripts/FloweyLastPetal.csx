using System;
using System.Linq;

// UNDERTALE: FLOWEY // LAST PETAL
// Run this script in UndertaleModTool with Undertale's data.win loaded.
// The script intentionally edits only resources whose names contain "flowey".

void Log(string text)
{
    Console.WriteLine("[LAST PETAL] " + text);
}

string FindCodeName(string containsA, string containsB)
{
    var code = Data.Code.FirstOrDefault(c =>
        c?.Name?.Content != null &&
        c.Name.Content.IndexOf(containsA, StringComparison.OrdinalIgnoreCase) >= 0 &&
        c.Name.Content.IndexOf(containsB, StringComparison.OrdinalIgnoreCase) >= 0);
    return code?.Name?.Content;
}

string Decompile(UndertaleModLib.Models.UndertaleCode code)
{
    var context = new GlobalDecompileContext(Data);
    return new Underanalyzer.Decompiler.DecompileContext(
        context,
        code,
        Data.ToolInfo.DecompilerSettings).DecompileToString();
}

void Prepend(UndertaleModLib.Models.UndertaleCode code, string injected)
{
    string original = Decompile(code);
    if (original.Contains("LAST PETAL HOOK"))
        return;

    code.ReplaceGML(
        "// LAST PETAL HOOK\n" + injected + "\n\n" + original,
        Data);
}

// Create a reusable horror controller script.
var controllerName = "gml_Script_flowey_last_petal_controller";
var controller = Data.Code.FirstOrDefault(c => c?.Name?.Content == controllerName);
if (controller == null)
{
    controller = UndertaleModLib.Models.UndertaleCode.CreateEmptyEntry(Data, controllerName);
    controller.ReplaceGML(@"
// FLOWEY // LAST PETAL
// This controller deliberately uses ordinary GML so it can run on the original game.

if (!variable_global_exists("last_petal_horror"))
    global.last_petal_horror = 0;

if (!variable_instance_exists(id, "last_petal_timer"))
    last_petal_timer = 0;

if (!variable_instance_exists(id, "last_petal_original_xscale"))
    last_petal_original_xscale = image_xscale;

if (!variable_instance_exists(id, "last_petal_original_yscale"))
    last_petal_original_yscale = image_yscale;

global.last_petal_horror = min(global.last_petal_horror + 1, 12);
last_petal_timer = max(last_petal_timer, room_speed * 2);

// The longer Flowey is around, the less stable he looks.
if (global.last_petal_horror >= 3)
{
    image_angle = choose(0, 0, 0, 180);
    image_xscale = last_petal_original_xscale * choose(1, 1, -1);
    image_yscale = last_petal_original_yscale * choose(1, 1, -1);
}

if (global.last_petal_horror >= 6)
{
    image_alpha = choose(1, 1, 0.82, 0.55);
    image_blend = choose(c_white, c_white, c_red);
}
else
{
    image_alpha = 1;
    image_blend = c_white;
}

if (global.last_petal_horror >= 9 && irandom(90) == 0)
{
    // A rare visual glitch: Flowey freezes for a moment instead of animating normally.
    image_speed = 0;
}
else
{
    image_speed = max(image_speed, 0.1);
}
", Data);
    Data.Code.Add(controller);
    Log("Created " + controllerName);
}

// Locate Flowey's Create and Step events without depending on one exact Undertale build.
string createName = FindCodeName("flowey", "Create_0");
string stepName = FindCodeName("flowey", "Step_0");

if (createName != null)
{
    var createCode = Data.Code.ByName(createName);
    Prepend(createCode, "global.last_petal_horror = 0;\nflowey_last_petal_controller();");
    Log("Patched Flowey Create: " + createName);
}
else
{
    Log("WARNING: no Flowey Create event was found. No Create event was changed.");
}

if (stepName != null)
{
    var stepCode = Data.Code.ByName(stepName);
    Prepend(stepCode, "flowey_last_petal_controller();");
    Log("Patched Flowey Step: " + stepName);
}
else
{
    Log("WARNING: no Flowey Step event was found. No Step event was changed.");
}

// Add a global initialization entry for the controller script if needed.
if (Data.GlobalInitScripts != null && !Data.GlobalInitScripts.Any(x => x?.Code?.Name?.Content == controllerName))
{
    Data.GlobalInitScripts.Add(new UndertaleModLib.Models.UndertaleGlobalInit
    {
        Code = controller
    });
    Log("Registered global init for the controller.");
}

ScriptMessage("FLOWEY // LAST PETAL installed.\n\nFlowey hooks: Create=" + (createName != null ? "FOUND" : "NOT FOUND") + ", Step=" + (stepName != null ? "FOUND" : "NOT FOUND") + "\n\nSave this as a new data.win and keep your original backup.");
