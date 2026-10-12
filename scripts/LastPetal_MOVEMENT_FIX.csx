using System;
using System.Linq;
using UndertaleModLib.Models;
using UndertaleModLib.Compiler;

// LAST PETAL MOVEMENT FIX
// Surgical cleanup for the continuous-upward-movement bug.
// ONLY removes our known EVERYONE HORROR / HORROR OVERDRIVE blocks
// from Step / Begin Step / End Step events.
// Original Undertale movement code is preserved.

EnsureDataLoaded();

string RemoveHook(string source, string marker, string[] endLines, out int removed)
{
    removed = 0;
    int search = 0;

    while (true)
    {
        int start = source.IndexOf(marker, search, StringComparison.Ordinal);
        if (start < 0)
            break;

        int end = -1;
        foreach (string endLine in endLines)
        {
            int candidate = source.IndexOf(endLine, start, StringComparison.Ordinal);
            if (candidate >= 0 && (end < 0 || candidate < end))
                end = candidate + endLine.Length;
        }

        if (end < 0)
        {
            ScriptError("Found " + marker + " but could not safely find its end in " +
                "the Step event. That event was NOT changed.");
            return source;
        }

        while (end < source.Length &&
              (source[end] == '\r' || source[end] == '\n' ||
               source[end] == ' ' || source[end] == '\t'))
            end++;

        source = source.Remove(start, end - start);
        removed++;
        search = start;
    }

    return source;
}

int changed = 0;
int removedHooks = 0;

foreach (UndertaleCode code in Data.Code.ToList())
{
    if (code == null || code.Name == null || code.Name.Content == null)
        continue;

    string name = code.Name.Content;

    bool isStep = name.IndexOf("_Step_", StringComparison.OrdinalIgnoreCase) >= 0 ||
                  name.IndexOf("_BeginStep_", StringComparison.OrdinalIgnoreCase) >= 0 ||
                  name.IndexOf("_EndStep_", StringComparison.OrdinalIgnoreCase) >= 0;

    if (!isStep)
        continue;

    string original = GetDecompiledText(code);
    string cleaned = original;
    int total = 0;
    int removed;

    cleaned = RemoveHook(cleaned,
        "// LAST PETAL EVERYONE HORROR HOOK",
        new[] {
            "image_xscale = -image_xscale;",
            "image_angle += choose(-6, 6);",
            "last_petal_sans_pressure = choose(\"watching\", \"warning\", \"relentless\", \"ambush\");"
        }, out removed);
    total += removed;

    cleaned = RemoveHook(cleaned,
        "// LAST PETAL HORROR OVERDRIVE HOOK",
        new[] {
            "image_angle += choose(-3, 3, -6, 6);",
            "image_angle += choose(-4, 4, -8, 8);",
            "image_xscale = -image_xscale;",
            "last_petal_reaction = \"horrified\";"
        }, out removed);
    total += removed;

    if (total == 0 || cleaned == original)
        continue;

    var imports = new CodeImportGroup(Data)
    {
        MainThreadAction = MainThreadAction
    };

    imports.QueueReplace(code, cleaned);
    var result = imports.Import();

    if (!result.Successful)
    {
        ScriptError("Movement fix failed for " + name + ":\n" +
            result.PrintAllErrors(false));
        return;
    }

    changed++;
    removedHooks += total;
}

ScriptMessage(
    "LAST PETAL MOVEMENT FIX FINISHED.\n\n" +
    "Step events changed: " + changed + "\n" +
    "Old horror Step hooks removed: " + removedHooks + "\n\n" +
    "This script does NOT delete original Undertale movement code.\n" +
    "It only removes the two known Last Petal horror blocks from Step events.\n\n" +
    "BACK UP data.win before saving."
);
