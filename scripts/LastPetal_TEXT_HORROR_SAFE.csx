EnsureDataLoaded();

// SAFE TEXT-AI FOUNDATION
// This script deliberately does NOT patch draw_text, dialogue renderers,
// Step events, player movement, or every string in the game.
// Those global patches caused the room-to-room text corruption.

var gameStart = Data.Scripts.ByName("SCR_GAMESTART", true)?.Code as UndertaleCode;

if (gameStart == null)
{
    ScriptError("SAFE TEXT AI: SCR_GAMESTART was not found. Nothing was changed.");
    return;
}

string existing = GetDecompiledText(gameStart);

// Use a real, known Undertale anchor instead of an empty-string replacement.
// If the anchor is missing, abort without changing data.win.
const string anchor = "global.debug = ";

if (!existing.Contains(anchor, StringComparison.Ordinal))
{
    ScriptError("SAFE TEXT AI: expected SCR_GAMESTART anchor was not found. Nothing was changed.");
    return;
}

string safeState = @"
// LAST PETAL SAFE AI STATE
// Memory is state only; no text renderer is modified here.
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

// Insert only after the exact existing debug assignment.
// Nothing else in the game is searched or rewritten.
importGroup.QueueFindReplace(
    gameStart,
    anchor,
    anchor + safeState + "\n"
);

try
{
    importGroup.Import();
    ChangeSelection(gameStart);
    ScriptMessage("SAFE TEXT AI installed.");
    ScriptMessage("No dialogue renderer, draw_text function, Step event, movement code, or global text replacement was modified.");
    ScriptMessage("If the next room's text is normal, this foundation is safe to build on.");
}
catch (Exception ex)
{
    ScriptError("SAFE TEXT AI aborted: " + ex.Message);
}
