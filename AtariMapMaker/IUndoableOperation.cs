namespace AtariMapMaker
{
    public interface IUndoableOperation
    {
        void Undo(AtariMap map);
        void Redo(AtariMap map);
        string Description { get; }
    }
}
