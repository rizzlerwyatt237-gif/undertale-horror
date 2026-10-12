using System;
using System.Linq;
using System.Text;

// VERIFY HORROR INSTALL
// Diagnostic only. Makes NO changes to data.win.
// Use this on the exact data.win that you are launching.

EnsureDataLoaded();

var sb = new StringBuilder();
sb.AppendLine("===== VERIFY HORROR INSTALL =====");
sb.AppendLine("NO CHANGES WERE MADE.");
sb.AppendLine();

int masterMarkers = 0;
int floweyMarkers = 0;
int sansMarkers = 0;
int undyneMarkers = 0;

foreach (var code in Data.Code)
{
    if (code?.Name?.Content == null) continue;
    string text = GetDecompiledText(code);

    if (text.Contains("ULTIMATE_HORROR_MASTER_START", StringComparison.Ordinal) ||
        text.Contains("ULTIMATE_HORROR_MASTER_STATE", StringComparison.Ordinal))
        masterMarkers++;

    if (text.Contains("ULTIMATE_FLOWEY_ALLY", StringComparison.Ordinal))
        floweyMarkers++;

    if (text.Contains("ULTIMATE_SANS_STATE", StringComparison.Ordinal))
        sansMarkers++;

    if (text.Contains("ULTIMATE_UNDYNE_CREATE", StringComparison.Ordinal) ||
        text.Contains("ULTIMATE_UNDYNE_MELT", StringComparison.Ordinal))
        undyneMarkers++;
}

int horrorStrings = Data.Strings.Count(s =>
    s != null &&
    !string.IsNullOrEmpty(s.Content) &&
    s.Content.Contains("[HORROR-DIALOGUE]", StringComparison.Ordinal));

sb.AppendLine("MASTER startup/state markers: " + masterMarkers);
sb.AppendLine("FLOWEY ally markers: " + floweyMarkers);
sb.AppendLine("SANS horror markers: " + sansMarkers);
sb.AppendLine("UNDYNE markers: " + undyneMarkers);
sb.AppendLine("[HORROR-DIALOGUE] strings: " + horrorStrings);
sb.AppendLine();
sb.AppendLine("If these are all 0, you are checking a clean/unmodified data.win.");
sb.AppendLine("If they are nonzero but the game looks completely normal, you are probably launching a different data.win than this one.");
sb.AppendLine();
sb.AppendLine("GENERAL INFO:");
sb.AppendLine("Internal name: " + (Data.GeneralInfo?.Name?.Content ?? "<unknown>"));
sb.AppendLine("Display name: " + (Data.GeneralInfo?.DisplayName?.Content ?? "<unknown>"));
sb.AppendLine("=================================");

ScriptMessage(sb.ToString());