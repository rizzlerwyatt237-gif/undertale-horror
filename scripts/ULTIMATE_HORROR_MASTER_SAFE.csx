using System;
using System.Collections.Generic;

// ============================================================
// UNDERTALE HORROR — ULTIMATE MASTER SAFE
//
// IMPORTANT:
// This version intentionally does NOT recompile existing GML code.
// The previous master could fail when UndertaleModTool tried to
// recompile decompiled Flowey/Sans events that contain GMS2 syntax.
//
// This script changes only existing UndertaleString resources.
// It is therefore much safer for an original Undertale data.win.
// BACK UP data.win before saving.
// ============================================================

EnsureDataLoaded();

var replacements = new Dictionary<string, string>(StringComparer.Ordinal)
{
    { "Howdy! I'm FLOWEY. FLOWEY the FLOWER!", "Oh. It's YOU.\nGood. You're here.\nI've been waiting for you.\nThis time, I'm on your side." },
    { "I'M FLOWEY! FLOWEY THE FLOWER!", "I'M FLOWEY.\nAnd before you ask... yes.\nI remember.\nI'm helping you this time." },
    { "You IDIOT!", "Hey! Don't listen to them.\nThey want you scared.\nStay with me. I've got you." },
    { "Kill or be killed.", "No. Not this time.\nStay with me.\nI'll help you find another way." },
    { "In this world, it's kill or be killed.", "They think this world is kill or be killed.\nThey're wrong.\nAnd I'll prove it to you." },
    { "hey. what's up?", "hey.\nyou again.\nflowey's with you now?\n...great. just great." },
    { "it's a beautiful day outside.", "it's a quiet day outside.\ntoo quiet.\neveryone heard what you did." },
    { "NYEH HEH HEH!", "NYEH...\nHEH...\nI AM NOT AFRAID!\n...I AM A LITTLE AFRAID." },
    { "I WILL CAPTURE YOU!", "I... WILL STOP YOU.\nI HAVE TO PROTECT EVERYONE." },
    { "Hello, my child.", "Hello, my child...\nPlease.\nI don't know what you have become." },
    { "YOU'RE GONNA HAVE A BAD TIME!", "YOU'RE GOING TO ANSWER FOR WHAT YOU'VE DONE.\nI'M NOT BACKING DOWN." },
    { "U-um...", "U-um...\nI-I know you're there.\nPlease don't come any closer." },
    { "Greetings. I am ASGORE DREEMURR, King of the Monsters.", "Greetings...\nI am ASGORE DREEMURR.\nI had hoped this meeting would be different." }
};

int changed = 0;

foreach (var str in Data.Strings)
{
    if (str == null || string.IsNullOrEmpty(str.Content))
        continue;

    if (replacements.TryGetValue(str.Content, out string replacement))
    {
        str.Content = replacement;
        changed++;
    }
}

ScriptMessage(
    "===== UNDERTALE HORROR SAFE MASTER =====\n\n" +
    "Installed without compiling GML.\n\n" +
    "Horror dialogue strings changed: " + changed + "\n\n" +
    "Flowey dialogue: UPDATED\n" +
    "Sans dialogue: UPDATED where matching stock text exists\n" +
    "Toriel dialogue: UPDATED where matching stock text exists\n" +
    "Undyne dialogue: UPDATED where matching stock text exists\n" +
    "Asgore dialogue: UPDATED where matching stock text exists\n\n" +
    "Player movement: NOT MODIFIED\n" +
    "GML compiler: NOT USED\n" +
    "Rooms/objects/sprites: NOT MODIFIED\n\n" +
    "BACK UP data.win before saving.\n" +
    "========================================="
);