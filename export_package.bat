@set CURR_DIR=%~dp0
@set UNITY_PATH="C:\\Program Files\\Unity-5.6.2f1\\Editor\\Unity.exe"
@set PROJ_PATH=%CURR_DIR%\UnityProject
@set EXPORT_DIR=%CURR_DIR%\dist
@set METHOD=Assets.Sagiri.Editor.EntryPoint.ExportPackage
@set PACKAGE_NAME=sagiri.unitypackage
@set EXPORT_PATH=%EXPORT_DIR%\%PACKAGE_NAME%

%UNITY_PATH% -quit -batchmode -nographics -silent-crashes -projectPath %PROJ_PATH% -executeMethod %METHOD% -logFile export.log
