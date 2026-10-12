using System;
using System.Linq;

// RESTORE HORROR TEXT
// Reverses the safe blanket text change made by HORROR_ALL_VISIBLE_TEXT.csx.
// It removes only the exact [HORROR] prefix that script added.
// It does NOT touch GML, rooms, objects, sprites, movement, or events.
// BACK UP data.win before running.

EnsureDataLoaded();

int restored = 0;

foreach (var str in Data.Strings)
{
    if (str == null || string.IsNullOrEmpty(str.Content))
        continue;

    if (str.Content.StartsWith("[HORROR] ", StringComparison.Ordinal))
    {
        str.Content = str.Content.Substring("[HORROR] ".Length);
        restored++;
    }
}

ScriptMessage(
    "===== RESTORE HORROR TEXT =====\n\n" +
    "Restored strings: " + restored + "\n\n" +
    "Only the [HORROR] prefix was removed.\n" +
    "No GML, rooms, objects, sprites, movement, or events were changed.\n\n" +
    "Now save the repaired data.win and test the game.\n" +
    "================================"
);
