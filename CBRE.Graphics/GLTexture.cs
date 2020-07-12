using CBRE.Common;
using CBRE.Graphics.Helpers;

namespace CBRE.Graphics {
    public class GLTexture : ITexture {
        public int Reference { get; private set; }
        public TextureFlags Flags { get; private set; }
        public int FrameBufferObject { get; private set; }
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool HasTransparency { get; set; }

        internal GLTexture(int reference, string name, TextureFlags flags, int fbo) {
            Name = name;
            Reference = reference;
            Flags = flags;
            FrameBufferObject = fbo;
        }

        public void Bind() {
            TextureHelper.Bind(Reference);
        }

        public void Unbind() {
            TextureHelper.Unbind();
        }

        public void Dispose() {
            TextureHelper.DeleteTexture(Reference);
            TextureHelper.Textures.Remove(Name);
        }
    }
}
