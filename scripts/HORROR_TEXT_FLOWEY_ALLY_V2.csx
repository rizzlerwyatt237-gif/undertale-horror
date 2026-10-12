// HORROR TEXT — FLOWEY ALLY V2 FIXED
// Flowey is ON THE PLAYER'S SIDE.
// IMPORTANT: replacement text uses literal \\n escapes so imported GML stays valid.
// Text-only: does not touch player movement, rooms, sprites, attacks, or Step events.

using System;
using System.Collections.Generic;
using UndertaleModLib.Compiler;

EnsureDataLoaded();

var replacements = new Dictionary<string, string>(StringComparer.Ordinal)
{
    // FLOWEY — loyal ally, still creepy and aware.
    { "Howdy! I'm FLOWEY. FLOWEY the FLOWER!", "Oh. It's YOU.\\nGood. You're here.\\nI've been waiting for you.\\nThis time, I'm on your side." },
    { "I'M FLOWEY! FLOWEY THE FLOWER!", "I'M FLOWEY.\\nAnd before you ask... yes.\\nI remember.\\nI'm helping you this time." },
    { "You IDIOT!", "Hey! Don't listen to them.\\nThey want you scared.\\nStay with me. I've got you." },

    // FLOWEY ally-specific phrases.
    { "Kill or be killed.", "No. Not this time.\\nStay with me.\\nI'll help you find another way." },
    { "In this world, it's kill or be killed.", "They think this world is kill or be killed.\\nThey're wrong.\\nAnd I'll prove it to you." },

    // SANS — suspicious and hostile toward the player.
    { "hey. what's up?", "heh.\\nyou again.\\nflowey's with you now?\\n...great. just great." },
    { "it's a beautiful day outside.", "it's a quiet day outside.\\ntoo quiet.\\neveryone heard what you did." },

    // PAPYRUS — frightened but trying to stand up to the player.
    { "NYEH HEH HEH!", "NYEH...\\nHEH...\\nI AM NOT AFRAID!\\n...I AM A LITTLE AFRAID." },
    { "I WILL CAPTURE YOU!", "I... WILL STOP YOU.\\nI HAVE TO PROTECT EVERYONE." },

    // TORIEL — protective and worried.
    { "Hello, my child.", "Hello, my child...\\nPlease.\\nI don't know what you have become." },

    // UNDYNE — angry and determined.
    { "YOU'RE GONNA HAVE A BAD TIME!", "YOU'RE GOING TO ANSWER FOR WHAT YOU'VE DONE.\\nI'M NOT BACKING DOWN." },

    // ALPHYS — nervous.
    { "U-um...", "U-um...\\nI-I know you're there.\\nPlease don't come any closer." },

    // ASGORE — grim.
    { "Greetings. I am ASGORE DREEMURR, King of the Monsters.", "Greetings...\\nI am ASGORE DREEMURR.\\nI had hoped this meeting would be different." }
};

var importGroup = new CodeImportGroup(Data)
{
    MainThreadAction = MainThreadAction,
    ThrowOnNoOpFindReplace = false
};

int changedCodes = 0;
int changedPhrases = 0;

foreach (var code in Data.Code)
{
    if (code == null)
        continue;

    string text;
    try
    {
        text = GetDecompiledText(code);
    }
    catch
    {
        continue;
    }

    if (string.IsNullOrEmpty(text))
        continue;

    bool queued = false;

    foreach (var pair in replacements)
    {
        if (!text.Contains(pair.Key, StringComparison.Ordinal))
            continue;

        importGroup.QueueFindReplace(code, pair.Key, pair.Value);
        queued = true;
        changedPhrases++;
    }

    if (queued)
        changedCodes++;
}

if (changedCodes == 0)
{
    ScriptMessage("FLOWEY ALLY TEXT: no matching dialogue was found. Nothing was changed.");
    return;
}

importGroup.Import();

ScriptMessage($"FLOWEY ALLY TEXT installed: {changedPhrases} replacements across {changedCodes} code entries.");
ScriptMessage("Flowey is now written as the player's ally; player movement and Step events were not modified.");
