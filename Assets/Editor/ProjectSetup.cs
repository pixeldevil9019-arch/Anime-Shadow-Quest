#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

/// <summary>Creates a minimal build scene and registers it in Build Settings.</summary>
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
        AssetDatabase.Refresh();
        Debug.Log("Anime Shadow Quest: Main scene registered for builds.");
    }
}
#endif
