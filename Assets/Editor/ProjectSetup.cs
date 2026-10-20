#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

/// <summary>Creates a build scene and applies Android settings before CI builds the APK.</summary>
[InitializeOnLoad]
public static class ProjectSetup
{
    static ProjectSetup(){EditorApplication.delayCall+=EnsureMainScene;}
    [MenuItem("Anime Shadow Quest/Repair Build Scene")]
    public static void EnsureMainScene()
    {
        const string folder="Assets/Scenes";
        const string path=folder+"/Main.unity";
        if(!Directory.Exists(folder))Directory.CreateDirectory(folder);
        if(!File.Exists(path))
        {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene,path);
        }
        var scenes=new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool found=false;
        foreach(var s in scenes)if(s.path==path){found=true;break;}
        if(!found){scenes.Add(new EditorBuildSettingsScene(path,true));EditorBuildSettings.scenes=scenes.ToArray();}

        PlayerSettings.productName="Anime Shadow Quest";
        PlayerSettings.companyName="Moonbound Studio";
        PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android,"com.pixeldevil.animeshadowquest");
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android,ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.useCustomMainGradleTemplate=true;
        PlayerSettings.Android.useCustomGradlePropertiesTemplate=true;
        PlayerSettings.Android.useCustomSettingsTemplate=true;
        AssetDatabase.Refresh();
        Debug.Log("Anime Shadow Quest: build scene and Android player settings are ready.");
    }
}
#endif
