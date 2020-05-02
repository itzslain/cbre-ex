using Sledge.DataStructures.Geometric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sledge.Editor.Compiling.Lightmap
{
    public struct Progress
    {
        public readonly string Message;
        public readonly float Amount;

        public Progress(string msg, float amount)
        {
            Message = msg; Amount = amount;
        }
    }
}
