using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>Creates a self-contained stylized 3D valley at runtime; no external art assets required.</summary>
public sealed class GameBootstrap : MonoBehaviour
{
    public static int Score { get; private set; }
    public static int EnemiesDefeated { get; private set; }
    public static bool IsGameOver { get; private set; }
    public static int PlayerLevel { get; private set; } = 1;
    public static int XP { get; private set; }
    public static int Coins { get; private set; }
    private Material grass, stone, teal, pink, gold, dark, leaf;
    private Transform worldRoot;
    private float elapsed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureBootstrap()
    {
        if (FindFirstObjectByType<GameBootstrap>() == null)
            new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
    }

    private void Awake()
    {
        if (FindObjectsByType<GameBootstrap>(FindObjectsSortMode.None).Length > 1) { Destroy(gameObject); return; }
        Score = 0; EnemiesDefeated = 0; IsGameOver = false;
        worldRoot = new GameObject("Generated World").transform;
        CreateMaterials(); BuildWorld(); BuildPlayer(); BuildLighting(); SpawnWave(1);
    }

    private Material Mat(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        Material m = new Material(shader); m.color = color; return m;
    }
    private void CreateMaterials()
    {
        grass=Mat(new Color(.18f,.34f,.26f)); stone=Mat(new Color(.26f,.29f,.38f));
        teal=Mat(new Color(.16f,.83f,.91f)); pink=Mat(new Color(.92f,.25f,.49f));
        gold=Mat(new Color(1f,.72f,.24f)); dark=Mat(new Color(.08f,.10f,.17f));
        leaf=Mat(new Color(.15f,.48f,.32f));
    }
    private GameObject Primitive(PrimitiveType type, string name, Vector3 pos, Vector3 scale, Material mat, Transform parent=null)
    {
        var g=GameObject.CreatePrimitive(type); g.name=name; g.transform.position=pos; g.transform.localScale=scale;
        if(parent) { g.transform.SetParent(parent,true); if(parent.name=="Stylized Hero") { var collider=g.GetComponent<Collider>(); if(collider) collider.enabled=false; } }
        var r=g.GetComponent<Renderer>(); if(r) r.sharedMaterial=mat;
        return g;
    }
    private void BuildWorld()
    {
        Primitive(PrimitiveType.Plane,"Valley Ground",new Vector3(0,-.12f,0),new Vector3(12,1,12),grass,worldRoot);
        Primitive(PrimitiveType.Cube,"Central Path",new Vector3(0,-.02f,5),new Vector3(5,.08f,28),stone,worldRoot);
        Primitive(PrimitiveType.Cube,"Shrine Platform",new Vector3(0,.05f,17),new Vector3(9,.35f,6),stone,worldRoot);
        for(int i=0;i<14;i++){
            float a=i*Mathf.PI*2/14f; Vector3 p=new Vector3(Mathf.Cos(a)*20,0,Mathf.Sin(a)*20);
            Primitive(PrimitiveType.Cylinder,"Ancient Pillar",p+Vector3.up*1.5f,new Vector3(.65f,1.5f,.65f),stone,worldRoot);
            Primitive(PrimitiveType.Sphere,"Pillar Crystal",p+Vector3.up*3.15f,new Vector3(.38f,.55f,.38f),teal,worldRoot);
        }
        for(int i=0;i<36;i++){
            float a=i*2.39996f; float radius=10+(i%7)*2.1f;
            Vector3 p=new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius+3);
            if(p.magnitude<8) continue;
            Primitive(PrimitiveType.Cylinder,"Tree Trunk",p+Vector3.up*.9f,new Vector3(.25f,.9f,.25f),dark,worldRoot);
            Primitive(PrimitiveType.Sphere,"Canopy",p+Vector3.up*2.15f,new Vector3(1.4f,1.7f,1.4f),leaf,worldRoot);
        }
        for(int i=0;i<8;i++){
            float x=(i%2==0?-1:1)*(3.3f+(i/2)*1.3f);
            Primitive(PrimitiveType.Cube,"Shrine Beam",new Vector3(x,2.1f,17),new Vector3(.25f,4.2f,.3f),gold,worldRoot);
        }
        Primitive(PrimitiveType.Cube,"Shrine Header",new Vector3(0,4.2f,17),new Vector3(9,.35f,.45f),pink,worldRoot);
        Primitive(PrimitiveType.Sphere,"Spirit Orb",new Vector3(0,2.1f,17),new Vector3(1.2f,1.2f,1.2f),teal,worldRoot);
    }
    private void BuildPlayer()
    {
        var player=new GameObject("Astra Player"); player.transform.position=new Vector3(0,1,0);
        var body=player.AddComponent<CapsuleCollider>(); body.height=1.9f; body.radius=.42f;
        var rb=player.AddComponent<Rigidbody>(); rb.constraints=RigidbodyConstraints.FreezeRotation; rb.interpolation=RigidbodyInterpolation.Interpolate; rb.collisionDetectionMode=CollisionDetectionMode.Continuous;
        var visuals=new GameObject("Stylized Hero"); visuals.transform.SetParent(player.transform,false);
        Primitive(PrimitiveType.Capsule,"Body",new Vector3(0,0,0),new Vector3(.72f,.78f,.62f),teal,visuals.transform);
        Primitive(PrimitiveType.Sphere,"Head",new Vector3(0,.83f,.03f),new Vector3(.56f,.56f,.52f),new Material(dark),visuals.transform);
        Primitive(PrimitiveType.Cube,"Scarf",new Vector3(0,.42f,.25f),new Vector3(.78f,.16f,.22f),pink,visuals.transform);
        Primitive(PrimitiveType.Cube,"Energy Blade",new Vector3(.58f,.1f,.18f),new Vector3(.12f,1.05f,.13f),gold,visuals.transform);
        player.AddComponent<HeroVitals>();\n        player.AddComponent<AnimePlayerController>();
        var camObj=new GameObject("Main Camera"); camObj.tag="MainCamera";
        var cam=camObj.AddComponent<Camera>(); cam.fieldOfView=62; cam.nearClipPlane=.1f; cam.farClipPlane=180;
        if(!camObj.GetComponent<AudioListener>()) camObj.AddComponent<AudioListener>();
        var follow=camObj.AddComponent<ThirdPersonCamera>(); follow.target=player.transform;
    }
    private void BuildLighting()
    {
        RenderSettings.ambientLight=new Color(.45f,.49f,.66f);
        RenderSettings.fog=true; RenderSettings.fogColor=new Color(.15f,.20f,.28f); RenderSettings.fogDensity=.004f;
        var sun=new GameObject("Moonlight").AddComponent<Light>(); sun.type=LightType.Directional; sun.intensity=1.2f; sun.transform.rotation=Quaternion.Euler(48,-32,0);
    }
    private void SpawnWave(int wave)
    {
        int count=Mathf.Min(3+wave,10);
        for(int i=0;i<count;i++){
            float angle=(i/(float)count)*Mathf.PI*2f+wave*.31f; float radius=6.5f+(i%3)*1.1f;
            Vector3 p=new Vector3(Mathf.Cos(angle)*radius,1,Mathf.Sin(angle)*radius+2);
            var e=Primitive(PrimitiveType.Capsule,"Shadow Wraith Lv."+wave,p,new Vector3(.85f,1f,.85f),pink);
            e.AddComponent<TrainingEnemy>().Configure(2+wave, 1+wave/3);
        }
        IsGameOver=false;
    }
    public static void AddScore(int value){ Score+=value; Coins+=value/10; XP+=value/2; while(XP>=PlayerLevel*100){XP-=PlayerLevel*100;PlayerLevel++;} }
    public static void EnemyDefeated(){EnemiesDefeated++;}
    public static void EndGame(){IsGameOver=true;}
    public static void NextWave(){FindFirstObjectByType<GameBootstrap>()?.StartNextWave();}
    private void StartNextWave(){SpawnWave(1+EnemiesDefeated/4);}
    private void Update(){elapsed+=Time.deltaTime;}
    private void OnGUI()
    {
        GUI.skin.box.fontSize=16; GUI.skin.label.fontSize=15; GUI.skin.button.fontSize=16;
        GUI.Box(new Rect(14,14,290,114),"");
        GUI.Label(new Rect(28,21,265,24),"ANIME SHADOW QUEST");
        GUI.Label(new Rect(28,48,260,22),"LV "+PlayerLevel+"   XP "+XP+"   COINS "+Coins);
        GUI.Label(new Rect(28,73,260,22),"Score "+Score+"   Wraiths "+EnemiesDefeated);
        GUI.Label(new Rect(28,96,260,20),"Valley Expedition");
        if(GUI.Button(new Rect(18,Screen.height-100,100,64),"MOVE ◀")) AnimePlayerController.TouchMove=-1;
        if(GUI.Button(new Rect(124,Screen.height-100,100,64),"MOVE ▶")) AnimePlayerController.TouchMove=1;
        if(GUI.Button(new Rect(Screen.width-226,Screen.height-100,96,64),"JUMP")) AnimePlayerController.TouchJump=true;
        if(GUI.Button(new Rect(Screen.width-122,Screen.height-100,104,64),"ATTACK")) AnimePlayerController.TouchAttack=true;
        if(GUI.Button(new Rect(Screen.width-122,Screen.height-174,104,58),"DODGE")) AnimePlayerController.TouchDodge=true;
        if(GUI.Button(new Rect(18,Screen.height-174,130,58),"NEXT WAVE")) NextWave();
        GUI.Label(new Rect(18,137,480,25),"WASD move • Space jump • Shift sprint • J attack • K dodge");
        if(IsGameOver){
            GUI.Box(new Rect(Screen.width/2-170,Screen.height/2-70,340,140),"WAVE CLEARED!");
            GUI.Label(new Rect(Screen.width/2-145,Screen.height/2-30,290,25),"The spirits retreat. Prepare for the next wave.");
            if(GUI.Button(new Rect(Screen.width/2-70,Screen.height/2+12,140,38),"NEXT WAVE")) NextWave();
        }
        if(Event.current.type==EventType.Repaint && GUIUtility.hotControl==0) AnimePlayerController.TouchMove=0;
    }
}
