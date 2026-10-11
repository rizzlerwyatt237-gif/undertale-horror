EnsureDataLoaded();

// LAST PETAL GAMESTART SCANNER
// Diagnostic only. This script makes NO changes to the data.win.
// It prints the exact SCR_GAMESTART code around global.debug so we can
// identify why the previous import failed instead of guessing again.

var gameStart = Data.Scripts.ByName("SCR_GAMESTART", true)?.Code as UndertaleCode;

if (gameStart == null)
{
    ScriptError("GAMESTART SCAN: SCR_GAMESTART was not found. NO CHANGES WERE MADE.");
    return;
}

string existing = GetDecompiledText(gameStart);
int index = existing.IndexOf("global.debug", StringComparison.Ordinal);

if (index < 0)
{
    ScriptMessage("GAMESTART SCAN: no global.debug text was found in SCR_GAMESTART.");
    ScriptMessage("NO CHANGES WERE MADE.");
    return;
}

int start = Math.Max(0, index - 500);
int length = Math.Min(existing.Length - start, 1600);
string excerpt = existing.Substring(start, length);

ScriptMessage("===== SCR_GAMESTART EXCERPT =====");
ScriptMessage(excerpt);
ScriptMessage("===== END EXCERPT =====");
ScriptMessage("NO CHANGES WERE MADE TO data.win.");
