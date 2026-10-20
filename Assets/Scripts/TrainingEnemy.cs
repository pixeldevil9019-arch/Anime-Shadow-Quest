using UnityEngine;
public sealed class TrainingEnemy : MonoBehaviour
{
    public int health=3, damage=1;
    private Renderer cachedRenderer;
    private Color originalColor;
    private float flashTimer, attackTimer, staggerTimer;
    private Transform target;
    private Vector3 home;
    public void Configure(int hp,int hitDamage){health=Mathf.Max(1,hp);damage=Mathf.Max(1,hitDamage);}
    private void Awake(){cachedRenderer=GetComponent<Renderer>();if(cachedRenderer)originalColor=cachedRenderer.sharedMaterial.color;home=transform.position;}
    private void Start(){var player=GameObject.Find("Astra Player");if(player)target=player.transform;}
    private void Update()
    {
        if(GameBootstrap.IsGameOver)return;
        attackTimer-=Time.deltaTime;flashTimer-=Time.deltaTime;staggerTimer-=Time.deltaTime;
        if(flashTimer<=0&&cachedRenderer)cachedRenderer.material.color=originalColor;
        if(target&&staggerTimer<=0){
            Vector3 d=target.position-transform.position;d.y=0;
            if(d.magnitude>1.65f&&d.magnitude<14f){Vector3 dir=d.normalized;transform.position+=dir*(1.4f+Mathf.Min(.7f,GameBootstrap.EnemiesDefeated*.02f))*Time.deltaTime;transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(dir),5f*Time.deltaTime);}
            else if(d.magnitude<=1.65f&&attackTimer<=0){attackTimer=1.25f;var hero=target.GetComponent<HeroVitals>();if(hero)hero.TakeDamage(damage);}
        }else transform.Rotate(0,35f*Time.deltaTime,0);
    }
    public void TakeHit()
    {
        if(GameBootstrap.IsGameOver||health<=0)return;
        health--;staggerTimer=.18f;flashTimer=.13f;
        if(cachedRenderer)cachedRenderer.material.color=Color.white;
        transform.localScale*=.92f;
        if(health<=0){GameBootstrap.AddScore(100);GameBootstrap.EnemyDefeated();Destroy(gameObject);if(FindObjectsByType<TrainingEnemy>(FindObjectsSortMode.None).Length<=1)GameBootstrap.EndGame();}
    }
}
