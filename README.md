# Rimworld.Ribbons (Roebuck's Rimworld Ribbons)
A C# library demonstrating how to inject new iTabs into RimWorld

This is a simple RimWorld mod demonstrating **ITab injection** for pawns **without needing Harmony or HugsLib** for runtime patching. I havem't found any tutorials for handling the injection of this sort of thing without the use of either of those libraries, so I'm posting this here in the hopes it saves someone all the time it took me to figure it out.

This specifically adds a new inspection tab that displays **ribbons, medals, or awards** that a pawn has in their inventory.


---

## Features

- Adds a custom tab to colonist pawns’ inspect panes.
- Dynamically displays all ribbon-type items in the pawn's inventory.
- Supports:
  - Multiple rows of icons.
  - Variable icon sizes.
  - Tooltips and highlight on hover.
- Calculates **horizontal and vertical centering** automatically.
- No Harmony patches or external libraries required. Base RimWorld functionality.

---

## How It Works

1. **RibbonExtension (`DefModExtension`)**  
   - Attach this to any `ThingDef` XML to mark it as a ribbon/medal.  
   - Stores:
     - `awardType` – type of the ribbon.
     - `awardCount` – how many awards this item counts as.
     - `iconPath` – optional custom icon.
   - Lazily loads icons via `ContentFinder<Texture2D>`.

2. **ITab_Pawn_Medals (`ITab`)**  
   - Injected into colonist pawns automatically by RimWorld when the tab is defined.
   - Pulls all items with `RibbonExtension` from the pawn’s inventory.
   - Calculates layout dynamically:
     - Icons per row.
     - Row widths for horizontal centering.
     - Vertical centering for multiple rows.
   - Draws icons, highlights, and tooltips on hover.

3. **Vanilla ITab Injection**  
   - Adds a new pawn inspection tab using only RimWorld’s built-in `ITab` system.   
   - Lightweight, simple, and should be fully compatible with most other mods.
---


Hope this helps!
