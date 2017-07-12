using UnityEngine;

/*
 * https://github.com/5minlab/minamo/blob/master/UnityProject/Assets/Minamo/Editor/EntryPoint.cs
 */

namespace Assets.Sagiri.Editor {
    class EntryPoint {
        public static void ExportPackage() {
            string output;
            if (EnvironmentReader.TryRead("EXPORT_PATH", out output)) {
                Debug.LogFormat("export package : {0}", output);
                var b = new PackageBuilder();
                b.Build(output);
            }
        }
    }
}
