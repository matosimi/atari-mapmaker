# Atari Map Maker

A Windows desktop tool for creating and editing character and tile-based maps for Atari 8-bit (ANTIC) graphics. Maps can use multiple fonts per screen, DLI (Display List Interrupt) colors, screen metadata, and tilemaps with linked submaps. Save and load maps in the `.atrmap` JSON format.

**Author:** Martin Simeček  
**UI font:** Segoe UI

---

## Lasermania (branch `lasermania`)

This branch adds support for designing and testing **Lasermania** levels (tilemap-based) directly from the editor.

Lasermania is an Atari XL/XE game published by Avalon in 1990, revamped by me and PG in 2020.

### Lasermania panel

- **Run Lasermania level** (menu or panel) – Validates the current screen and, if valid, builds a level payload, merges it with `Lasermania_for_kit.xex`, saves `lm_test.xex`, and runs the emulator from **Lasermania Setup**.
- **Lasermania Setup** – Dialog to set **emulator path** (e.g. Altirra) and **additional options**. Values are stored in `lasermania.cfg` in the application directory (two lines: path, then options). The emulator is launched with `lm_test.xex` when you run a level.

### Level validation (before run)

The current screen is validated; run is allowed only if all of the following hold:

- **Tiles:** Exactly one tile **0x10** (laser), one **0x30** (end); 0..3 tiles **0x23** (key) and the same number of **0x11** (gate); 0..3 **beam_in** + **beam_out** metadata and matching count of tile **0x26**; at least one **0x35** (sensor) or **0x21** (memory capsule).
- **Metadata:** Exactly one **END**, one **LASER** (direction 0..7 in Value), one **start**; 0..3 **key** and **gate** (same count); 0..3 **beam_in** and **beam_out** (same count). For each value 0..3 there is at most one key, one gate, one beam_in, one beam_out, and key count must equal gate count (and beam_in = beam_out) per value.
- **Position match:** LASER metadata at 0x10 tile position; END at 0x30; each key at 0x23; each gate at 0x11; each beam_in/beam_out at 0x26.

### Metadata for Lasermania (tilemaps)

- When editing metadata on a **tilemap**, the **Type** field is a **dropdown** with only valid Lasermania types: END, LASER, start, key, gate, beam_in, beam_out, color accent, memory capsule, sensor. **Color** is fixed per type (no color picker).
- **Laser direction indicator:** When the metadata layer is visible, each **LASER** metadata item shows a short **diagonal line** (1 char) in the laser’s color, indicating emission direction (Value 0..7: clockwise from top-left, `\` or `/` from the center of the top/right/bottom/left edge).

### Other Lasermania-related behaviour

- **Flip Screen Horizontal/Vertical** (context menu) now works correctly for **tilemaps**: tile data is flipped and the display buffer (CharData) for that screen is refreshed; metadata is unchanged.
- **Run** builds the level as: screen tiles (row-major) + 4 key positions (by value 0..3) + 4 gate + 4 beam_in + 4 beam_out + laser position + laser direction (0..7) + start position + exit position + color accent. This is appended after `Lasermania_for_kit.xex` and the fixed suffix; the result is saved as `lm_test.xex` and the emulator is started. Place `Lasermania_for_kit.xex` in the application directory; use **Lasermania Setup** to set the emulator path in `lasermania.cfg`.

---

## Overview

- **Character maps:** Grid of character codes (0–255) with optional multi-font and per-line DLI colors.
- **Tilemaps:** Map of tile indexes that reference a linked submap; each “screen” is a grid of tiles that expand to character data for display and export.
- **Clipboard:** Copy/paste rectangular regions (characters or tiles). Optional “Skip 0” and “Inverse” for character data.
- **Element library:** Save selections as named elements and paste them back (tilemaps and character maps).
- **Metadata layer:** Attach metadata items (text, value, color) to character or tile cells per screen; export or mass-edit.
- **Undo/Redo:** Supported for map edits.

---

## Map Area – Mouse Controls

### Left button

| Action | Description |
|--------|-------------|
| **Click (no selection)** | Start a **selection** from this point. |
| **Drag** | Adjust selection rectangle. |
| **Release** | End selection; the region is copied to the **clipboard** (shown in the Clipboard panel). |
| **Click (with clipboard)** | **Paste** clipboard at the clicked position. Snaps to character or tile grid. |
| **Click + hold Ctrl** | **Continuous paste:** paste at cursor, then keep pasting as you move the mouse (release Ctrl to stop). |
| **Click (metadata layer on)** | On a **metadata cell:** **Ctrl+Click** = copy metadata item to clipboard; **Click** (with copied item) = paste; **Click** (no copy) = add or edit metadata (dialog). |

### Right button

| Action | Description |
|--------|-------------|
| **Click** | If “Edit DLI” is on: show **DLI form** for the screen under cursor. Otherwise hide DLI form. If a screen was locked, unlock it. |
| **Alt + Right-click** | **Lock** the screen under cursor (yellow “L” corners). Locked screen is used for font mapping reference and keeps DLI/form focus on that screen. Right-click again (without Alt) to unlock; or Alt+Right-click same screen to unlock. |
| **Double-click** | **Cancel** current clipboard (clear paste preview). If a metadata item was copied, clear it. Same as pressing **Esc**. |

### Middle button

- **Click** = Open **context menu** (same as right-click context menu for the map).

### Context menu (right-click or middle-click)

- **Clear screen** – Fill current screen with character 0 (or tile 0 on tilemaps).
- **Flip Screen Horizontal / Vertical** – Flip the current screen.
- **Link to Screen...** – Define a screen link (overlay) from current screen to another.
- **Screen Metadata...** – Open metadata list for this screen (add/edit/delete items).
- **Export metadata...** – Export metadata for this screen.
- **Screen Description...** – Edit text description for this screen.
- **Apply Font Template...** – Apply a font template to this screen.
- **Undo** (Ctrl+Z)
- **Redo** (Ctrl+Y)

---

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| **Ctrl+Z** | Undo |
| **Ctrl+Y** | Redo |
| **I** | Invert clipboard (when clipboard is valid and inverse is available). |
| **Esc** | Cancel clipboard (clear paste preview and exit paste mode). If a metadata item was copied, clear it. |

---

## Panels and Tabs

### Zoom & view

- **Zoom** – Slider to change character/tile size (zoom level).
- **Screen borders** – Toggle red lines between screens.
- **Grid** – Toggle character/tile grid.

### Colors

- **General Colors** – COLPF0–COLPF3, COLBAK (and ALPA if used). Double-click a color to open the color picker and change the shared palette for the font/map.

### Clipboard

- **Clipboard** – Shows the current selection (characters or tiles). Click on the map to paste.
- **Skip 0** – When pasting character data, do not overwrite cells that are 0.
- **Inverse** – Invert clipboard content (e.g. swap character codes). Button enabled when clipboard is valid; shortcut **I**.

### DLI

- **Show DLI** – Show DLI color editor overlay when editing a screen.
- **Edit DLI** – Enable right-click to open DLI form for the screen under cursor.
- **Enable Multi-Font** – Use multiple font slots per map; font mapping is per screen/line.
- **Reference Font Mapping** – Use a reference screen (X,Y) for font mapping; set the reference screen with the numeric controls.

### Metadata

- **Show/Edit metadata** – Toggle metadata layer. When on, you can add/edit/paste metadata items on the map; character editing is disabled.
- **Show texts** – Toggle visibility of metadata text on the map.
- **Mass change metadata...** – Bulk edit or filter metadata across the map.
- Title shows **Metadata N** where N is the total number of metadata items (all screens).

### Font

- **Show font** – Open the **Font / character picker** window (pick character code and see font).
- **Load Font** – Load a font file into the default slot.
- **Export Font** – Export current font.
- **Show Tiles** – Open the **Tile picker** (for tilemaps; pick tile index from the submap).
- **Font Templates** – Manage and apply font templates.
- **Element Library** – Open the **Element Library** (save selection as element, paste from element).
- **Map Description** – Edit map-wide description.
- **Tilemap Config** – For tilemaps: set submap path, tile numbering, **Show Byte Overlay** (hex tile index on each tile), and overlay transparency.

---

## Tilemaps

1. Create a **new map** with **Tilemap** checked; set map and screen size (in tiles). You must set the **submap** (character map that defines each tile) in **Tilemap Config** before the map is fully usable.
2. **Tilemap Config** – Set **Submap File** (linked character map), optional **Show Byte Overlay** (hex index on each tile) and **Overlay Transparency**.
3. **Show Tiles** – Opens the tile picker: choose a tile index to draw. Drawing on the map places that tile index; the visible character data is generated from the submap.
4. **Clipboard** – Selection can copy either **character data** or **tile indexes** (e.g. when copying from the tile picker or element library). Paste places characters or tiles depending on clipboard type; the UI shows which mode is active.
5. **Element Library** – For tilemaps you can save and paste tile-index elements; paste uses tile indexes.

---

## Metadata Layer

1. Enable **Show/Edit metadata** in the Metadata group.
2. **Add:** Click an empty cell → dialog to set position, text, value, color. **Edit/remove:** Click a cell that has metadata → dialog (option to remove).
3. **Copy:** **Ctrl+Click** on a cell that has a metadata item → copies that item.
4. **Paste:** Click another cell (with no modifier) → pastes the copied item there.
5. **Cancel:** **Esc** or **right double-click** clears the copied metadata item.
6. **Screen Metadata...** (context menu) – List all metadata items for the current screen; add, edit, delete, export.
7. **Mass change metadata...** – Filter by screen/position/value/text and change or delete in bulk.

---

## Load / Save / Export

- **Load Map** – Load a map from a `.atrmap` file (replaces current map).
- **Save Map** – Save the current map to a `.atrmap` file.
- **Export** – Export a rectangular region (screen range + optional extra chars) as raw data (e.g. for assembly).
- **Import** – Import raw data into a region.
- **Column Export / Column Import** – Per-column export/import.
- **Export DLI** – Export DLI color data.
- **Show Screen Selection** – Highlight the export region on the map (From/To X,Y).

---

## New Map

- **Map size** – Width and height in **screens**.
- **Screen size** – Width and height in **characters** (or in **tiles** for a tilemap).
- **Tilemap** – Check to create a tilemap; then configure the submap in **Tilemap Config**.
- **Create new map** – Creates the map; for tilemaps you still need to set the submap to get correct display.

---

## Data Manipulation

- **Extend map with additional row** – Add one row of screens at the bottom; keeps fonts, metadata, DLI, and tilemap data.
- **Replace chars** – Replace one character (or tile) code with another over the whole map or current screen.
- **Shift chars** – Utility to shift character ranges (e.g. 64–79 → 80–95, 32–47 → 64–79).

---

## Status Bar

- Shows **current screen**, **position** (tile X,Y and char X,Y for tilemaps), **character code** (and **tile index** for tilemaps), and **occurrence** (e.g. “3 of 7” = this is the 3rd occurrence of that character/tile on the screen, 7 total).

---

## File Format

- Maps are saved as **`.atrmap`** (JSON). The format supports:
  - Map and screen size, character or tile data
  - Multiple fonts, font line mapping per screen, font mapping references
  - DLI (color) data, screen descriptions, map description
  - Metadata (per-screen metadata items)
  - Tilemap: submap path, tilemap options (e.g. byte overlay), element library, screen links

---

## Tips

- **Lock a screen** with **Alt+Right-click** so the reference screen and DLI/form focus stay on that screen while you scroll.
- Use **Ctrl+hold and move** for **continuous paste** when filling large areas.
- For tilemaps, use **Tilemap Config → Show Byte Overlay** to see hex tile indexes on each tile; adjust transparency for readability.
- **Skip 0** is useful when pasting character data over existing graphics without erasing background (0).
- **I** inverts the clipboard (e.g. for inverting a character set or pattern).
