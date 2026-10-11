EnsureDataLoaded();

// SAFE TEXT-AI FOUNDATION v2
// Does not patch draw_text, dialogue renderers, Step events, movement,
// or every string in the game.

var gameStart = Data.Scripts.ByName("SCR_GAMESTART", true)?.Code as UndertaleCode;

if (gameStart == null)
{
    ScriptError("SAFE TEXT AI: SCR_GAMESTART was not found. Nothing was changed.");
    return;
}

string existing = GetDecompiledText(gameStart);
const string anchor = "global.debug = ";

if (!existing.Contains(anchor))
{
    ScriptError("SAFE TEXT AI: expected SCR_GAMESTART anchor was not found. Nothing was changed.");
    return;
}

string safeState = @"
// LAST PETAL SAFE AI STATE
// State only; no text renderer is modified.
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

// IMPORTANT: put the state BEFORE the existing assignment.
// Replacing "global.debug = " with "state + global.debug = " keeps the
// original right-hand side intact and therefore produces valid GML.
importGroup.QueueFindReplace(
    gameStart,
    anchor,
    safeState + "\nglobal.debug = "
);

try
{
    importGroup.Import();
    ChangeSelection(gameStart);
    ScriptMessage("SAFE TEXT AI installed successfully.");
    ScriptMessage("Only SCR_GAMESTART was patched.");
    ScriptMessage("No dialogue renderer, draw_text, Step event, movement code, or global text replacement was modified.");
}
catch (Exception ex)
{
    ScriptError("SAFE TEXT AI aborted: " + ex.Message);
}
