# sagiri

[![Build Status](https://travis-ci.org/5minlab/sagiri.svg?branch=master)](https://travis-ci.org/5minlab/sagiri)

Web browser based Realtime Untiy3D Log viewer

![logs in unity](https://raw.githubusercontent.com/5minlab/sagiri/master/document/log-unity.png)

![browser log viewer](https://raw.githubusercontent.com/5minlab/sagiri/master/document/log-browser.png)

It mix two library, [CUDLR][CUDLR] and [Unity-File-Debug].
Use [CUDLR][CUDLR] as log server.
Use [Unity-File-Debug][Unity-File-Debug] as log viewer.

## Features
* Supports iOS, Android, PC/Mac Standalone, and the Unity Editor
* Capture Unity log messages and stack traces

## How to use
1. Import [latest release's package](https://github.com/5minlab/sagiri/releases) into your project.
2. Move the prefab `SagiriServer` into your scene
3. Set the port on `SagiriServer` game object. (default value is 55055)
4. Run the game and connect to http://localhost:55055 with your browser.
(you can find address in unity game screen)
5. If you want to view log in unity build, check `Development build` flag.
or comment this code block.

```
[PostProcessBuild(1)]
public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
	if (!Debug.isDebugBuild) {
		//EntryPoint.RemoveSteamingAssets(target, pathToBuiltProject);
	}
}
```

## Example
open scene, `Sagiri/Examples/SagiriExample`

[CUDLR]: https://github.com/proletariatgames/CUDLR
[Unity-File-Debug]: https://github.com/Sacred-Seed-Studio/Unity-File-Debug
