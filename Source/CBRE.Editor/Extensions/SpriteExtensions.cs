using CBRE.DataStructures.Geometric;
using CBRE.DataStructures.MapObjects;
using CBRE.Editor.Documents;
using CBRE.Providers.Texture;
using CBRE.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CBRE.Editor.Extensions
{
    public static class SpriteExtensions
    {
        private const string SpriteMetaKey = "Model";
        private const string SpriteBoundingBoxMetaKey = "BoundingBox";

        public static bool UpdateSprites(this Map map, Document document)
        {
            if (CBRE.Settings.View.DisableSpriteRendering) return false;
            return UpdateSprites(document, map.WorldSpawn);
        }

        public static bool UpdateSprites(this Map map, Document document, IEnumerable<MapObject> objects)
        {
            if (CBRE.Settings.View.DisableSpriteRendering) return false;

            bool updated = false;
            foreach (MapObject mo in objects) updated |= UpdateSprites(document, mo);
            return updated;
        }

        private static bool UpdateSprites(Document document, MapObject mo)
        {
            bool updatedChildren = false;
            foreach (MapObject child in mo.GetChildren()) updatedChildren |= UpdateSprites(document, child);

            Entity e = mo as Entity;
            if (e == null || !ShouldHaveSprite(e, document))
            {
                bool has = e != null && HasSprite(e);
                // HACK: literal horror, please forgive me Bill Gates.
                bool usesModels = e != null && e.GameData != null && e.GameData.Behaviours.FirstOrDefault(x => x.Name == "useModels") != null;

                if (has || usesModels) UnsetSprite(e);

                return updatedChildren || has;
            }

            string sprite = GetSpriteName(e);
            string existingSprite = e.MetaData.Get<string>(SpriteMetaKey);
            if (String.Equals(sprite, existingSprite, StringComparison.OrdinalIgnoreCase)) return updatedChildren; // Already set; No need to continue

            TextureItem tex = document.TextureCollection.GetItem(sprite);
            if (tex == null)
            {
                UnsetSprite(e);
                return true;
            }
            SetSprite(e, tex);
            return true;
        }

        private static bool ShouldHaveSprite(Entity entity, Document document)
        {
            string modelFile = entity.EntityData.GetPropertyValue("file");
            string modelPath = Directories.GetModelPath(modelFile);

            if (!string.IsNullOrEmpty(modelFile) && modelPath != null) return false;

            return GetSpriteName(entity) != null;
        }

        private static string GetSpriteName(Entity entity)
        {
            if (entity.GameData == null) return null;

            DataStructures.GameData.Behaviour spr = entity.GameData.Behaviours.FirstOrDefault(x => String.Equals(x.Name, "sprite", StringComparison.OrdinalIgnoreCase))
                ?? entity.GameData.Behaviours.FirstOrDefault(x => String.Equals(x.Name, "iconsprite", StringComparison.OrdinalIgnoreCase));

            if (spr == null) return null;

            // First see if the studio behaviour forces a model...
            if (spr.Values.Count == 1 && !String.IsNullOrWhiteSpace(spr.Values[0]))
            {
                return spr.Values[0].Trim();
            }

            return null;
        }

        private static void SetSprite(Entity entity, TextureItem tex)
        {
            entity.MetaData.Set(SpriteMetaKey, tex.Name);
            Coordinate bb = new Coordinate(64, 64, 64);

            // Don't set the bounding box if the sprite comes from the iconsprite gamedata
            if (entity.GameData == null || !entity.GameData.Behaviours.Any(x => String.Equals(x.Name, "iconsprite", StringComparison.CurrentCultureIgnoreCase)))
            {
                entity.MetaData.Set(SpriteBoundingBoxMetaKey, new Box(-bb / 2, bb / 2));
                entity.MetaData.Set("RotateBoundingBox", false); // todo rotations
                entity.UpdateBoundingBox();
            }
        }

        private static void UnsetSprite(Entity entity)
        {
            // HACK: HACK HACK HACK!!!!
            if (entity.GameData.Behaviours.FirstOrDefault(x => x.Name == "useModels") == null)
            {
                entity.MetaData.Unset(SpriteMetaKey);
                entity.MetaData.Unset(SpriteBoundingBoxMetaKey);
            }

            entity.MetaData.Unset("RotateBoundingBox");
            entity.UpdateBoundingBox();
        }

        public static string GetSprite(this Entity entity)
        {
            return entity.MetaData.Get<string>(SpriteMetaKey);
        }

        public static bool HasSprite(this Entity entity)
        {
            return entity.MetaData.Has<string>(SpriteMetaKey);
        }
    }
}
