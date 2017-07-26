using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/*
 * https://github.com/5minlab/minamo/blob/master/UnityProject/Assets/Minamo/Editor/Menu_PackageExporter.cs
 */

namespace Assets.Sagiri.Editor {
    class PackageBuilder {
        internal bool Build(string output) {
            var assets = GetAssetPaths();
            AssetDatabase.ExportPackage(assets, output);
            return true;
        }

        internal string[] GetAssetPaths() {
            var assetPaths = new List<string>();
            assetPaths.AddRange(GetScripts());
            assetPaths.AddRange(GetAssets());
            return assetPaths.ToArray();
        }

        string[] GetScripts() {
            var dirs = new List<string>()
            {
                "Assets/Sagiri",
                "Assets/Sagiri/Prefabs",
                "Assets/Sagiri/Examples",
                "Assets/Sagiri/Editor",
            };
            var founds = AssetDatabase.FindAssets("", dirs.ToArray());
            var set = new HashSet<string>();
            foreach (var guid in founds) {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var ext = Path.GetExtension(assetPath);
                var dirname = Path.GetDirectoryName(assetPath);
                if (dirs.Contains(dirname) && ext != "") {
                    set.Add(assetPath);
                }
            }

            var list = new List<string>();
            list.AddRange(set);
            list.Sort();
            return list.ToArray();
        }

        string[] GetAssets() {
            var dirs = new string[]
            {
                "Assets/StreamingAssets/Sagiri",
            };
            var founds = AssetDatabase.FindAssets("", dirs);
            var set = new HashSet<string>();
            foreach (var guid in founds) {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var ext = Path.GetExtension(assetPath);
                if (ext == ".html" || ext == ".ico" || ext == ".js" || ext == ".css") {
                    set.Add(assetPath);
                }
            }

            var list = new List<string>();
            list.AddRange(set);
            list.Sort();
            return list.ToArray();
        }
    }

    class PackageExporter : ScriptableWizard {
        [SerializeField]
        string[] assets = null;

        [MenuItem("Window/Sagiri/Export package")]
        static void Export() {
            ScriptableWizard.DisplayWizard("Exporter", typeof(PackageExporter), "Export");
        }

        private void OnWizardUpdate() {
            var b = new PackageBuilder();
            assets = b.GetAssetPaths();
        }

        private void OnWizardCreate() {
            string targetFilePath = EditorUtility.SaveFilePanel("Save package", "", "Sagiri", "unitypackage");
            if (targetFilePath == "") {
                return;
            }
            AssetDatabase.ExportPackage(assets, targetFilePath);
        }
    }
}
