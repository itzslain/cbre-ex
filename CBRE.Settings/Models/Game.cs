using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CBRE.Settings.Models
{
    public class Game
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string GameDir { get; set; }
        public string BaseDir { get; set; }
        public string ModDir { get; set; }
        public string Executable { get; set; }
        public string ExecutableParameters { get; set; }
        public string MapDir { get; set; }
        public bool Autosave { get; set; }
        public bool UseCustomAutosaveDir { get; set; }
        public string AutosaveDir { get; set; }
        public int AutosaveTime { get; set; }
        public int AutosaveLimit { get; set; }
        public bool AutosaveOnlyOnChanged { get; set; }
        public bool AutosaveTriggerFileSave { get; set; }
        public string DefaultPointEntity { get; set; }
        public string DefaultBrushEntity { get; set; }
        public decimal DefaultTextureScale { get; set; }
        public decimal DefaultLightmapScale { get; set; }

        public bool OverrideMapSize { get; set; }
        public int OverrideMapSizeLow { get; set; }
        public int OverrideMapSizeHigh { get; set; }

        public string PackageBlacklist { get; set; }
        public string PackageWhitelist { get; set; }

        public Game()
        {
            AutosaveTime = 5;
            AutosaveLimit = 5;
            AutosaveOnlyOnChanged = true;
            AutosaveTriggerFileSave = true;

            DefaultBrushEntity = "";
            DefaultPointEntity = "light";
        }

        public string GetMapDirectory()
        {
            return Path.Combine(GetModDirectory(), "maps");
        }

        public string GetModDirectory()
        {
            return Path.Combine(GameDir, ModDir);
        }

        public string GetBaseDirectory()
        {
            return Path.Combine(GameDir, BaseDir);
        }

        public string GetExecutable()
        {
            return Path.Combine(GameDir, Executable);
        }

        public string GetGameLaunchArgument()
        {
            //TODO: remove?
            return "";
        }
    }
}
