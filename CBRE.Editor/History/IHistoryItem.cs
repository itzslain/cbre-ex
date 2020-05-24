using System;
using System.Text;
using CBRE.DataStructures.MapObjects;
using CBRE.Editor.Documents;

namespace CBRE.Editor.History
{
    public interface IHistoryItem : IDisposable
    {
        string Name { get; }
        bool SkipInStack { get; }
        bool ModifiesState { get; }
        void Undo(Document document);
        void Redo(Document document);
    }
}
