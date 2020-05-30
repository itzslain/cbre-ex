using CBRE.DataStructures.MapObjects;
using CBRE.Editor.Documents;
using CBRE.Providers.Model;
using OpenTK;
using System.Collections.Generic;
using System.Linq;

namespace CBRE.Editor.Compiling
{
    class FBXExport
    {
        public static void SaveToFile(string filename,Document document,ExportForm form)
        {
            AssimpProvider.SaveToFile(filename, document.Map);
        }
    }
}
