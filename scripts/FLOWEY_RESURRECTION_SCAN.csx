using System;
using System.Linq;
using System.Text;
using UndertaleModLib.Models;

// FLOWEY RESURRECTION SCAN
// Diagnostic only. NO CHANGES are made to data.win.
// This finds the actual Flowey-related object/code names in the opened Undertale build.

EnsureDataLoaded();

var sb = new StringBuilder();
sb.AppendLine("===== FLOWEY RESURRECTION SCAN =====");
sb.AppendLine("NO CHANGES WERE MADE.");
sb.AppendLine();

var objects = Data.GameObjects
    .Where(o => o?.Name?.Content != null && o.Name.Content.IndexOf("flowey", StringComparison.OrdinalIgnoreCase) >= 0)
    .Select(o => o.Name.Content)
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .OrderBy(x => x)
    .ToList();

sb.AppendLine("FLOWEY GAME OBJECTS:");
if (objects.Count == 0)
    sb.AppendLine("  NONE FOUND");
else
    foreach (var name in objects)
        sb.AppendLine("  " + name);

sb.AppendLine();
sb.AppendLine("FLOWEY CODE ENTRIES:");
var codes = Data.Code
    .Where(c => c?.Name?.Content != null && c.Name.Content.IndexOf("flowey", StringComparison.OrdinalIgnoreCase) >= 0)
    .Select(c => c.Name.Content)
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .OrderBy(x => x)
    .ToList();

if (codes.Count == 0)
    sb.AppendLine("  NONE FOUND");
else
    foreach (var name in codes)
        sb.AppendLine("  " + name);

sb.AppendLine();
sb.AppendLine("GENERAL GAME INFO:");
sb.AppendLine("  Internal name: " + (Data.GeneralInfo?.Name?.Content ?? "<unknown>"));
sb.AppendLine("  Display name: " + (Data.GeneralInfo?.DisplayName?.Content ?? "<unknown>"));
sb.AppendLine("===================================");

ScriptMessage(sb.ToString());
