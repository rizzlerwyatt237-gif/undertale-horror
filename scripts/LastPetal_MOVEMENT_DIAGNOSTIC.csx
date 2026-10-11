using System;
using System.Linq;
using System.Collections.Generic;
using UndertaleModLib.Models;

// LAST PETAL MOVEMENT DIAGNOSTIC
// READ-ONLY: this script makes NO changes to data.win.
// It scans code for our previous horror hooks inside Step events and
// reports anything that could be responsible for continuous movement.

EnsureDataLoaded();

var markers = new[] {
    "LAST PETAL EVERYONE HORROR HOOK",
    "LAST PETAL HORROR OVERDRIVE HOOK",
    "LAST PETAL SAFE MASTER",
    "LAST PETAL SAFE FLOWEY",
    "LAST PETAL HORROR"
};

var found = new List<string>();

foreach (UndertaleCode code in Data.Code)
{
    if (code == null || code.Name == null || code.Name.Content == null)
        continue;

    string name = code.Name.Content;
    bool isStep = name.IndexOf("_Step_", StringComparison.OrdinalIgnoreCase) >= 0 ||
                  name.EndsWith("_Step", StringComparison.OrdinalIgnoreCase);

    if (!isStep)
        continue;

    string text = GetDecompiledText(code);

    foreach (string marker in markers)
    {
        if (text.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0)
        {
            found.Add(name + "  ->  " + marker);
            break;
        }
    }
}

if (found.Count == 0)
{
    ScriptMessage(
        "LAST PETAL MOVEMENT DIAGNOSTIC\n\n" +
        "No known Last Petal horror hooks were found in Step events.\n\n" +
        "NO CHANGES WERE MADE."
    );
    return;
}

ScriptMessage(
    "LAST PETAL MOVEMENT DIAGNOSTIC\n\n" +
    "Found " + found.Count + " Step event(s) containing a Last Petal marker:\n\n" +
    string.Join("\n", found) +
    "\n\nNO CHANGES WERE MADE.\n\n" +
    "Send me a screenshot of this result before running any cleanup script."
);