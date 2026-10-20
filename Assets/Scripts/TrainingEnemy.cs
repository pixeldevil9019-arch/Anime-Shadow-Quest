using UnityEngine;

public sealed class TrainingEnemy : MonoBehaviour
{
    public int health=3, damage=1;
    public bool isBoss;
    private Renderer cachedRenderer;
    private Color originalColor;
    private float flashTimer, attackTimer, staggerTimer, strafeTimer;
    private Transform target;
    public void Configure(int hp,int hitDamage,bool boss=false){health=Mathf.Max(1,hp);damage=Mathf.Max(1,hitDamage);isBoss=boss;}
    private void Awake(){cachedRenderer=GetComponent<Renderer>();if(cachedRenderer)originalColor=cachedRenderer.sharedMaterial.color;}
    private void Start(){GameObject player=GameObject.Find("Astra Player");if(player)target=player.transform;}
    private void Update()
    {
        if(GameBootstrap.IsGameOver)return;
        attackTimer-=Time.deltaTime;flashTimer-=Time.deltaTime;staggerTimer-=Time.deltaTime;strafeTimer-=Time.deltaTime;
        if(flashTimer<=0&&cachedRenderer)cachedRenderer.material.color=originalColor;
        if(!target||staggerTimer>0)return;
        Vector3 delta=target.position-transform.position;delta.y=0;float distance=delta.magnitude;
        if(distance>1.75f&&distance<(isBoss?24f:15f))
        {
            Vector3 dir=delta.normalized;float speed=isBoss?1.9f:1.45f+Mathf.Min(.8f,GameBootstrap.EnemiesDefeated*.015f);
            transform.position+=dir*speed*Time.deltaTime;transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(dir),6f*Time.deltaTime);
        }
        else if(distance<=1.75f&&attackTimer<=0)
        {
            attackTimer=isBoss?1f:1.35f;HeroVitals vitals=target.GetComponent<HeroVitals>();if(vitals)vitals.TakeDamage(damage);
        }
        if(isBoss&&strafeTimer<=0&&distance>2.2f){strafeTimer=1.4f;transform.position+=transform.right*(Random.value>.5f?1.2f:-1.2f);}
    }
    public void TakeHit()
    {
        if(GameBootstrap.IsGameOver||health<=0)return;
        health--;staggerTimer=isBoss?.3f:.18f;flashTimer=.13f;
        if(cachedRenderer)cachedRenderer.material.color=Color.white;
        transform.localScale*=isBoss?.97f:.92f;
        if(health<=0)
        {
            GameBootstrap.AddScore(isBoss?1000:100);GameBootstrap.EnemyDefeated();Destroy(gameObject);
            if(FindObjectsByType<TrainingEnemy>(FindObjectsSortMode.None).Length<=1)GameBootstrap.EndGame();
        }
    }
}