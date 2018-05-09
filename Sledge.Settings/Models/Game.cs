using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Sledge.Providers;

namespace Sledge.Settings.Models
{
    public class Game
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public Engine Engine { get; set; }
        public int BuildID { get; set; }
        public bool SteamInstall { get; set; }
        public string WonGameDir { get; set; }
        public string SteamGameDir { get; set; }
        public string BaseDir { get; set; }
        public string ModDir { get; set; }
        public bool UseHDModels { get; set; }
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

        public bool IncludeFgdDirectoriesInEnvironment { get; set; }
        public bool OverrideMapSize { get; set; }
        public int OverrideMapSizeLow { get; set; }
        public int OverrideMapSizeHigh { get; set; }

        //public List<Fgd> Fgds { get; set; }
        public List<string> AdditionalPackages { get; set; }
        public string PackageBlacklist { get; set; }
        public string PackageWhitelist { get; set; }

        public Game()
        {
            //Fgds = new List<Fgd>();
            AdditionalPackages = new List<string>();
            AutosaveTime = 5;
            AutosaveLimit = 5;
            AutosaveOnlyOnChanged = true;
            AutosaveTriggerFileSave = true;
        }
        
        public string GetMapDirectory()
        {
            return Path.Combine(GetModDirectory(), "maps");
        }

        public string GetModDirectory()
        {
            return SteamInstall
                ? Path.Combine(Steam.SteamDirectory, "steamapps", "common", SteamGameDir, ModDir)
                : Path.Combine(WonGameDir, ModDir);
        }

        public string GetBaseDirectory()
        {
            return SteamInstall
                ? Path.Combine(Steam.SteamDirectory, "steamapps", "common", SteamGameDir, BaseDir)
                : Path.Combine(WonGameDir, BaseDir);
        }
        
        public string GetExecutable()
        {
            return SteamInstall
                ? Path.Combine(Steam.SteamDirectory, "steam.exe")
                : Path.Combine(WonGameDir, Executable);
        }

        public string GetGameLaunchArgument()
        {
            if (SteamInstall)
            {
                var id = GetSteamAppId();
                return "-applaunch " + id;
            }
            else
            {
                var mod = (ModDir ?? "").ToLowerInvariant();
                if (mod != "valve") return "-game " + mod;
                return "";
            }
        }

        public IEnumerable<string> GetTextureBlacklist()
        {
            var bl = new List<string>();
            if (Engine == Engine.Goldsource)
            {
                bl.Add("cached");
                bl.Add("gfx");
            }
            bl.AddRange((PackageBlacklist ?? "").Trim().Split('\n').Select(x => x.Trim()).Where(x => !String.IsNullOrWhiteSpace(x)));
            return bl;
        }

        public IEnumerable<string> GetTextureWhitelist()
        {
            return (PackageWhitelist ?? "").Trim().Split('\n').Select(x => x.Trim()).Where(x => !String.IsNullOrWhiteSpace(x));
        }

        private int GetSteamAppId()
        {
            if (Engine == Engine.Goldsource)
            {
                switch ((ModDir ?? "").ToLowerInvariant())
                {
                    case "bshift":
                        return 130;
                    case "czero":
                        return 80;
                    case "czeror":
                        return 100;
                    case "cstrike":
                        return 10;
                    case "dod":
                        return 30;
                    case "dmc":
                        return 40;
                    case "ricochet":
                        return 60;
                    case "gearbox":
                        return 50;
                    case "tfc":
                        return 20;
                    case "valve":
                    default:
                        return 70;
                }
            }
            // todo source
            return 0;
        }
    }
}
