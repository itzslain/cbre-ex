using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sledge.Settings
{
    public class Directories
    {
        public static string TextureDir { get; set; }
        public static string ModelDir { get; set; }

        static Directories()
        {
            TextureDir = "";
            ModelDir = "";
        }
    }
}
