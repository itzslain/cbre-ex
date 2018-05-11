using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Sledge.Common;
using Sledge.FileSystem;
using Sledge.Graphics.Helpers;

namespace Sledge.Providers.Texture
{
    public class MiscTexProvider : TextureProvider
    {
        private enum SpriteRenderMode
        {
            Normal = 0,     // No transparency
            Additive = 1,   // R/G/B = R/G/B, A = (R+G+B)/3
            IndexAlpha = 2, // R/G/B = Palette index 255, A = (R+G+B)/3
            AlphaTest = 3   // R/G/B = R/G/B, Palette index 255 = transparent
        }

        private static Size GetSize(string file)
        {
            Image myImage = Image.FromFile(file);

            return new Size(myImage.Width, myImage.Height);
        }

        static Dictionary<string, Image> loadedImages = new Dictionary<string, Image>();
        private static Bitmap Parse(string file)
        {
            if (loadedImages.ContainsKey(file.ToLower()))
            {
                return new Bitmap(loadedImages[file]);
            }

            return null;
        }

        public override void LoadTextures(IEnumerable<TextureItem> items)
        {
            foreach (var item in items)
            {
                using (var bmp = Parse(item.Name))
                {
                    if (bmp == null)
                    {
                        throw new Exception(item.Name);
                    }
                    TextureHelper.Create(item.Name, bmp, item.Width, item.Height, item.Flags);
                }
            }
        }

        public override IEnumerable<TexturePackage> CreatePackages(IEnumerable<string> sourceRoots, IEnumerable<string> additionalPackages, IEnumerable<string> blacklist, IEnumerable<string> whitelist)
        {
            // Sprite provider ignores the black/whitelists
            var dirs = sourceRoots.Union(additionalPackages).Where(Directory.Exists).Select(Path.GetFullPath).Select(x => x.ToLowerInvariant()).Distinct().ToList();
            //string asd = ""; //dirs.ForEach(d => asd += d + "\n");
            //throw new Exception(asd);
            
            foreach (var dir in dirs)
            {
                int slashInd = Math.Max(dir.LastIndexOf('/'),dir.LastIndexOf('\\'));
                var tp = new TexturePackage(dir, dir.Remove(0,slashInd), this);

                var sprs = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".jpg") || s.EndsWith(".jpeg") || s.EndsWith(".png"));
                //sprs.ToList().ForEach(s => asd += s + "\n");
                //throw new Exception(asd);
                if (!sprs.Any()) continue;

                foreach (var spr in sprs)
                {
                    var size = GetSize(spr);
                    var rel = Path.GetFullPath(spr).Substring(dir.Length).TrimStart('/', '\\').Replace('\\', '/');
                    rel = rel.Replace(".jpg", "").Replace(".jpeg", "").Replace(".png", "");

                    if (!loadedImages.ContainsKey(rel.ToLower()))
                    {
                        Image myImage = Image.FromFile(spr);
                        loadedImages.Add(rel.ToLower(), myImage);
                    }
                    
                    tp.AddTexture(new TextureItem(tp, rel.ToLowerInvariant(), TextureFlags.None, size.Width, size.Height));
                }

                if (tp.Items.Any()) yield return tp;
            }
        }

        public override void DeletePackages(IEnumerable<TexturePackage> packages)
        {

        }

        public override ITextureStreamSource GetStreamSource(int maxWidth, int maxHeight, IEnumerable<TexturePackage> packages)
        {
            return new JpegTextureStreamSource(packages);
        }

        private class JpegTextureStreamSource : ITextureStreamSource
        {
            private readonly List<TexturePackage> _packages;

            public JpegTextureStreamSource(IEnumerable<TexturePackage> packages)
            {
                _packages = packages.ToList();
            }

            public bool HasImage(TextureItem item)
            {
                return _packages.Any(x => x.Items.ContainsValue(item));
            }

            public Bitmap GetImage(TextureItem item)
            {
                return Parse(item.Name);
            }

            public void Dispose()
            {
                _packages.Clear();
            }
        }
    }
}
