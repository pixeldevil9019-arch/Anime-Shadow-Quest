#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>Deterministic CI entry point: repair build settings, then create an Android APK.</summary>
public static class AnimeShadowQuestBuild
{
    public static void PerformBuild()
    {
        ProjectSetup.EnsureMainScene();
        string[] scenes=Array.ConvertAll(
            System.Array.FindAll(EditorBuildSettings.scenes, scene=>scene.enabled),
            scene=>scene.path);
        if(scenes.Length==0)throw new Exception("No enabled scenes found. Main scene setup failed.");

        const string outputPath="build/Android/AnimeShadowQuest.apk";
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
        EditorUserBuildSettings.buildAppBundle=false;
        var options=new BuildPlayerOptions
        {
            scenes=scenes,
            locationPathName=outputPath,
            target=BuildTarget.Android,
            options=BuildOptions.None
        };
        Debug.Log("Building Anime Shadow Quest APK to "+outputPath);
        BuildReport report=BuildPipeline.BuildPlayer(options);
        if(report.summary.result!=BuildResult.Succeeded)
            throw new Exception("Android APK build failed: "+report.summary.result+"; errors="+report.summary.totalErrors);
        Debug.Log("Android APK created successfully. Size: "+report.summary.totalSize+" bytes.");
    }
}
#endif
