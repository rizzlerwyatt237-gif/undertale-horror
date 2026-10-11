EnsureDataLoaded();

// SAFE TEXT AI FOUNDATION - FIXED
// Only patches the existing SCR_GAMESTART assignment.
// Does not touch draw_text, dialogue renderers, Step events, movement, or rooms.

var gameStart = Data.Scripts.ByName("SCR_GAMESTART", true)?.Code as UndertaleCode;

if (gameStart == null)
{
    ScriptError("SAFE TEXT AI: SCR_GAMESTART was not found. Nothing was changed.");
    return;
}

string existing = GetDecompiledText(gameStart);
const string anchor = "global.debug = ";
int anchorIndex = existing.IndexOf(anchor, StringComparison.Ordinal);

if (anchorIndex < 0)
{
    ScriptError("SAFE TEXT AI: expected SCR_GAMESTART anchor was not found. Nothing was changed.");
    return;
}

// Keep the original debug value. We replace the complete assignment with
// one valid GML statement followed by the AI state assignments.
int semicolonIndex = existing.IndexOf(';', anchorIndex);
if (semicolonIndex < 0)
{
    ScriptError("SAFE TEXT AI: could not find the end of the debug assignment. Nothing was changed.");
    return;
}

string originalAssignment = existing.Substring(anchorIndex, semicolonIndex - anchorIndex + 1);
string safeReplacement = originalAssignment + @"

// LAST PETAL SAFE AI STATE
global.last_petal_ai_memory = 0;
global.last_petal_ai_turn = 0;
global.last_petal_ai_mood = 0;
global.last_petal_ai_flowey = 0;
global.last_petal_ai_chara = 0;
global.last_petal_ai_horror = 0;
";

UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data)
{
    ThrowOnNoOpFindReplace = true,
    MainThreadAction = MainThreadAction
};

importGroup.QueueFindReplace(gameStart, originalAssignment, safeReplacement);

try
{
    importGroup.Import();
    ChangeSelection(gameStart);
    ScriptMessage("SAFE TEXT AI installed successfully.");
    ScriptMessage("Only SCR_GAMESTART was changed; the text renderer, Step events, movement, and room code were not modified.");
}
catch (Exception ex)
{
    ScriptError("SAFE TEXT AI aborted: " + ex.Message);
}