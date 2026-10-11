using System;
using UndertaleModLib.Compiler;

EnsureDataLoaded();

// SAFE TEXT HORROR / MEMORY LAYER
// IMPORTANT: this version never uses an empty-string replacement and never
// patches Step events, movement, collisions, sprites, or arbitrary dialogue.
// The previous version could corrupt code/text because replacing "" is unsafe.

var importGroup = new CodeImportGroup(Data)
{
    MainThreadAction = MainThreadAction,
    ThrowOnNoOpFindReplace = false
};

// Put adaptive memory in the game's startup code without replacing an empty
// string. We only inject immediately before a real return statement.
var stateCode = @"
// LAST PETAL SAFE HORROR MEMORY
if (!variable_global_exists('last_petal_ai_memory')) global.last_petal_ai_memory = 0;
if (!variable_global_exists('last_petal_ai_mood')) global.last_petal_ai_mood = 0;
if (!variable_global_exists('last_petal_ai_turn')) global.last_petal_ai_turn = 0;
if (!variable_global_exists('last_petal_ai_chara')) global.last_petal_ai_chara = 0;
if (!variable_global_exists('last_petal_ai_flowey')) global.last_petal_ai_flowey = 0;

global.last_petal_ai_turn += 1;

if (variable_global_exists('last_petal_kills')) {
    global.last_petal_ai_memory = max(global.last_petal_ai_memory, global.last_petal_kills);
    if (global.last_petal_kills >= 15) global.last_petal_ai_mood = 3;
    else if (global.last_petal_kills >= 5) global.last_petal_ai_mood = 2;
    else if (global.last_petal_kills >= 1) global.last_petal_ai_mood = 1;
    else global.last_petal_ai_mood = 0;
}

if (global.last_petal_ai_mood >= 3) {
    global.last_petal_ai_chara = 3;
    global.last_petal_ai_flowey = 3;
} else if (global.last_petal_ai_mood >= 1) {
    global.last_petal_ai_chara = 2;
    global.last_petal_ai_flowey = 2;
} else {
    global.last_petal_ai_chara = 0;
    global.last_petal_ai_flowey = 1;
}
";

// Use only a known real token in the startup script.
var gameStart = Data.Scripts.ByName("SCR_GAMESTART", true)?.Code;
if (gameStart != null)
{
    importGroup.QueueFindReplace(gameStart, "return", stateCode + "\nreturn");
}
else
{
    ScriptMessage("SCR_GAMESTART was not found; no code was changed.");
}

importGroup.Import();

ScriptMessage("SAFE TEXT HORROR MEMORY patch installed.");
ScriptMessage("The unsafe empty-string replacement is gone.");
ScriptMessage("No Step events, movement, collisions, sprites, or dialogue text were modified.");
ScriptMessage("Use a backup of data.win before saving.");
