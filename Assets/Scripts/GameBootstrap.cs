using UnityEngine;

/// <summary>Builds the playable world from primitives so the project needs no paid assets.</summary>
public sealed class GameBootstrap : MonoBehaviour
{
    public static int Score { get; private set; }
    public static int EnemiesDefeated { get; private set; }
    public static bool IsGameOver { get; private set; }
    public static int PlayerLevel { get; private set; } = 1;
    public static int XP { get; private set; }
    public static int Coins { get; private set; }
    public static int Wave { get; private set; } = 1;
    public static string Chapter { get; private set; } = "CHAPTER I — THE AWAKENING";
    private Material grass, stone, teal, pink, gold, dark, leaf, water;
    private Transform worldRoot;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureBootstrap()
    {
        if (FindFirstObjectByType<GameBootstrap>() == null)
            new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
    }

    private void Awake()
    {
        if (FindObjectsByType<GameBootstrap>(FindObjectsSortMode.None).Length > 1) { Destroy(gameObject); return; }
        Score=PlayerPrefs.GetInt("ASQ.Score",0); EnemiesDefeated=PlayerPrefs.GetInt("ASQ.Defeated",0); IsGameOver=false; PlayerLevel=PlayerPrefs.GetInt("ASQ.Level",1); XP=PlayerPrefs.GetInt("ASQ.XP",0); Coins=PlayerPrefs.GetInt("ASQ.Coins",0); Wave=Mathf.Max(1,PlayerPrefs.GetInt("ASQ.Wave",1));
        worldRoot=new GameObject("Generated World").transform;
        CreateMaterials(); BuildWorld(); BuildPlayer(); BuildLighting(); SpawnWave(Wave);
        gameObject.AddComponent<CinematicDirector>();
    }

    private Material Mat(Color color)
    {
        Shader shader=Shader.Find("Universal Render Pipeline/Lit");
        if(shader==null) shader=Shader.Find("Standard");
        if(shader==null) shader=Shader.Find("Sprites/Default");
        Material m=new Material(shader); m.color=color; return m;
    }
    private void CreateMaterials()
    {
        grass=Mat(new Color(.12f,.23f,.25f)); stone=Mat(new Color(.22f,.25f,.36f));
        teal=Mat(new Color(.13f,.83f,.91f)); pink=Mat(new Color(.94f,.23f,.49f));
        gold=Mat(new Color(1f,.70f,.23f)); dark=Mat(new Color(.055f,.07f,.13f));
        leaf=Mat(new Color(.12f,.37f,.34f)); water=Mat(new Color(.08f,.43f,.58f));
    }
    private GameObject Primitive(PrimitiveType type,string name,Vector3 pos,Vector3 scale,Material mat,Transform parent=null)
    {
        GameObject g=GameObject.CreatePrimitive(type); g.name=name; g.transform.position=pos; g.transform.localScale=scale;
        if(parent)
        {
            g.transform.SetParent(parent,true);
            if(parent.name=="Stylized Hero") { Collider col=g.GetComponent<Collider>(); if(col) col.enabled=false; }
        }
        Renderer r=g.GetComponent<Renderer>(); if(r) r.sharedMaterial=mat;
        return g;
    }
    private void BuildWorld()
    {
        Primitive(PrimitiveType.Plane,"Moonlit Valley",new Vector3(0,-.12f,0),new Vector3(12,1,12),grass,worldRoot);
        Primitive(PrimitiveType.Cube,"Ancient Causeway",new Vector3(0,-.015f,5),new Vector3(5,.08f,30),stone,worldRoot);
        Primitive(PrimitiveType.Cube,"Shrine Island",new Vector3(0,.05f,17),new Vector3(10,.35f,7),stone,worldRoot);
        Primitive(PrimitiveType.Cube,"Reflecting Pool",new Vector3(0,.015f,-8),new Vector3(12,.04f,4),water,worldRoot);
        for(int i=0;i<18;i++)
        {
            float a=i*Mathf.PI*2f/18f; Vector3 p=new Vector3(Mathf.Cos(a)*22,0,Mathf.Sin(a)*22);
            Primitive(PrimitiveType.Cylinder,"Ruined Pillar",p+Vector3.up*1.45f,new Vector3(.65f,1.45f,.65f),stone,worldRoot);
            Primitive(PrimitiveType.Sphere,"Spirit Crystal",p+Vector3.up*3.1f,new Vector3(.32f,.52f,.32f),teal,worldRoot);
        }
        for(int i=0;i<54;i++)
        {
            float a=i*2.39996f; float radius=10+(i%9)*1.65f;
            Vector3 p=new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius+3);
            if(p.magnitude<8) continue;
            Primitive(PrimitiveType.Cylinder,"Sakura Trunk",p+Vector3.up*.9f,new Vector3(.22f,.9f,.22f),dark,worldRoot);
            Primitive(PrimitiveType.Sphere,"Sakura Canopy",p+Vector3.up*2.15f,new Vector3(1.35f,1.5f,1.35f),i%3==0?pink:leaf,worldRoot);
        }
        for(int i=0;i<8;i++)
        {
            float x=(i%2==0?-1:1)*(3.1f+(i/2)*1.35f);
            Primitive(PrimitiveType.Cube,"Shrine Pillar",new Vector3(x,2.1f,17),new Vector3(.24f,4.2f,.32f),gold,worldRoot);
        }
        Primitive(PrimitiveType.Cube,"Shrine Crown",new Vector3(0,4.2f,17),new Vector3(9,.35f,.5f),pink,worldRoot);
        Primitive(PrimitiveType.Sphere,"Awakening Orb",new Vector3(0,2.1f,17),new Vector3(1.25f,1.25f,1.25f),teal,worldRoot);
        for(int i=0;i<12;i++)
        {
            Vector3 p=new Vector3((i%4-1.5f)*3f,.5f,9+(i/4)*2.2f);
            Primitive(PrimitiveType.Cube,"Path Lantern",p,new Vector3(.24f,.8f,.24f),gold,worldRoot);
            Primitive(PrimitiveType.Sphere,"Lantern Glow",p+Vector3.up*.52f,new Vector3(.3f,.3f,.3f),teal,worldRoot);
        }
    }
    private void BuildPlayer()
    {
        GameObject player=new GameObject("Astra Player"); player.transform.position=new Vector3(0,1,0);
        CapsuleCollider capsule=player.AddComponent<CapsuleCollider>(); capsule.height=1.9f; capsule.radius=.42f;
        Rigidbody rb=player.AddComponent<Rigidbody>(); rb.constraints=RigidbodyConstraints.FreezeRotation; rb.interpolation=RigidbodyInterpolation.Interpolate; rb.collisionDetectionMode=CollisionDetectionMode.Continuous;
        Transform visuals=new GameObject("Stylized Hero").transform; visuals.SetParent(player.transform,false);
        Primitive(PrimitiveType.Capsule,"Hero Body",Vector3.zero,new Vector3(.72f,.78f,.62f),teal,visuals);
        Primitive(PrimitiveType.Sphere,"Hero Hair",new Vector3(0,.83f,.03f),new Vector3(.58f,.57f,.54f),dark,visuals);
        Primitive(PrimitiveType.Cube,"Hero Scarf",new Vector3(0,.42f,.25f),new Vector3(.78f,.16f,.22f),pink,visuals);
        Primitive(PrimitiveType.Cube,"Spirit Blade",new Vector3(.58f,.1f,.18f),new Vector3(.12f,1.05f,.13f),gold,visuals);
        player.AddComponent<HeroVitals>(); player.AddComponent<AnimePlayerController>();
        GameObject camObj=new GameObject("Main Camera"); camObj.tag="MainCamera";
        Camera cam=camObj.AddComponent<Camera>(); cam.fieldOfView=62; cam.nearClipPlane=.1f; cam.farClipPlane=180;
        camObj.AddComponent<AudioListener>();
        camObj.AddComponent<ThirdPersonCamera>().target=player.transform;
    }
    private void BuildLighting()
    {
        RenderSettings.ambientLight=new Color(.36f,.40f,.58f); RenderSettings.fog=true;
        RenderSettings.fogColor=new Color(.08f,.12f,.22f); RenderSettings.fogDensity=.0035f;
        GameObject sunObj=new GameObject("Cinematic Moonlight"); Light sun=sunObj.AddComponent<Light>();
        sun.type=LightType.Directional; sun.intensity=1.25f; sun.color=new Color(.66f,.79f,1f); sun.transform.rotation=Quaternion.Euler(42,-32,0);
    }
    private void SpawnWave(int wave)
    {
        Wave=wave; SaveProgress();
        Chapter=wave<4?"CHAPTER I — THE AWAKENING":wave<8?"CHAPTER II — THE FORGOTTEN GARDEN":wave<12?"CHAPTER III — THE SHATTERED MOON":"CHAPTER IV — THE ETERNAL NIGHT";
        int count=Mathf.Min(2+wave,12);
        bool boss=wave%5==0;
        if(boss) count=1+Mathf.Min(2,wave/5);
        for(int i=0;i<count;i++)
        {
            float angle=(i/(float)count)*Mathf.PI*2f+wave*.31f; float radius=boss?7.5f:6.5f+(i%3)*1.1f;
            Vector3 p=new Vector3(Mathf.Cos(angle)*radius,1,Mathf.Sin(angle)*radius+2);
            GameObject enemy=Primitive(boss?PrimitiveType.Sphere:PrimitiveType.Capsule,boss?"MOON DEVOURER — BOSS":"Shadow Wraith Lv."+wave,p,boss?new Vector3(1.9f,2.3f,1.9f):new Vector3(.85f,1f,.85f),boss?gold:pink);
            TrainingEnemy ai=enemy.AddComponent<TrainingEnemy>(); ai.Configure(boss?8+wave*2:2+wave, boss?8:1+wave/3, boss);
        }
        IsGameOver=false;
    }
    public static void AddScore(int value)
    {
        Score+=value; Coins+=Mathf.Max(1,value/10); XP+=Mathf.Max(1,value/2);
        while(XP>=PlayerLevel*100){XP-=PlayerLevel*100;PlayerLevel++;}
        SaveProgress();
    }
    public static void EnemyDefeated(){EnemiesDefeated++;SaveProgress();}
    public static void EndGame(){IsGameOver=true;SaveProgress();}
    public static void SaveProgress()
    {
        PlayerPrefs.SetInt("ASQ.Score",Score);
        PlayerPrefs.SetInt("ASQ.Defeated",EnemiesDefeated);
        PlayerPrefs.SetInt("ASQ.Level",PlayerLevel);
        PlayerPrefs.SetInt("ASQ.XP",XP);
        PlayerPrefs.SetInt("ASQ.Coins",Coins);
        PlayerPrefs.SetInt("ASQ.Wave",Wave);
        PlayerPrefs.Save();
    }
    public static void NextWave()
    {
        GameBootstrap game=FindFirstObjectByType<GameBootstrap>();
        if(game!=null && IsGameOver) game.StartNextWave();
    }
    private void StartNextWave()
    {
        foreach(TrainingEnemy enemy in FindObjectsByType<TrainingEnemy>(FindObjectsSortMode.None)) if(enemy) Destroy(enemy.gameObject);
        SpawnWave(Wave+1);
    }
    private void OnGUI()
    {
        GUI.skin.box.fontSize=16; GUI.skin.label.fontSize=15; GUI.skin.button.fontSize=16;
        GUI.Box(new Rect(14,14,305,142),"");
        GUI.Label(new Rect(28,20,280,24),"ANIME SHADOW QUEST");
        GUI.Label(new Rect(28,45,280,22),"LV "+PlayerLevel+"   XP "+XP+"   COINS "+Coins);
        GUI.Label(new Rect(28,69,280,22),"Score "+Score+"   Spirits "+EnemiesDefeated);
        HeroVitals vitals=FindFirstObjectByType<HeroVitals>();
        if(vitals) GUI.Label(new Rect(28,93,270,22),"HP "+vitals.Health+" / "+vitals.maxHealth);
        GUI.Label(new Rect(28,119,280,22),"WAVE "+Wave+"  •  "+Chapter);
        bool moveLeft=GUI.RepeatButton(new Rect(18,Screen.height-100,100,64),"◀ MOVE");
        bool moveRight=GUI.RepeatButton(new Rect(124,Screen.height-100,100,64),"MOVE ▶");
        if(Event.current.type==EventType.Repaint) AnimePlayerController.TouchMove=moveLeft&&!moveRight?-1:moveRight&&!moveLeft?1:0;
        if(GUI.Button(new Rect(Screen.width-226,Screen.height-100,96,64),"JUMP")) AnimePlayerController.TouchJump=true;
        if(GUI.Button(new Rect(Screen.width-122,Screen.height-100,104,64),"ATTACK")) AnimePlayerController.TouchAttack=true;
        if(GUI.Button(new Rect(Screen.width-122,Screen.height-174,104,58),"DODGE")) AnimePlayerController.TouchDodge=true;
        if(GUI.Button(new Rect(18,Screen.height-174,130,58),"HEAL +")) { if(vitals) vitals.Heal(30); }
        GUI.Label(new Rect(18,165,520,24),"WASD / arrows move • Space jump • Shift sprint • J attack • K dodge");
        if(IsGameOver)
        {
            GUI.Box(new Rect(Screen.width/2-180,Screen.height/2-76,360,152),vitals!=null&&vitals.Health<=0?"THE HERO HAS FALLEN":"WAVE CLEARED");
            GUI.Label(new Rect(Screen.width/2-150,Screen.height/2-30,300,25),vitals!=null&&vitals.Health<=0?"Try again: heal and return to battle.":"The spirits retreat. A stronger enemy awaits.");
            if(GUI.Button(new Rect(Screen.width/2-72,Screen.height/2+14,144,38),vitals!=null&&vitals.Health<=0?"REVIVE":"NEXT CHAPTER"))
            {
                if(vitals!=null&&vitals.Health<=0){vitals.Heal(100);IsGameOver=false;}
                else NextWave();
            }
        }
    }
}