using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using CBRE.Editor.Properties;
using System.IO;

namespace CBRE.Editor
{
    public static class CBRECursors
    {
        static CBRECursors()
        {
            RotateCursor = new Cursor(new MemoryStream(Resources.Cursor_Rotate));
        }

        public static Cursor RotateCursor;
    }
}
