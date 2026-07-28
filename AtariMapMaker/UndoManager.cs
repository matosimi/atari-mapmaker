using System;
using System.Collections.Generic;

namespace AtariMapMaker
{
    public class UndoManager
    {
        private const int MAX_UNDO_STEPS = 5;
        private readonly List<IUndoableOperation> undoStack;
        private readonly List<IUndoableOperation> redoStack;
        private AtariMap map;

        public UndoManager(AtariMap map)
        {
            this.map = map;
            undoStack = new List<IUndoableOperation>(MAX_UNDO_STEPS);
            redoStack = new List<IUndoableOperation>(MAX_UNDO_STEPS);
        }

        public void SetMap(AtariMap map)
        {
            this.map = map;
            Clear();
        }

        public void PushOperation(IUndoableOperation operation)
        {
            if (operation == null)
                return;

            redoStack.Clear();
            undoStack.Add(operation);

            while (undoStack.Count > MAX_UNDO_STEPS)
                undoStack.RemoveAt(0);
        }

        public bool CanUndo()
        {
            return undoStack.Count > 0;
        }

        public bool CanRedo()
        {
            return redoStack.Count > 0;
        }

        public void Undo()
        {
            if (!CanUndo())
                return;

            IUndoableOperation operation = undoStack[undoStack.Count - 1];
            undoStack.RemoveAt(undoStack.Count - 1);
            operation.Undo(map);
            redoStack.Add(operation);
        }

        public void Redo()
        {
            if (!CanRedo())
                return;

            IUndoableOperation operation = redoStack[redoStack.Count - 1];
            redoStack.RemoveAt(redoStack.Count - 1);
            operation.Redo(map);
            undoStack.Add(operation);
        }

        public string GetUndoDescription()
        {
            if (!CanUndo())
                return "";
            return undoStack[undoStack.Count - 1].Description;
        }

        public string GetRedoDescription()
        {
            if (!CanRedo())
                return "";
            return redoStack[redoStack.Count - 1].Description;
        }

        public void Clear()
        {
            undoStack.Clear();
            redoStack.Clear();
        }
    }

    /// <summary>
    /// Restores a set of map Data[] cells (used for clipboard paste undo/redo).
    /// </summary>
    public class MapDataRegionOperation : IUndoableOperation
    {
        private readonly int[] indices;
        private readonly byte[] oldValues;
        private readonly byte[] newValues;

        public string Description { get; }

        public MapDataRegionOperation(int[] indices, byte[] oldValues, byte[] newValues, string description = "Paste")
        {
            this.indices = indices ?? throw new ArgumentNullException(nameof(indices));
            this.oldValues = oldValues ?? throw new ArgumentNullException(nameof(oldValues));
            this.newValues = newValues ?? throw new ArgumentNullException(nameof(newValues));
            if (indices.Length != oldValues.Length || indices.Length != newValues.Length)
                throw new ArgumentException("Change arrays must have the same length.");
            Description = description;
        }

        /// <summary>
        /// Builds an operation from before/after Data snapshots. Returns null if nothing changed.
        /// </summary>
        public static MapDataRegionOperation CreateFromDiff(byte[] before, byte[] after, string description = "Paste")
        {
            if (before == null || after == null)
                return null;

            int len = Math.Min(before.Length, after.Length);
            var indices = new List<int>();
            var oldVals = new List<byte>();
            var newVals = new List<byte>();

            for (int i = 0; i < len; i++)
            {
                if (before[i] != after[i])
                {
                    indices.Add(i);
                    oldVals.Add(before[i]);
                    newVals.Add(after[i]);
                }
            }

            if (indices.Count == 0)
                return null;

            return new MapDataRegionOperation(indices.ToArray(), oldVals.ToArray(), newVals.ToArray(), description);
        }

        public void Undo(AtariMap map)
        {
            Apply(map, oldValues);
        }

        public void Redo(AtariMap map)
        {
            Apply(map, newValues);
        }

        private void Apply(AtariMap map, byte[] values)
        {
            if (map?.Data == null)
                return;

            for (int i = 0; i < indices.Length; i++)
            {
                int index = indices[i];
                if (index >= 0 && index < map.Data.Length)
                    map.Data[index] = values[i];
            }

            if (map.IsTilemap)
                map.RegenerateCharDataFromTiles();
        }
    }

    // Concrete undo operation implementations (scaffolding for future use)
    public class MapDataChangeOperation : IUndoableOperation
    {
        private int index;
        private byte oldValue;
        private byte newValue;

        public string Description => "Character Change";

        public MapDataChangeOperation(int index, byte oldValue, byte newValue)
        {
            this.index = index;
            this.oldValue = oldValue;
            this.newValue = newValue;
        }

        public void Undo(AtariMap map)
        {
            if (map != null && index >= 0 && index < map.Data.Length)
                map.Data[index] = oldValue;
        }

        public void Redo(AtariMap map)
        {
            if (map != null && index >= 0 && index < map.Data.Length)
                map.Data[index] = newValue;
        }
    }

    public class DliColorChangeOperation : IUndoableOperation
    {
        private int screenX, screenY, line, colorNumber;
        private byte oldValue;
        private byte newValue;

        public string Description => "DLI Color Change";

        public DliColorChangeOperation(int screenX, int screenY, int line, int colorNumber, byte oldValue, byte newValue)
        {
            this.screenX = screenX;
            this.screenY = screenY;
            this.line = line;
            this.colorNumber = colorNumber;
            this.oldValue = oldValue;
            this.newValue = newValue;
        }

        public void Undo(AtariMap map)
        {
            if (map != null)
                map.SetDliColor(screenX, screenY, line, colorNumber, oldValue);
        }

        public void Redo(AtariMap map)
        {
            if (map != null)
                map.SetDliColor(screenX, screenY, line, colorNumber, newValue);
        }
    }

    public class FontChangeOperation : IUndoableOperation
    {
        private int line;
        private byte oldFontIndex;
        private byte newFontIndex;

        public string Description => "Font Change";

        public FontChangeOperation(int line, byte oldFontIndex, byte newFontIndex)
        {
            this.line = line;
            this.oldFontIndex = oldFontIndex;
            this.newFontIndex = newFontIndex;
        }

        public void Undo(AtariMap map)
        {
            if (map != null)
                map.SetFontForLine(line, oldFontIndex);
        }

        public void Redo(AtariMap map)
        {
            if (map != null)
                map.SetFontForLine(line, newFontIndex);
        }
    }
}
