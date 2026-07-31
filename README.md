# Atari Map Maker

A Windows desktop tool for creating and editing character and tile-based maps for Atari 8-bit (ANTIC) graphics. Maps can use multiple fonts per screen, DLI (Display List Interrupt) colors, screen metadata, and tilemaps with linked submaps. Save and load maps in the `.atrmap` JSON format.

**Author:** Martin Simeček  
**UI font:** Segoe UI

---

## Overview

- **Character maps:** Grid of character codes (0–255) with optional multi-font and per-line DLI colors.
- **Tilemaps:** Map of tile indexes that reference a linked submap; each “screen” is a grid of tiles that expand to character data for display and export.
- **Clipboard:** Copy/paste rectangular regions (characters or tiles). Optional **Autopaste**, **Skip 0**, and **Inverse**. Supports **Ctrl+C** / **Ctrl+V**.
- **Element library:** Save selections as named elements and paste them back (tilemaps and character maps).
- **Metadata layer:** Attach typed metadata items (text, value, color) to character or tile cells per screen; link lines, move, export, or mass-edit.
- **Undo/Redo:** History depth of 5 (Colors tab **History** group, or Ctrl+Z / Ctrl+Y).

---

## Map Area – Mouse Controls

### Left button

| Action | Description |
|--------|-------------|
| **Click (no selection)** | Start a **selection** from this point. |
| **Drag** | Adjust selection rectangle. |
| **Release** | End selection. With **Autopaste** on: region is copied to the **clipboard**. With Autopaste off: selection stays until you press **Ctrl+C**. |
| **Click (with clipboard / floating paste)** | **Paste** at the clicked position (snaps to character or tile grid). Autopaste on: stay in paste mode. Autopaste off (after Ctrl+V): place once, then exit paste mode. |
| **Click + hold Ctrl** (Autopaste on) | **Continuous paste:** paste at cursor, then keep pasting as you move (release Ctrl to stop). |
| **Alt + Left-click** (metadata edit on) | **Grab and move** a metadata item; next click places it. |
| **Click (metadata edit on)** | On a **metadata cell:** **Ctrl+Click** = copy metadata item; **Click** (with copied item) = paste; **Click** (no copy) = add or edit metadata (dialog). |

### Right button

| Action | Description |
|--------|-------------|
| **Click** | If “Edit DLI” is on: show **DLI form** for the screen under cursor. Otherwise hide DLI form. If a screen was locked, unlock it. |
| **Alt + Right-click** | **Lock** the screen under cursor (yellow **Locked** label, drawn above other screen labels). Locked screen is used for font mapping reference and keeps DLI/form focus on that screen. Right-click again (without Alt) to unlock; or Alt+Right-click same screen to unlock. |
| **Double-click** | **Cancel** current clipboard (clear paste preview). If a metadata item was copied, clear it. Same as pressing **Esc**. |

### Middle button

- **Click** = Open **context menu** (same as right-click context menu for the map). Menu items that are active for the current screen show **checkmarks** (e.g. link, non-default options).

### Context menu (right-click or middle-click)

- **Clear screen** – Fill current screen with character 0 (or tile 0 on tilemaps).
- **Flip Screen Horizontal / Vertical** – Flip the current screen.
- **Link to Screen...** – Define a screen link (overlay) from current screen to another.
- **Screen Metadata...** – Open metadata list for this screen (add/edit/delete/copy/paste items).
- **Export metadata...** – Export metadata for this screen (settings are remembered).
- **Screen Description...** – Edit text description for this screen (first line shown above the screen).
- **Apply Font Template...** – Apply a font template to this screen.
- **Undo** (Ctrl+Z)
- **Redo** (Ctrl+Y)

### Screen labels (above / near each screen)

Hover or view screens show status labels (cyan unless noted):

- **Locked** (yellow) – screen is locked; drawn on top of other labels.
- Metadata present, custom DLI, screen link, first line of screen description.
- **Reference font mapping from screen X:Y** – when Reference Font Mapping is on and this screen uses another screen’s mapping.
- **Font mapping included** – screen has its own font line mapping.

---

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| **Ctrl+Z** | Undo (up to 5 steps). |
| **Ctrl+Y** | Redo. |
| **Ctrl+C** | Copy current selection to clipboard (**Autopaste off** only; with Autopaste on, selection copies automatically). |
| **Ctrl+V** | Arm / show floating paste. Autopaste on: enter paste mode. Autopaste off: arm one-shot floating paste until you click to place. |
| **I** | Invert clipboard (when clipboard is valid and inverse is available). |
| **Esc** | Cancel clipboard (clear paste preview and exit paste mode). If a metadata item was copied, clear it. |

---

## Panels and Tabs

### Zoom & view

- **Zoom** – Slider to change character/tile size (zoom level).
- **Screen borders** – Toggle red lines between screens.
- **Grid** – Toggle character/tile grid.

### Colors

- **General Colors** – COLPF0–COLPF3, COLBAK (and ALPA if used). Double-click a color to open the color picker. ALPA dual picker shows both colors with current and previous values.
- **History** – **Undo** / **Redo** buttons (depth 5); same as Ctrl+Z / Ctrl+Y.

### Clipboard

- **Clipboard** – Shows the current selection (characters or tiles). Click on the map to paste when paste mode is active.
- **Edit - Autopaste** – **On** (default): select copies clipboard; click or Ctrl+V pastes; Ctrl+drag continuous paste. **Off:** Ctrl+C copies; Ctrl+V arms floating paste; click places once.
- **Skip 0** – When pasting character data, do not overwrite cells that are 0.
- **Inverse** – Invert clipboard content (e.g. swap character codes). Button enabled when clipboard is valid; shortcut **I**.

### DLI

- **Show DLI** – Show DLI color editor overlay when editing a screen.
- **Edit DLI** – Enable right-click to open DLI form for the screen under cursor.
- **Enable Multi-Font** – Use multiple font slots per map; font mapping is per screen/line.
- **Reference Font Mapping** – Use a reference screen (X,Y) for font mapping; set the reference screen with the numeric controls. Screens show labels when they include or reference font mapping.

### Metadata

- **Show metadata** / **Edit metadata** – Separate toggles. Show displays the layer; Edit enables add/edit/paste (and disables char editing / some panels). Turning Edit on also turns Show on.
- **Show texts** – Toggle visibility of metadata text on the map.
- **Show color links** / **Show value links** – Draw lines connecting metadata items that share the same color or value (restricted to the **local screen**).
- **Map ↔ Meta** – Blend slider between character map and metadata overlay.
- **Mass change metadata...** – Bulk edit or filter metadata across the map (enabled when Edit metadata is on).
- Title shows **Metadata N** where N is the total number of metadata items (all screens).

### Font

- **Show font** – Open the **Font / character picker**. Can display the font with **DLI colors from the current screen** (not only global colors), so characters match on-screen appearance.
- **Load Font** – Load a font file into the default slot.
- **Export Font** – Export current font.
- **Show Tiles** – Open the **Tile picker** (for tilemaps; pick tile index from the submap).
- **Font Templates** – Manage and apply font templates.
- **Element Library** – Open the **Element Library** (save selection as element, paste from element).
- **Map Description** – Edit map-wide description (first line appears in the window title).
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

1. Enable **Show metadata** (and **Edit metadata** to change items).
2. **Add:** Click an empty cell → dialog (type, text, value, color; color suggestions available). **Edit/remove:** Click a cell that has metadata → dialog.
3. **Types:** Metadata items have a type byte with a map-wide label registry (shared labels per type).
4. **Copy:** **Ctrl+Click** on a cell that has a metadata item → copies that item. Also available in **Screen Metadata...**.
5. **Paste:** Click another cell (with no modifier) → pastes the copied item there.
6. **Move:** **Alt+Left-click** on a metadata item to grab it; click to place.
7. **Cancel:** **Esc** or **right double-click** clears the copied metadata item.
8. **Links:** Optionally show lines between items with the same value or color (per screen only).
9. **Screen Metadata...** (context menu) – List all metadata items for the current screen; add, edit, delete, copy/paste, export.
10. **Mass change metadata...** – Filter by screen/position/value/text/type and change or delete in bulk.
11. **Export metadata...** – Includes color values; form options are remembered between opens.

---

## Load / Save / Export

- **Load Map** – Load a map from a `.atrmap` file (replaces current map). Filename is remembered for subsequent save/export dialogs.
- **Save Map** – Save the current map to a `.atrmap` file.
- **Export** – Export a rectangular region (screen range + optional extra chars) as raw data (e.g. for assembly).
- **Single screen** – When checked, export/import uses one screen (From X,Y) instead of a From–To range.
- **Import** – Import raw data into a region.
- **Column Export / Column Import** – Per-column export/import.
- **Export DLI** – Export DLI color data (order: color register by register, all lines of each register). DLI mask and ALPA DLI mask fields control which registers are included.
- **Show Screen Selection** – Highlight the export region on the map (From/To X,Y).

Window title shows version, current `.atrmap` filename (if any), and the first line of the map description. **New Map → About** shows version, git commit hash, and commit date.

---

## New Map

- **Map size** – Width and height in **screens**.
- **Screen size** – Width and height in **characters** (or in **tiles** for a tilemap).
- **Tilemap** – Check to create a tilemap; then configure the submap in **Tilemap Config**.
- **ALPA** – Enable ALPA-related color/DLI behavior for the new map.
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
  - Metadata (per-screen metadata items) and metadata type labels
  - Tilemap: submap path, tilemap options (e.g. byte overlay), element library, screen links

---

## Tips

- **Lock a screen** with **Alt+Right-click** so the reference screen and DLI/form focus stay on that screen while you scroll; look for the yellow **Locked** label.
- Turn **Edit - Autopaste** off when you want explicit **Ctrl+C** / **Ctrl+V** (one-shot paste) instead of select-to-copy and click-to-paste.
- Use **Ctrl+hold and move** (Autopaste on) for **continuous paste** when filling large areas.
- For tilemaps, use **Tilemap Config → Show Byte Overlay** to see hex tile indexes on each tile; adjust transparency for readability.
- **Skip 0** is useful when pasting character data over existing graphics without erasing background (0).
- **I** inverts the clipboard (e.g. for inverting a character set or pattern).
- Adjust **Map ↔ Meta** blend when metadata overlays make the map hard to read.
- Use **Show color/value links** to visualize related metadata on a screen; links stay within that screen.
