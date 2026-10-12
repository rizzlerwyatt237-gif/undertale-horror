using System;
using System.IO;
using System.Linq;
using System.Text;
using UndertaleModLib.Models;

// DIALOGUE STORAGE SCANNER
// Diagnostic only. Makes NO changes to data.win.

EnsureDataLoaded();

string path = PromptSaveFile(".txt", "Text files (*.txt)|*.txt|All files (*.*)|*.*");
if (string.IsNullOrWhiteSpace(path)) return;

string[] needles = { "FLOWEY", "SANS", "TORIEL", "UNDYNE", "ASGORE", "Howdy", "Kill or be killed", "BAD TIME" };
string[] nameNeedles = { "text", "dialog", "message", "msg", "talk", "speech", "language", "local" };

var sb = new StringBuilder();
sb.AppendLine("===== DIALOGUE STORAGE SCAN =====");
sb.AppendLine("NO CHANGES WERE MADE.");
sb.AppendLine();
sb.AppendLine("Internal name: " + (Data.GeneralInfo?.Name?.Content ?? "<unknown>"));
sb.AppendLine("Display name: " + (Data.GeneralInfo?.DisplayName?.Content ?? "<unknown>"));
sb.AppendLine("String count: " + Data.Strings.Count);
sb.AppendLine("Code count: " + Data.Code.Count);
sb.AppendLine();

sb.AppendLine("===== MATCHING STRINGS =====");
int stringHits = 0;
for (int i = 0; i < Data.Strings.Count; i++)
{
    var s = Data.Strings[i];
    if (s == null || string.IsNullOrEmpty(s.Content)) continue;
    string low = s.Content.ToLowerInvariant();
    if (!needles.Any(n => low.Contains(n.ToLowerInvariant()))) continue;
    stringHits++;
    sb.AppendLine("INDEX " + i + ": " + s.Content.Replace("\r", "\\r").Replace("\n", "\\n"));
}
sb.AppendLine("String matches: " + stringHits);
sb.AppendLine();

sb.AppendLine("===== CODE ENTRIES WITH TEXT-LIKE NAMES =====");
int codeNameHits = 0;
foreach (UndertaleCode c in Data.Code)
{
    if (c?.Name?.Content == null) continue;
    string low = c.Name.Content.ToLowerInvariant();
    if (!nameNeedles.Any(n => low.Contains(n))) continue;
    codeNameHits++;
    sb.AppendLine(c.Name.Content);
}
sb.AppendLine("Text-like code entries: " + codeNameHits);
sb.AppendLine();

sb.AppendLine("===== CODE ENTRIES CONTAINING KNOWN TEXT =====");
int codeHits = 0;
foreach (UndertaleCode c in Data.Code)
{
    if (c?.Name?.Content == null) continue;
    string text;
    try { text = GetDecompiledText(c); }
    catch { continue; }
    if (string.IsNullOrEmpty(text)) continue;
    string low = text.ToLowerInvariant();
    if (!needles.Any(n => low.Contains(n.ToLowerInvariant()))) continue;
    codeHits++;
    sb.AppendLine(c.Name.Content);
}
sb.AppendLine("Code matches: " + codeHits);
sb.AppendLine();

sb.AppendLine("===== CONCLUSION GUIDE =====");
sb.AppendLine("String matches > 0: candidate text exists in Data.Strings.");
sb.AppendLine("Code matches > 0: code directly contains/references recognizable dialogue text.");
sb.AppendLine("Both are 0: dialogue is likely stored/selected through another resource or encoded table.");
sb.AppendLine("Do not run another replacement script yet.");
sb.AppendLine("Send this TXT report back so the real source can be identified.");

File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
ScriptMessage("DIALOGUE STORAGE SCAN COMPLETE.\n\nNo changes were made.\nString matches: " + stringHits + "\nCode matches: " + codeHits + "\nText-like code entries: " + codeNameHits + "\n\nReport saved to:\n" + path);