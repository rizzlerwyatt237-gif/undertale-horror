using System;
using System.Collections.Generic;
using System.Linq;
using UndertaleModLib.Models;
using UndertaleModLib.Compiler;

// REAL DIALOGUE HORROR PATCH
// Targets Undertale's actual textdata_en / SCR_TEXT dialogue resources.
// The previous master changed only a small list of exact string matches.
// This script finds dialogue strings used by the text-data scripts and
// changes their visible wording while preserving Undertale control codes.
// It does NOT touch movement, rooms, sprites, attacks, or player Step code.
// BACK UP data.win before running.

EnsureDataLoaded();

var dialogueOwners = Data.Code
    .Where(c => c?.Name?.Content != null)
    .Where(c => {
        string n = c.Name.Content.ToLowerInvariant();
        return n.Contains("textdata_en") || n.Contains("text_data_en") || n.Contains("scr_text");
    })
    .ToList();

if (dialogueOwners.Count == 0)
{
    ScriptError("REAL DIALOGUE PATCH: could not find textdata_en / SCR_TEXT code entries. Nothing was changed.");
    return;
}

var usedStrings = new HashSet<UndertaleString>();

// Find actual string resources referenced by the dialogue code.
foreach (UndertaleCode owner in dialogueOwners)
{
    string code = GetDecompiledText(owner);

    foreach (var str in Data.Strings)
    {
        if (str == null || string.IsNullOrEmpty(str.Content))
            continue;

        string escaped = str.Content
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");

        if (escaped.Length >= 3 && code.Contains("\"" + escaped + "\"", StringComparison.Ordinal))
            usedStrings.Add(str);
    }
}

int changed = 0;

foreach (var str in usedStrings)
{
    string original = str.Content;
    if (string.IsNullOrWhiteSpace(original))
        continue;

    // Skip keys, filenames, identifiers and other non-dialogue resources.
    string t = original.Trim();
    if (t.Length < 3 ||
        t.StartsWith("SCR_", StringComparison.OrdinalIgnoreCase) ||
        t.StartsWith("obj_", StringComparison.OrdinalIgnoreCase) ||
        t.StartsWith("room_", StringComparison.OrdinalIgnoreCase) ||
        t.StartsWith("spr_", StringComparison.OrdinalIgnoreCase) ||
        t.StartsWith("snd_", StringComparison.OrdinalIgnoreCase) ||
        t.Contains(".png", StringComparison.OrdinalIgnoreCase) ||
        t.Contains(".ogg", StringComparison.OrdinalIgnoreCase) ||
        t.Contains(".wav", StringComparison.OrdinalIgnoreCase) ||
        t.Contains(".gml", StringComparison.OrdinalIgnoreCase))
        continue;

    if (t.StartsWith("[HORROR-DIALOGUE]", StringComparison.Ordinal))
        continue;

    // Keep Undertale's control codes intact. Add a clearly visible horror
    // marker after leading control sequences and speaker markers.
    int pos = 0;
    while (pos < original.Length)
    {
        if (original[pos] == '\\' && pos + 1 < original.Length)
        {
            pos += 2;
            continue;
        }
        if (original[pos] == '*')
        {
            pos++;
            continue;
        }
        if (original[pos] == ' ' || original[pos] == '\t')
        {
            pos++;
            continue;
        }
        break;
    }

    string replacement = original.Insert(pos, "[HORROR-DIALOGUE] ");
    str.Content = replacement;
    changed++;
}

ScriptMessage(
    "===== REAL DIALOGUE HORROR PATCH =====\n\n" +
    "Dialogue code entries found: " + dialogueOwners.Count + "\n" +
    "Actual dialogue strings changed: " + changed + "\n\n" +
    "This targets textdata_en / SCR_TEXT dialogue instead of only a few exact lines.\n" +
    "Undertale control codes were preserved.\n" +
    "Player movement was NOT modified.\n\n" +
    "Save data.win and test a conversation.\n" +
    "If changed: the dialogue system is now being patched correctly.\n" +
    "======================================="
);