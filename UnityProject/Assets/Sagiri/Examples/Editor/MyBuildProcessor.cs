using Assets.Sagiri.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Assets.Sagiri.Examples.Editor {
    class MyBuildPostprocessor {
        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
            if (!Debug.isDebugBuild) {
                EntryPoint.RemoveSteamingAssets(target, pathToBuiltProject);
            }
        }
    }
}