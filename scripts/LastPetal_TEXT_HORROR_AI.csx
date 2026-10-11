using System;
using System.Linq;
using UndertaleModLib.Compiler;

EnsureDataLoaded();

// SAFE TEXT/CHARACTER LAYER
// This patch intentionally does NOT touch Step events, player movement,
// collisions, sprites, or room movement. It adds an adaptive horror state
// and dialogue hooks that can be used by existing character events.

var importGroup = new CodeImportGroup(Data)
{
    MainThreadAction = MainThreadAction,
    ThrowOnNoOpFindReplace = false
};

// Shared adaptive state. This is scripted/adaptive "AI-like" behavior,
// not a live cloud AI or internet connection.
var stateCode = @"
// LAST PETAL TEXT HORROR AI STATE
if (!variable_global_exists('last_petal_ai_memory')) global.last_petal_ai_memory = 0;
if (!variable_global_exists('last_petal_ai_mood')) global.last_petal_ai_mood = 0;
if (!variable_global_exists('last_petal_ai_seen')) global.last_petal_ai_seen = 0;
if (!variable_global_exists('last_petal_ai_turn')) global.last_petal_ai_turn = 0;
if (!variable_global_exists('last_petal_ai_chara')) global.last_petal_ai_chara = 0;
if (!variable_global_exists('last_petal_ai_flowey')) global.last_petal_ai_flowey = 0;
if (!variable_global_exists('last_petal_ai_text')) global.last_petal_ai_text = ''; 

global.last_petal_ai_seen += 1;
if (variable_global_exists('last_petal_kills')) global.last_petal_ai_memory += global.last_petal_kills;
if (variable_global_exists('last_petal_horror')) global.last_petal_ai_memory += global.last_petal_horror;
if (global.last_petal_ai_memory < 0) global.last_petal_ai_memory = 0;
if (global.last_petal_ai_memory > 999) global.last_petal_ai_memory = 999;

if (variable_global_exists('last_petal_kills')) {
    if (global.last_petal_kills >= 15) global.last_petal_ai_mood = 3;
    else if (global.last_petal_kills >= 5) global.last_petal_ai_mood = 2;
    else if (global.last_petal_kills >= 1) global.last_petal_ai_mood = 1;
    else global.last_petal_ai_mood = 0;
}

// Chara and Flowey are observers/commentators here; they react to the
// player's accumulated behavior instead of controlling movement.
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

// Try the game-start script first because it is a safer place for persistent
// globals than arbitrary Step events.
var gameStart = Data.Scripts.ByName("SCR_GAMESTART", true)?.Code;
if (gameStart != null)
    importGroup.QueueFindReplace(gameStart, "return", stateCode + "\nreturn");

// Add reusable text-generation functions. These return short original lines
// based on the current adaptive state. No graphic descriptions are used.
var aiFunctions = @"
function last_petal_ai_chara_line() {
    var m = variable_global_exists('last_petal_ai_chara') ? global.last_petal_ai_chara : 0;
    if (m >= 3) return 'You keep pretending this is a game. I remember every choice.';
    if (m >= 2) return 'I know what you have been doing. Do not pretend I cannot see it.';
    return '...I am watching. Even when nobody is talking.';
}

function last_petal_ai_flowey_line() {
    var m = variable_global_exists('last_petal_ai_flowey') ? global.last_petal_ai_flowey : 1;
    if (m >= 3) return 'You are not the one who should be afraid... but I am.';
    if (m >= 2) return 'I remember what you did. Please do not make me remember more.';
    return 'Heh heh... you keep making choices. I keep noticing.';
}

function last_petal_ai_text_line() {
    var m = variable_global_exists('last_petal_ai_mood') ? global.last_petal_ai_mood : 0;
    if (m >= 3) return 'THE ROOM REMEMBERS.';
    if (m >= 2) return 'SOMETHING IS WATCHING YOUR CHOICES.';
    if (m >= 1) return '...THAT WAS NOT FORGOTTEN.';
    return '...YOU FEEL LIKE YOU ARE BEING WATCHED.';
}
";

// Define the functions through the normal GML importer when possible.
// If a target script already exists, replace its body; otherwise the state
// layer above still remains safe and usable by future patches.
foreach (var candidate in new[] { "gml_GlobalScript_scr_game_start", "gml_GlobalScript_SCR_GAMESTART" })
{
    var code = Data.Code.ByName(candidate);
    if (code != null)
        importGroup.QueueFindReplace(code, "", aiFunctions);
}

importGroup.Import();

ScriptMessage("TEXT HORROR AI layer installed.");
ScriptMessage("No Step events or movement code were modified.");
ScriptMessage("Chara/Flowey adaptive state: scripted AI-style memory based on route history.");
ScriptMessage("Back up data.win before saving.");
