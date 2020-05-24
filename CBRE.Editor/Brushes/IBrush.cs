using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBRE.DataStructures.Geometric;
using CBRE.DataStructures.MapObjects;
using CBRE.Common;
using CBRE.Editor.Brushes.Controls;

namespace CBRE.Editor.Brushes
{
    public interface IBrush
    {
        string Name { get; }
        bool CanRound { get; }
        IEnumerable<BrushControl> GetControls();
        IEnumerable<MapObject> Create(IDGenerator generator, Box box, ITexture texture, int roundDecimals);
    }
}
