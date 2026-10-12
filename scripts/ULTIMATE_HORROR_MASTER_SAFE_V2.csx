using System;
using System.Collections.Generic;
using UndertaleModLib.Models;

// UNDERTALE HORROR SAFE MASTER V2
// String-only patch. Does NOT compile or replace GML.
// It tolerates extra formatting/control text around dialogue.
// BACK UP data.win before running.

EnsureDataLoaded();

int changed = 0;
int flowey = 0;
int sans = 0;
int toriel = 0;
int undyne = 0;
int asgore = 0;

// Only replace distinctive dialogue fragments. We deliberately do not
// replace every string in the game, because many strings are internal data.
var replacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    { "Howdy! I'm FLOWEY", "Oh. It's YOU.\\nGood. You're here.\\nI've been waiting for you.\\nThis time, I'm on your side." },
    { "FLOWEY the FLOWER", "FLOWEY...\\nSomething is very wrong here.\\nStay close to me." },
    { "I'M FLOWEY", "I'M FLOWEY.\\nAnd yes... I remember.\\nI'm helping you this time." },
    { "Kill or be killed.", "No. Not this time.\\nThere has to be another way." },
    { "In this world, it's kill or be killed.", "They think this world is kill or be killed.\\nThey're wrong." },
    { "YOU'RE GONNA HAVE A BAD TIME!", "YOU'RE GOING TO ANSWER FOR WHAT YOU'VE DONE.\\nI'm not backing down." },
    { "NYEH HEH HEH!", "NYEH... HEH...\\nI am not afraid.\\n...Okay, maybe a little." },
    { "I WILL CAPTURE YOU!", "I... WILL STOP YOU.\\nI have to protect everyone." },
    { "Hello, my child.", "Hello, my child...\\nPlease.\\nI don't know what you have become." },
    { "Greetings. I am ASGORE DREEMURR", "Greetings...\\nI am ASGORE DREEMURR.\\nI had hoped this meeting would be different." },
    { "hey. what's up?", "hey. what's up?\\n...You look different." }
};

foreach (UndertaleString str in Data.Strings)
{
    if (str == null || string.IsNullOrEmpty(str.Content))
        continue;

    string original = str.Content;
    string lower = original.ToLowerInvariant();
    bool hit = false;

    foreach (var pair in replacements)
    {
        if (!lower.Contains(pair.Key.ToLowerInvariant()))
            continue;

        // Do not repeatedly patch our own text.
        if (original.Contains("I'm helping you this time.") ||
            original.Contains("This time, I'm on your side.") ||
            original.Contains("I'm not backing down."))
            break;

        str.Content = pair.Value.Replace("\\n", "\n");
        hit = true;
        changed++;
        break;
    }

    if (!hit)
        continue;

    if (lower.Contains("flowey")) flowey++;
    if (lower.Contains("bad time")) sans++;
    if (lower.Contains("my child")) toriel++;
    if (lower.Contains("capture you")) undyne++;
    if (lower.Contains("asgore dreemurr")) asgore++;
}

ScriptMessage(
    "===== UNDERTALE HORROR SAFE MASTER V2 =====\n\n" +
    "Installed WITHOUT compiling GML.\n\n" +
    "Dialogue strings changed: " + changed + "\n" +
    "Flowey matches: " + flowey + "\n" +
    "Sans matches: " + sans + "\n" +
    "Toriel matches: " + toriel + "\n" +
    "Undyne matches: " + undyne + "\n" +
    "Asgore matches: " + asgore + "\n\n" +
    "Player movement: NOT MODIFIED\n" +
    "GML compiler: NOT USED\n" +
    "Rooms/objects/sprites: NOT MODIFIED\n\n" +
    "If the total is still 0, this Undertale build uses different dialogue strings.\n" +
    "=========================================="
);