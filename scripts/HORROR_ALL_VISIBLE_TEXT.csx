using System;
using System.Linq;
using UndertaleModLib.Models;

// HORROR ALL VISIBLE TEXT
// Changes human-readable strings across the game to give the mod a
// consistent horror tone. It intentionally skips strings that look
// like internal asset/code identifiers so the game is less likely to break.
// BACK UP data.win before running.

EnsureDataLoaded();

int changed = 0;

foreach (var str in Data.Strings)
{
    if (str == null || string.IsNullOrWhiteSpace(str.Content))
        continue;

    string s = str.Content;
    string t = s.Trim();

    if (t.Length < 2)
        continue;

    // Skip likely internal identifiers, paths, formats, and control strings.
    if (t.StartsWith("gml_", StringComparison.OrdinalIgnoreCase) ||
        t.StartsWith("obj_", StringComparison.OrdinalIgnoreCase) ||
        t.StartsWith("spr_", StringComparison.OrdinalIgnoreCase) ||
        t.StartsWith("room_", StringComparison.OrdinalIgnoreCase) ||
        t.StartsWith("snd_", StringComparison.OrdinalIgnoreCase) ||
        t.StartsWith("scr_", StringComparison.OrdinalIgnoreCase) ||
        t.Contains("\\") || t.Contains("/") ||
        t.Contains(".png") || t.Contains(".ogg") || t.Contains(".wav") ||
        t.Contains(".gml") || t.Contains(".exe"))
        continue;

    // Skip strings that are overwhelmingly numeric/symbolic.
    int letters = t.Count(char.IsLetter);
    if (letters < 3)
        continue;

    // Don't repeatedly modify strings from a previous run.
    if (t.StartsWith("[HORROR] ", StringComparison.Ordinal))
        continue;

    // Preserve multiline dialogue while making every eligible visible
    // line clearly different and horror-themed.
    string replacement = "[HORROR] " + s;
    str.Content = replacement;
    changed++;
}

ScriptMessage(
    "===== HORROR ALL VISIBLE TEXT =====\n\n" +
    "Changed human-readable strings: " + changed + "\n\n" +
    "Every eligible visible text string now begins with [HORROR].\n" +
    "Internal-looking asset/code strings were skipped to reduce breakage.\n\n" +
    "BACK UP data.win before saving.\n" +
    "===================================="
);
