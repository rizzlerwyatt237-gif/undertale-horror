using System;
using System.Linq;
using System.Collections.Generic;
using UndertaleModLib.Models;
using UndertaleModLib.Compiler;

// LAST PETAL CLEANUP
// Removes ONLY the character-event hooks created by our known horror scripts.
// It does not touch rooms, sprites, movement, dialogue text, or SCR_GAMESTART.

EnsureDataLoaded();

string RemoveHook(string source, string marker, string[] endLines, out int removed)
{
    removed = 0;
    int search = 0;

    while (true)
    {
        int start = source.IndexOf(marker, search, StringComparison.Ordinal);
        if (start < 0) break;

        int end = -1;
        foreach (string endLine in endLines)
        {
            int candidate = source.IndexOf(endLine, start, StringComparison.Ordinal);
            if (candidate >= 0 && (end < 0 || candidate < end))
                end = candidate + endLine.Length;
        }

        if (end < 0)
        {
            ScriptError("Found " + marker + " but could not safely locate its end. No changes were made to this event.");
            return source;
        }

        // Consume the blank lines immediately following our injected block.
        while (end < source.Length && (source[end] == '\r' || source[end] == '\n' || source[end] == ' ' || source[end] == '\t'))
            end++;

        source = source.Remove(start, end - start);
        removed++;
        search = start;
    }

    return source;
}

var targetNames = new[] {
    "toriel", "papyrus", "undyne", "asgore", "mettaton", "alphys",
    "monsterkid", "monster_kid", "napstablook", "temmie", "doggo", "flowey", "sans"
};

int changed = 0;
int hooksRemoved = 0;

foreach (var code in Data.Code.Where(c => c?.Name?.Content != null).ToList())
{
    string name = code.Name.Content;
    bool target = targetNames.Any(n => name.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0);
    if (!target) continue;

    string original = GetDecompiledText(code);
    string cleaned = original;
    int totalRemoved = 0;

    int removed;
    cleaned = RemoveHook(cleaned, "// LAST PETAL EVERYONE HORROR HOOK", new[] {
        "global.last_petal_everyone_horrified = true;",
        "last_petal_flowey_reaction = \"terrified\";",
        "last_petal_sans_pressure = choose(\"watching\", \"warning\", \"relentless\", \"ambush\");"
    }, out removed);
    totalRemoved += removed;

    cleaned = RemoveHook(cleaned, "// LAST PETAL HORROR OVERDRIVE HOOK", new[] {
        "last_petal_reaction = \"horrified\";",
        "global.last_petal_flowey_hates_player = false;",
        "last_petal_sans_no_mercy = (global.last_petal_kills >= 15);",
        "image_angle += choose(-3, 3, -6, 6);",
        "image_angle += choose(-4, 4, -8, 8);",
        "image_xscale = -image_xscale;"
    }, out removed);
    totalRemoved += removed;

    if (totalRemoved == 0 || cleaned == original)
        continue;

    var imports = new CodeImportGroup(Data)
    {
        MainThreadAction = MainThreadAction
    };

    imports.QueueReplace(code, cleaned);
    var result = imports.Import();
    if (!result.Successful)
    {
        ScriptError("Cleanup failed for " + name + ":\n" + result.PrintAllErrors(false));
        return;
    }

    changed++;
    hooksRemoved += totalRemoved;
}

ScriptMessage(
    "LAST PETAL CHARACTER CLEANUP FINISHED.\n\n" +
    "Character events cleaned: " + changed + "\n" +
    "Horror hooks removed: " + hooksRemoved + "\n\n" +
    "The cleanup specifically removes our EVERYONE HORROR and HORROR OVERDRIVE hooks.\n" +
    "It does NOT modify SCR_GAMESTART or the text renderer.\n\n" +
    "Save a backup before saving data.win."
);
