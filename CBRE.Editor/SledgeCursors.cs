using System.IO;
using System.Windows.Forms;
using CBRE.Editor.Properties;

namespace CBRE.Editor {
    public static class CBRECursors {
        static CBRECursors() {
            RotateCursor = new Cursor(new MemoryStream(Resources.Cursor_Rotate));
        }

        public static Cursor RotateCursor;
    }
}
