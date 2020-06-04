namespace CBRE.Settings
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
