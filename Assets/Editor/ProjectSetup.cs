#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>Ensures CI has a valid scene in Build Settings before building Android.</summary>
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
        Scene scene;
        if(File.Exists(path))scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
        else {scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorSceneManager.SaveScene(scene,path);}
        var scenes=new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool found=false;
        foreach(var s in scenes)if(s.path==path){found=true;break;}
        if(!found){scenes.Add(new EditorBuildSettingsScene(path,true));EditorBuildSettings.scenes=scenes.ToArray();}
        if(!EditorApplication.isPlayingOrWillChangePlaymode)Debug.Log("Anime Shadow Quest: Main scene registered for builds.");
    }
}
#endif
