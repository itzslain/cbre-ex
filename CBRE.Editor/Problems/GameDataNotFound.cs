using CBRE.DataStructures.MapObjects;
using CBRE.Editor.Actions;
using CBRE.Editor.Actions.MapObjects.Operations;
using System.Collections.Generic;
using System.Linq;

namespace CBRE.Editor.Problems
{
    public class GameDataNotFound : IProblemCheck
    {
        public IEnumerable<Problem> Check(Map map, bool visibleOnly)
        {
            foreach (var entity in map.WorldSpawn
                .Find(x => x is Entity && (!visibleOnly || (!x.IsVisgroupHidden && !x.IsCodeHidden)))
                .OfType<Entity>()
                .Where(x => x.GameData == null))
            {
                yield return new Problem(GetType(), map, new[] { entity }, Fix, "Entity class not found: " + entity.EntityData.Name, "This entity class was not found in the current game data. Ensure that the correct FGDs are loaded. Fixing the problem will delete the entities.");
            }
        }

        public IAction Fix(Problem problem)
        {
            return new Delete(problem.Objects.Select(x => x.ID));
        }
    }
}