# UNDERTALE: FLOWEY // LAST PETAL

A horror-focused Undertale mod project built as an **UndertaleModTool script patch**.

## What this adds

- Flowey becomes the center of a slow-burn horror storyline.
- Flowey dialogue is replaced/augmented with unsettling fourth-wall-aware lines when matching dialogue code is found.
- A persistent horror state is stored in a global variable.
- Reloading and repeated encounters can increase the horror level.
- The patch is intentionally distributed as source/scripts rather than shipping Undertale's original `data.win`.

## Requirements

1. A legitimate PC copy of Undertale.
2. A current build of [UndertaleModTool](https://github.com/UnderminersTeam/UndertaleModTool).
3. A backup of your original `data.win`.

## Install

1. Open your Undertale `data.win` in UndertaleModTool.
2. Run `scripts/FloweyLastPetal.csx` from the Scripts menu.
3. Save the modified game as a new `data.win`.
4. Keep the original file backed up so you can restore it.

## Important

Undertale has multiple PC releases/builds. Internal resource names can differ between versions, so the patch searches resources rather than assuming one exact build. If no Flowey dialogue resource is found, the script stops without overwriting the game.

This repository does **not** include Undertale's original assets or executable.

## Horror design

The mod avoids a route-selection menu. Flowey's behavior becomes more disturbing based on repeated encounters and reload behavior. The intended tone is psychological horror: quiet changes, altered dialogue, wrong-looking rooms, and the feeling that Flowey remembers more than he should.
