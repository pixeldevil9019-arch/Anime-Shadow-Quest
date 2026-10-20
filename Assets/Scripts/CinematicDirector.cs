using UnityEngine;

/// <summary>Lightweight cinematic title cards using Unity's immediate UI, with tap/click to skip.</summary>
public sealed class CinematicDirector : MonoBehaviour
{
    private float startedAt;
    private bool skipped;
    private GUIStyle titleStyle, bodyStyle;
    private readonly string[] lines={
        "When the moon shattered, the spirits forgot their names.",
        "The shrine has awakened. The valley is no longer safe.",
        "You are Astra, last blade of the Moonbound order.",
        "Find the wraiths. Survive the trials. Restore the moon."
    };
    private void Start(){startedAt=Time.unscaledTime;}
    private void Update(){if(!skipped&&(Time.unscaledTime-startedAt>17f||Input.anyKeyDown||Input.touchCount>0))skipped=true;}
    private void OnGUI()
    {
        if(skipped)return;
        float t=Time.unscaledTime-startedAt;
        float alpha=Mathf.Clamp01(Mathf.Min(t*1.8f,(17f-t)*1.8f));
        Color old=GUI.color;
        GUI.color=new Color(0,0,0,.78f*alpha);
        GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);
        GUI.color=new Color(0,0,0,.72f*alpha);
        GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height*.16f),Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(0,Screen.height*.84f,Screen.width,Screen.height*.16f),Texture2D.whiteTexture);
        GUI.color=new Color(1,1,1,alpha);
        if(titleStyle==null)
        {
            titleStyle=new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,fontSize=Mathf.Max(24,Screen.height/20),fontStyle=FontStyle.Bold};
            bodyStyle=new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,fontSize=Mathf.Max(16,Screen.height/38),wordWrap=true};
        }
        GUI.Label(new Rect(0,Screen.height*.32f,Screen.width,Screen.height*.13f),"ANIME SHADOW QUEST",titleStyle);
        int line=Mathf.Clamp(Mathf.FloorToInt(t/3.7f),0,lines.Length-1);
        GUI.Label(new Rect(Screen.width*.1f,Screen.height*.49f,Screen.width*.8f,Screen.height*.12f),lines[line],bodyStyle);
        GUI.Label(new Rect(0,Screen.height*.88f,Screen.width,40),"TAP TO BEGIN  •  "+Mathf.CeilToInt(Mathf.Max(0,17f-t))+"s",bodyStyle);
        GUI.color=old;
    }
}