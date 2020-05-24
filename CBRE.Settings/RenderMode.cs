using System.ComponentModel;

namespace CBRE.Settings
{
    public enum RenderMode
    {
        [Description("OpenGL 2.1 (Fastest, requires compatible GPU)")]
        OpenGL3, //TODO: ???

        /*[Description("OpenGL 1.0 Display Lists (Should work for most GPUs)")]
        OpenGL1DisplayLists*/
    }
}