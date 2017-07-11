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
            var dirs = new string[]
            {
                "Assets/Sagiri"
            };
            var founds = AssetDatabase.FindAssets("", dirs);
            var list = new List<string>();
            foreach (var guid in founds) {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath.EndsWith(".cs")) {
                    list.Add(assetPath);
                }
            }
            return list.ToArray();
        }

        string[] GetAssets() {
            var dirs = new string[]
            {
                "Assets/StreamingAssets/Sagiri",
            };
            var founds = AssetDatabase.FindAssets("", dirs);
            var list = new List<string>();
            foreach (var guid in founds) {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var ext = Path.GetExtension(assetPath);
                if (ext == ".html" || ext == ".ico" || ext == ".js" || ext == ".css") {
                    list.Add(assetPath);
                }
            }
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
