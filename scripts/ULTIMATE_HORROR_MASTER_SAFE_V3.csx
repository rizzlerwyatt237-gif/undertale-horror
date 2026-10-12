using System;
using System.Collections.Generic;
using System.Linq;
using UndertaleModLib.Models;

// UNDERTALE HORROR SAFE MASTER V3
// BIG string + visual corruption pass.
// IMPORTANT: this script does NOT compile or replace GML.
// It edits existing string resources and existing object->sprite links only.
// BACK UP data.win before running.

EnsureDataLoaded();

int dialogueChanged = 0;
int visualChanged = 0;
int glitchCandidates = 0;
int flowey = 0, sans = 0, toriel = 0, undyne = 0, asgore = 0;

// ---------- DIALOGUE ----------
// These are fragments rather than exact full lines so they still work when
// the game stores control codes or formatting around the text.
var replacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    { "Howdy! I'm FLOWEY", "Oh. It's YOU.\nGood. You're here.\nI've been waiting for you.\nThis time, I'm on your side." },
    { "FLOWEY the FLOWER", "FLOWEY...\nSomething is very wrong here.\nStay close to me." },
    { "I'M FLOWEY", "I'M FLOWEY.\nAnd yes... I remember.\nI'm helping you this time." },
    { "Kill or be killed.", "No. Not this time.\nThere has to be another way." },
    { "In this world, it's kill or be killed.", "They think this world is kill or be killed.\nThey're wrong.\nI've seen what happens next." },
    { "YOU'RE GONNA HAVE A BAD TIME!", "YOU'RE GOING TO ANSWER FOR WHAT YOU'VE DONE.\nI'm not backing down." },
    { "NYEH HEH HEH!", "NYEH... HEH...\nI am not afraid.\n...Okay, maybe a little." },
    { "I WILL CAPTURE YOU!", "I... WILL STOP YOU.\nI have to protect everyone." },
    { "Hello, my child.", "Hello, my child...\nPlease.\nI don't know what you have become." },
    { "Greetings. I am ASGORE DREEMURR", "Greetings...\nI am ASGORE DREEMURR.\nI had hoped this meeting would be different." },
    { "hey. what's up?", "hey. what's up?\n...You look different." },
    { "It's a beautiful day outside.", "It's a quiet day outside.\nToo quiet.\nSomething is watching." },
    { "The air is filled with determination.", "The air is filled with determination.\nAnd something else.\nSomething that remembers." },
    { "You feel like you're going to have a bad time.", "You feel like something is wrong.\nVery wrong.\nThe world feels like it remembers you." },
    { "* You are filled with determination.", "* You are filled with determination.\n* The SAVE point flickers.\n* Something noticed." }
};

foreach (UndertaleString str in Data.Strings)
{
    if (str == null || string.IsNullOrEmpty(str.Content))
        continue;

    string original = str.Content;
    string lower = original.ToLowerInvariant();
    bool changed = false;

    foreach (var pair in replacements)
    {
        if (!lower.Contains(pair.Key.ToLowerInvariant()))
            continue;

        // Avoid stacking this script on itself.
        if (original.Contains("Something is very wrong here.") ||
            original.Contains("I'm helping you this time.") ||
            original.Contains("The SAVE point flickers.") ||
            original.Contains("The world feels like it remembers you."))
            break;

        str.Content = pair.Value;
        dialogueChanged++;
        changed = true;
        break;
    }

    if (!changed)
        continue;

    if (lower.Contains("flowey")) flowey++;
    if (lower.Contains("bad time")) sans++;
    if (lower.Contains("my child")) toriel++;
    if (lower.Contains("capture you")) undyne++;
    if (lower.Contains("asgore dreemurr")) asgore++;
}

// ---------- BROADER DIALOGUE FALLBACK ----------
// Some Undertale builds store dialogue in strings that don't contain the
// famous stock lines above. Add a small, safe glitch marker to likely prose.
// We skip identifiers, paths, file names, menu/control strings, and anything
// already changed by this script.
string[] skipWords =
{
    "options", "settings", "fullscreen", "windowed", "keyboard", "controller",
    "continue", "quit", "reset", "load", "save", "language", "version",
    "copyright", "undertale", "www.", "http", "png", "ogg", "wav", "gml_",
    "obj_", "spr_", "room_", "scr_", "snd_"
};

foreach (UndertaleString str in Data.Strings)
{
    if (str == null || string.IsNullOrWhiteSpace(str.Content))
        continue;

    string s = str.Content;
    string t = s.Trim();

    if (t.Length < 14 || !t.Contains(" "))
        continue;
    if (t.Contains("[HORROR_GLITCH]"))
        continue;
    if (t.Count(c => char.IsLetter(c)) < 5)
        continue;

    bool skip = false;
    string low = t.ToLowerInvariant();
    foreach (string word in skipWords)
    {
        if (low.Contains(word))
        {
            skip = true;
            break;
        }
    }
    if (skip)
        continue;

    // Only alter prose-like strings. This intentionally avoids code-ish text.
    bool prose = t.Contains(".") || t.Contains("!") || t.Contains("?") || t.Contains("*") || t.Contains(",");
    if (!prose)
        continue;

    str.Content = "[HORROR_GLITCH] " + s;
    glitchCandidates++;
}

// ---------- VISUAL CORRUPTION ----------
// UMT exposes each existing GameMaker object's Sprite property, so we can
// make persistent visual mistakes without recompiling GML.
// Prefer a same-character sprite whose name suggests hurt/dead/attack/etc.
// If one exists, use it. Otherwise leave the object alone.
var characterWords = new[] { "flowey", "sans", "toriel", "undyne", "asgore", "papyrus", "frisk", "chara" };
var glitchWords = new[] { "dead", "death", "hurt", "damage", "damaged", "defeat", "attack", "hit", "die", "down", "fall", "sad" };

foreach (UndertaleGameObject obj in Data.GameObjects)
{
    if (obj == null || obj.Name == null || obj.Name.Content == null)
        continue;
    if (obj.Sprite == null || obj.Sprite.Name == null || obj.Sprite.Name.Content == null)
        continue;

    string objName = obj.Name.Content;
    string objLow = objName.ToLowerInvariant();
    string currentSprite = obj.Sprite.Name.Content;
    string currentLow = currentSprite.ToLowerInvariant();

    string character = null;
    foreach (string c in characterWords)
    {
        if (objLow.Contains(c))
        {
            character = c;
            break;
        }
    }
    if (character == null)
        continue;

    // Don't repeatedly change the same object when V3 is run twice.
    if (objLow.Contains("horror_glitch"))
        continue;

    var candidates = Data.Sprites
        .Where(s => s != null && s.Name != null && s.Name.Content != null)
        .Where(s => s != obj.Sprite)
        .Where(s => s.Name.Content.IndexOf(character, StringComparison.OrdinalIgnoreCase) >= 0)
        .Where(s => glitchWords.Any(g => s.Name.Content.IndexOf(g, StringComparison.OrdinalIgnoreCase) >= 0))
        .OrderBy(s => s.Name.Content.Length)
        .ToList();

    if (candidates.Count == 0)
        continue;

    UndertaleSprite chosen = candidates[0];
    obj.Sprite = chosen;
    visualChanged++;
}

// ---------- REPORT ----------
ScriptMessage(
    "===== UNDERTALE HORROR SAFE MASTER V3 =====\n\n" +
    "Dialogue replacements: " + dialogueChanged + "\n" +
    "Broad horror/glitch text markers: " + glitchCandidates + "\n\n" +
    "Flowey targeted: " + flowey + "\n" +
    "Sans targeted: " + sans + "\n" +
    "Toriel targeted: " + toriel + "\n" +
    "Undyne targeted: " + undyne + "\n" +
    "Asgore targeted: " + asgore + "\n\n" +
    "Visual character sprite glitches: " + visualChanged + "\n\n" +
    "GML compiler: NOT USED\n" +
    "Player movement: NOT MODIFIED\n" +
    "Rooms: NOT MODIFIED\n\n" +
    "SAVE A BACKUP BEFORE SAVING THIS DATA.WIN.\n" +
    "Then test the game. If dialogue is STILL unchanged, the next step is to identify the actual text table used by this build instead of guessing.\n" +
    "============================================"
);