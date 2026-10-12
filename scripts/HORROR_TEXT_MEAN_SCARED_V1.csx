// HORROR TEXT — MEAN / SCARED V1
// Safe text-focused patch.
// Does NOT touch player movement, rooms, sprites, attacks, or Step events.
// It scans existing GML code and changes selected recognizable dialogue phrases.
// If a phrase is not present in a particular Undertale build, it is simply skipped.

using System;
using System.Collections.Generic;
using UndertaleModLib.Compiler;

EnsureDataLoaded();

var replacements = new Dictionary<string, string>(StringComparer.Ordinal)
{
    // FLOWEY — frightened but still nasty
    { "Howdy! I'm FLOWEY. FLOWEY the FLOWER!", "Oh. It's YOU again.\nI remember what you did.\n...And I'm not laughing anymore." },
    { "I'M FLOWEY! FLOWEY THE FLOWER!", "I'M FLOWEY.\nAND I REMEMBER YOU.\nDON'T COME ANY CLOSER." },
    { "You IDIOT!", "You really don't understand, do you?\nEveryone is scared of you." },

    // SANS — colder / meaner
    { "hey. what's up?", "heh.\nyou again.\nkeep walking.\ni'm already tired of watching you." },
    { "it's a beautiful day outside.", "it's a quiet day outside.\ntoo quiet.\neveryone heard what you did." },
    { "welp. i'm going to grillby's.", "welp. i'm leaving.\ni don't feel like being near you today." },

    // PAPYRUS — frightened instead of cheerful
    { "NYEH HEH HEH!", "NYEH...\nHEH...\nwhy aren't you afraid of me?\n...why am I afraid of you?" },
    { "I WILL CAPTURE YOU!", "I... WILL STOP YOU.\nI HAVE TO.\nBEFORE YOU HURT ANYONE ELSE." },

    // TORIEL — protective / worried
    { "Hello, my child.", "Hello, my child...\nPlease.\nDo not make me afraid of you." },
    { "What a terrible creature, torturing such a poor, innocent youth...", "What a terrible thing you have become.\nPlease...\nI do not want to be afraid of you." },

    // UNDYNE — furious and alarmed
    { "NYEH HEH HEH! YOU'RE GONNA HAVE A BAD TIME!", "YOU.\nI KNOW WHAT YOU'VE DONE.\nI'M NOT RUNNING FROM YOU.\nBUT I'M NOT PRETENDING I'M NOT SCARED." },
    { "YOU'RE GONNA HAVE A BAD TIME!", "YOU'RE GOING TO LEARN WHAT FEAR FEELS LIKE.\nBECAUSE I'M SCARED TOO." },

    // ALPHYS — nervous
    { "U-um...", "U-um...\nI-I know you're there.\nPlease don't make me regret talking to you." },

    // ASGORE — grim / disappointed
    { "Greetings. I am ASGORE DREEMURR, King of the Monsters.", "Greetings...\nI am ASGORE DREEMURR.\nI had hoped you would be kinder than this." },

    // Generic lines that are common enough to safely make more ominous.
    { "You feel your sins crawling on your back.", "You feel the weight of every choice crawling behind you." },
    { "Determination.", "Determination...\nEven when everyone else is afraid." }
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
    ScriptMessage("HORROR TEXT: no matching dialogue was found. Nothing was changed.");
    return;
}

importGroup.Import();

ScriptMessage($"HORROR TEXT installed: {changedPhrases} dialogue replacements queued across {changedCodes} code entries.");
ScriptMessage("Player movement and Step events were not modified.");
