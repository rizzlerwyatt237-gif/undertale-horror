using System;
using System.Linq;
using System.Text;

// ALL ENDINGS SCAN
// Diagnostic only. This does NOT modify data.win.
// It lists code entries whose names look related to endings, routes,
// epilogues, True Pacifist, Neutral, or Genocide so the horror patch
// can target the actual build safely.

EnsureDataLoaded();

var sb = new StringBuilder();
sb.AppendLine("===== ALL ENDINGS SCAN =====");
sb.AppendLine("NO CHANGES WERE MADE TO data.win.");
sb.AppendLine();

string[] terms =
{
    "ending", "epilogue", "genocide", "pacifist", "neutral",
    "route", "credits", "ending", "truepacifist", "soulless"
};

var matches = Data.Code
    .Where(c => c?.Name?.Content != null)
    .Select(c => c.Name.Content)
    .Where(name => terms.Any(term => name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .OrderBy(name => name)
    .ToList();

sb.AppendLine("MATCHING CODE ENTRIES: " + matches.Count);
sb.AppendLine();

if (matches.Count == 0)
    sb.AppendLine("  NONE FOUND");
else
    foreach (var name in matches)
        sb.AppendLine("  " + name);

sb.AppendLine();
sb.AppendLine("GAME OBJECTS WITH ROUTE/ENDING NAMES:");
var objects = Data.GameObjects
    .Where(o => o?.Name?.Content != null)
    .Select(o => o.Name.Content)
    .Where(name => terms.Any(term => name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .OrderBy(name => name)
    .ToList();

if (objects.Count == 0)
    sb.AppendLine("  NONE FOUND");
else
    foreach (var name in objects)
        sb.AppendLine("  " + name);

sb.AppendLine();
sb.AppendLine("Run this first. The next patch will use the exact names found here.");
sb.AppendLine("================================");

ScriptMessage(sb.ToString());
