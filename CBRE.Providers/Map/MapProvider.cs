using CBRE.DataStructures.MapObjects;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CBRE.Providers.Map {
    public abstract class MapProvider {
        private static readonly List<MapProvider> RegisteredProviders;

        public static string warnings;

        static MapProvider() {
            RegisteredProviders = new List<MapProvider>();
        }

        public static void Register(MapProvider provider) {
            RegisteredProviders.Add(provider);
        }

        public static void Deregister(MapProvider provider) {
            RegisteredProviders.Remove(provider);
        }

        public static void DeregisterAll() {
            RegisteredProviders.Clear();
        }

        public static DataStructures.MapObjects.Map GetMapFromFile(string fileName, IEnumerable<string> textureDirs, IEnumerable<string> modelDirs) {
            if (!File.Exists(fileName)) throw new ProviderException("The supplied file doesn't exist.");
            var provider = RegisteredProviders.FirstOrDefault(p => p.IsValidForFileName(fileName));
            if (provider != null) {
                warnings = "";
                return provider.GetFromFile(fileName, textureDirs, modelDirs);
            }
            throw new ProviderNotFoundException("No map provider was found for this file.");
        }

        public static void SaveMapToFile(string filename, DataStructures.MapObjects.Map map, DataStructures.GameData.GameData gameData) {
            var provider = RegisteredProviders.FirstOrDefault(p => p.IsValidForFileName(filename));
            if (provider != null) {
                provider.SaveToFile(filename, map, gameData);
                return;
            }
            throw new ProviderNotFoundException("No map provider was found for this file format.");
        }

        public static IEnumerable<MapFeature> GetFormatFeatures(string filename) {
            var provider = RegisteredProviders.FirstOrDefault(p => p.IsValidForFileName(filename));
            if (provider != null) {
                return provider.GetFormatFeatures();
            }
            throw new ProviderNotFoundException("No map provider was found for this file format.");
        }

        protected virtual DataStructures.MapObjects.Map GetFromFile(string filename, IEnumerable<string> textureDirs, IEnumerable<string> modelDirs) {
            using (var strm = new FileStream(filename, FileMode.Open, FileAccess.Read)) {
                return GetFromStream(strm, textureDirs, modelDirs);
            }
        }

        protected virtual void SaveToFile(string filename, DataStructures.MapObjects.Map map, DataStructures.GameData.GameData gameData) {
            using (var strm = new FileStream(filename, FileMode.Create, FileAccess.Write)) {
                SaveToStream(strm, map, gameData);
            }
        }

        protected abstract bool IsValidForFileName(string filename);
        protected abstract DataStructures.MapObjects.Map GetFromStream(Stream stream, IEnumerable<string> textureDirs, IEnumerable<string> modelDirs);
        protected abstract void SaveToStream(Stream stream, DataStructures.MapObjects.Map map, DataStructures.GameData.GameData gameData);
        protected abstract IEnumerable<MapFeature> GetFormatFeatures();
    }
}
