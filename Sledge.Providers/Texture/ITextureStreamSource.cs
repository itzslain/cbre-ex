using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Sledge.Providers.Texture
{
    public class BitmapRef
    {
        public BitmapRef(Bitmap bmp) { Bitmap = bmp; }
        public Bitmap Bitmap { get; private set; }
    }

    public interface ITextureStreamSource : IDisposable
    {
        bool HasImage(TextureItem item);
        BitmapRef GetImage(TextureItem item);
    }

    public class MultiTextureStreamSource : ITextureStreamSource
    {
        private readonly List<ITextureStreamSource> _streams;

        public MultiTextureStreamSource(IEnumerable<ITextureStreamSource> streams)
        {
            _streams = streams.ToList();
        }

        public bool HasImage(TextureItem item)
        {
            return _streams.Any(x => x.HasImage(item));
        }

        public BitmapRef GetImage(TextureItem item)
        {
            foreach (var ss in _streams)
            {
                if (ss.HasImage(item)) return ss.GetImage(item);
            }
            return null;
        }

        public void Dispose()
        {
            _streams.ForEach(x => x.Dispose());
        }
    }
}