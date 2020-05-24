using System.Collections.Generic;
using CBRE.DataStructures.MapObjects;
using CBRE.Editor.Actions;

namespace CBRE.Editor.Problems
{
    public interface IProblemCheck
    {
        IEnumerable<Problem> Check(Map map, bool visibleOnly);
        IAction Fix(Problem problem);
    }
}
