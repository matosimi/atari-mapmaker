using System;
using System.Collections.Generic;

namespace AtariMapMaker
{
    public class UndoManager
    {
        private const int MAX_UNDO_STEPS = 50;
        private Stack<IUndoableOperation> undoStack;
        private Stack<IUndoableOperation> redoStack;
        private AtariMap map;

        public UndoManager(AtariMap map)
        {
            this.map = map;
            undoStack = new Stack<IUndoableOperation>(MAX_UNDO_STEPS);
            redoStack = new Stack<IUndoableOperation>(MAX_UNDO_STEPS);
        }

        public void PushOperation(IUndoableOperation operation)
        {
            if (operation == null)
                return;

            // Clear redo stack when new operation is pushed
            redoStack.Clear();

            // Limit undo stack size
            if (undoStack.Count >= MAX_UNDO_STEPS)
            {
                // Remove oldest operation (would need a different data structure for this)
                // For now, just limit to MAX_UNDO_STEPS
                Stack<IUndoableOperation> tempStack = new Stack<IUndoableOperation>();
                while (undoStack.Count > MAX_UNDO_STEPS - 1)
                    tempStack.Push(undoStack.Pop());
                undoStack.Clear();
                while (tempStack.Count > 0)
                    undoStack.Push(tempStack.Pop());
            }

            undoStack.Push(operation);
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

            IUndoableOperation operation = undoStack.Pop();
            operation.Undo(map);
            redoStack.Push(operation);
        }

        public void Redo()
        {
            if (!CanRedo())
                return;

            IUndoableOperation operation = redoStack.Pop();
            operation.Redo(map);
            undoStack.Push(operation);
        }

        public string GetUndoDescription()
        {
            if (!CanUndo())
                return "";
            return undoStack.Peek().Description;
        }

        public string GetRedoDescription()
        {
            if (!CanRedo())
                return "";
            return redoStack.Peek().Description;
        }

        public void Clear()
        {
            undoStack.Clear();
            redoStack.Clear();
        }
    }

    // Concrete undo operation implementations
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
