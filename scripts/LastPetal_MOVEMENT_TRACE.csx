using System;
using System.Collections.Generic;
using System.Linq;
using UndertaleModLib.Models;

// LAST PETAL MOVEMENT TRACE
// READ-ONLY diagnostic. THIS SCRIPT MAKES NO CHANGES TO data.win.
// It searches Step/BeginStep/EndStep and keyboard events for movement-related
// code and Last Petal markers so we can identify the exact event causing the
// player to continuously move upward.

EnsureDataLoaded();

var needles = new[] {
    "vk_up", "keyboard_check", "keyboard_check_pressed", "keyboard_check_direct",
    "vspeed", "hspeed", "direction", "move_towards_point", "y +=", "y -=",
    "x +=", "x -=", "motion_set", "place_meeting",
    "LAST PETAL", "last_petal"
};

var hits = new List<string>();

foreach (var code in Data.Code.Where(c => c?.Name?.Content != null))
{
    string name = code.Name.Content;
    bool movementEvent =
        name.IndexOf("_Step_", StringComparison.OrdinalIgnoreCase) >= 0 ||
        name.IndexOf("_BeginStep_", StringComparison.OrdinalIgnoreCase) >= 0 ||
        name.IndexOf("_EndStep_", StringComparison.OrdinalIgnoreCase) >= 0 ||
        name.IndexOf("_KeyPress_", StringComparison.OrdinalIgnoreCase) >= 0 ||
        name.IndexOf("_Keyboard_", StringComparison.OrdinalIgnoreCase) >= 0 ||
        name.IndexOf("_KeyRelease_", StringComparison.OrdinalIgnoreCase) >= 0;

    if (!movementEvent)
        continue;

    string text = GetDecompiledText(code);
    var matched = needles.Where(n => text.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

    if (matched.Count == 0)
        continue;

    hits.Add(name + "  ->  " + string.Join(", ", matched));
}

if (hits.Count == 0)
{
    ScriptMessage(
        "LAST PETAL MOVEMENT TRACE\n\n" +
        "No movement/input code was found in Step or keyboard events.\n\n" +
        "NO CHANGES WERE MADE.\n\n" +
        "This means the upward movement may be coming from a different event type or from the player's existing movement code."
    );
    return;
}

string report = string.Join("\n", hits);
if (report.Length > 12000)
    report = report.Substring(0, 12000) + "\n...REPORT TRUNCATED...";

ScriptMessage(
    "LAST PETAL MOVEMENT TRACE\n\n" +
    "Found " + hits.Count + " relevant event(s).\n\n" +
    report +
    "\n\nNO CHANGES WERE MADE.\n\n" +
    "Send me a screenshot of this message. I will use the exact event name to make the next cleanup surgical instead of guessing."
);
