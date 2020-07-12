using CBRE.Providers;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CBRE.Settings.Models
{
    public class Build
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public Engine Engine { get; set; }
        public string Specification { get; set; }
        public bool DontRedirectOutput { get; set; }
        public string Path { get; set; }
        public string Bsp { get; set; }
        public string Csg { get; set; }
        public string Vis { get; set; }
        public string Rad { get; set; }
        public bool IncludePathInEnvironment { get; set; }
        public List<BuildProfile> Profiles { get; private set; }

        public CompileWorkingDirectory WorkingDirectory { get; set; }
        public bool AfterCopyBsp { get; set; }
        public bool AfterRunGame { get; set; }
        public bool AfterAskBeforeRun { get; set; }

        public bool CopyBsp { get; set; }
        public bool CopyRes { get; set; }
        public bool CopyLin { get; set; }
        public bool CopyMap { get; set; }
        public bool CopyPts { get; set; }
        public bool CopyLog { get; set; }
        public bool CopyErr { get; set; }

        public Build()
        {
            Profiles = new List<BuildProfile>();
        }

        public void Read(GenericStructure gs)
        {
            ID = gs.PropertyInteger("ID");
            Name = gs["Name"];
            Specification = gs["Specification"];
            Engine = (Engine)Enum.Parse(typeof(Engine), gs["EngineID"]);
            DontRedirectOutput = gs.PropertyBoolean("DontRedirectOutput");
            Path = gs["Path"];
            Bsp = gs["Bsp"];
            Csg = gs["Csg"];
            Vis = gs["Vis"];
            Rad = gs["Rad"];
            IncludePathInEnvironment = gs.PropertyBoolean("IncludePathInEnvironment", true);

            WorkingDirectory = gs.PropertyEnum("WorkingDirectory", CompileWorkingDirectory.TemporaryDirectory);
            AfterCopyBsp = gs.PropertyBoolean("AfterCopyBsp");
            AfterRunGame = gs.PropertyBoolean("AfterRunGame");
            AfterAskBeforeRun = gs.PropertyBoolean("AfterAskBeforeRun");

            CopyBsp = gs.PropertyBoolean("CopyBsp");
            CopyRes = gs.PropertyBoolean("CopyRes");
            CopyLin = gs.PropertyBoolean("CopyLin");
            CopyMap = gs.PropertyBoolean("CopyMap");
            CopyPts = gs.PropertyBoolean("CopyPts");
            CopyLog = gs.PropertyBoolean("CopyLog");
            CopyErr = gs.PropertyBoolean("CopyErr");

            foreach (var prof in gs.GetChildren("Profile"))
            {
                var bp = new BuildProfile();
                bp.Read(prof);
                Profiles.Add(bp);
            }
        }

        public void Write(GenericStructure gs)
        {
            gs["ID"] = ID.ToString();
            gs["Name"] = Name;
            gs["Specification"] = Specification;
            gs["EngineID"] = Engine.ToString();
            gs["DontRedirectOutput"] = DontRedirectOutput.ToString();
            gs["Path"] = Path;
            gs["Bsp"] = Bsp;
            gs["Csg"] = Csg;
            gs["Vis"] = Vis;
            gs["Rad"] = Rad;
            gs["IncludePathInEnvironment"] = IncludePathInEnvironment.ToString();

            gs["WorkingDirectory"] = WorkingDirectory.ToString();
            gs["AfterCopyBsp"] = AfterCopyBsp.ToString();
            gs["AfterRunGame"] = AfterRunGame.ToString();
            gs["AfterAskBeforeRun"] = AfterAskBeforeRun.ToString();

            gs["CopyBsp"] = CopyBsp.ToString();
            gs["CopyRes"] = CopyRes.ToString();
            gs["CopyLin"] = CopyLin.ToString();
            gs["CopyMap"] = CopyMap.ToString();
            gs["CopyPts"] = CopyPts.ToString();
            gs["CopyLog"] = CopyLog.ToString();
            gs["CopyErr"] = CopyErr.ToString();

            foreach (var bp in Profiles)
            {
                var prof = new GenericStructure("Profile");
                bp.Write(prof);
                gs.Children.Add(prof);
            }
        }
    }
}
