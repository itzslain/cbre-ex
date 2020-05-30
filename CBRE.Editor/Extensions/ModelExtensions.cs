using System;
using System.Collections.Generic;
using System.Linq;
using CBRE.DataStructures.Geometric;
using CBRE.DataStructures.MapObjects;
using CBRE.Editor.Documents;
using CBRE.FileSystem;
using CBRE.Providers.Model;
using CBRE.Settings;

namespace CBRE.Editor.Extensions
{
    public static class ModelExtensions
    {
        private const string ModelMetaKey = "Model";
        private const string ModelNameMetaKey = "ModelName";
        private const string ModelBoundingBoxMetaKey = "BoundingBox";

        public static bool UpdateModels(this Map map, Document document)
        {
            if (CBRE.Settings.View.DisableModelRendering) return false;

            var cache = document.GetMemory<Dictionary<string, ModelReference>>("ModelCache");
            if (cache == null)
            {
                cache = new Dictionary<string, ModelReference>();
                document.SetMemory("ModelCache", cache);
            }

            return UpdateModels(document, map.WorldSpawn, cache);
        }

        public static bool UpdateModels(this Map map, Document document, IEnumerable<MapObject> objects)
        {
            if (CBRE.Settings.View.DisableModelRendering) return false;

            var cache = document.GetMemory<Dictionary<string, ModelReference>>("ModelCache");
            if (cache == null)
            {
                cache = new Dictionary<string, ModelReference>();
                document.SetMemory("ModelCache", cache);
            }

            var updated = false;
            foreach (var mo in objects) updated |= UpdateModels(document, mo, cache);
            return updated;
        }

        private static bool UpdateModels(Document document, MapObject mo, Dictionary<string, ModelReference> cache)
        {
            var updatedChildren = false;
            foreach (var child in mo.GetChildren()) updatedChildren |= UpdateModels(document, child, cache);

            var e = mo as Entity;
            if (e == null || !ShouldHaveModel(e))
            {
                var has = e != null && HasModel(e);
                if (has) UnsetModel(e);
                return updatedChildren || has;
            }

            var model = GetModelName(e);
            var existingModel = e.MetaData.Get<string>(ModelNameMetaKey);
            if (String.Equals(model, existingModel, StringComparison.InvariantCultureIgnoreCase)) return updatedChildren; // Already set; No need to continue

            if (cache.ContainsKey(model))
            {
                var mr = cache[model];
                if (mr == null) UnsetModel(e);
                else SetModel(e, mr);
                return true;
            }
            else
            {
                var file = new NativeFile(Directories.ModelDir+"/"+model); //TODO: try to understand how this originally worked? //document.Environment.Root.TraversePath(model);
                if (file == null || !ModelProvider.CanLoad(file))
                {
                    // Model not valid, get rid of it
                    UnsetModel(e);
                    cache.Add(model, null);
                    return true;
                }

                try
                {
                    var mr = ModelProvider.CreateModelReference(file);
                    SetModel(e, mr);
                    cache.Add(model, mr);
                    return true;
                }
                catch (Exception)
                {
                    // Couldn't load
                    cache.Add(model, null);
                    return updatedChildren;
                }
            }
        }

        private static bool ShouldHaveModel(Entity entity)
        {
            return GetModelName(entity) != null;
        }

        private static string GetModelName(Entity entity)
        {
            if (entity.ClassName == "model")
            {
                IEnumerable<string> extensions = new string[] { ".b3d", ".x", ".fbx" };
                string propPath = Directories.ModelDir.Replace("\\","/");
                if (string.IsNullOrEmpty(propPath)) { return null; }
                if (propPath.Last() != '/') { propPath += "/"; }
                string propName = System.IO.Path.GetFileNameWithoutExtension(entity.EntityData.GetPropertyValue("file"));
                propPath += propName;
                extensions = extensions.Where(ext => System.IO.File.Exists(propPath + ext));
                if (extensions.Any())
                {
                    return propName + extensions.First();
                }
            }

            return null;
        }

        private static void SetModel(Entity entity, ModelReference model)
        {
            entity.MetaData.Set(ModelMetaKey, model);
            entity.MetaData.Set(ModelNameMetaKey, GetModelName(entity));
            entity.MetaData.Set(ModelBoundingBoxMetaKey, model.Model.GetBoundingBox());
            entity.UpdateBoundingBox();
        }

        private static void UnsetModel(Entity entity)
        {
            entity.MetaData.Unset(ModelMetaKey);
            entity.MetaData.Unset(ModelNameMetaKey);
            entity.MetaData.Unset(ModelBoundingBoxMetaKey);
            entity.UpdateBoundingBox();
        }

        public static ModelReference GetModel(this Entity entity)
        {
            return entity.MetaData.Get<ModelReference>(ModelMetaKey);
        }

        public static bool HasModel(this Entity entity)
        {
            return entity.MetaData.Has<ModelReference>(ModelMetaKey);
        }

        public static int HideDistance(this Entity entity)
        {
            if (HasModel(entity))
            {
                int dist = CBRE.Settings.View.ModelRenderDistance;
                decimal scale = entity.EntityData.GetPropertyCoordinate("scale", Coordinate.One).VectorMagnitude();
                return (int)(scale * dist);
            }
            else
            {
                return int.MaxValue;
            }
        }
    }
}
