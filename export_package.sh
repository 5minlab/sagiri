#!/bin/bash

CURR_DIR=`pwd`
UNITY_PATH="/Applications/Unity/Unity.app/Contents/MacOS/Unity"
PROJ_PATH=$CURR_DIR/UnityProject
EXPORT_DIR=$CURR_DIR/dist
METHOD=Assets.Sagiri.Editor.EntryPoint.ExportPackage
PACKAGE_NAME=minamo.unitypackage
export EXPORT_PATH=$EXPORT_DIR/$PACKAGE_NAME

LOG_FILE=export.log

$UNITY_PATH -quit -batchmode -nographics -silent-crashes -projectPath $PROJ_PATH -executeMethod $METHOD -logFile $LOG_FILE
