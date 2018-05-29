using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Sledge.FileSystem;
using Sledge.Settings;
using Sledge.Settings.Models;

namespace Sledge.Editor.Environment
{
    public class GameEnvironment
    {
        public Game Game { get; private set; }

        public Build Build
        {
            get
            {
                return SettingsManager.Build;
            }
        }

        private IFile _root;

        public IFile Root
        {
            get
            {
                if (_root == null)
                {
                    var dirs = GetGameDirectories().Where(Directory.Exists).ToList();
                    if (dirs.Any()) _root = new RootFile(Game.Name, dirs.Select(x => new NativeFile(x)));
                    else _root = new VirtualFile(null, "");
                }
                return _root;
            }
        }

        public GameEnvironment(Game game)
        {
            Game = game;
        }

        public IEnumerable<string> GetGameDirectories()
        {
            if (!string.IsNullOrWhiteSpace(Sledge.Settings.Directories.TextureDir)) yield return Sledge.Settings.Directories.TextureDir;
            if (!string.IsNullOrWhiteSpace(Sledge.Settings.Directories.ModelDir)) yield return Sledge.Settings.Directories.ModelDir;

            var b = Build;
            if (b != null && b.IncludePathInEnvironment)
            {
                yield return b.Path;
            }
            
            // Editor location to the path, for sprites and the like
            yield return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }
    }
}
