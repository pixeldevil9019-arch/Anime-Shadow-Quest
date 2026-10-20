using UnityEngine;
public sealed class HeroVitals : MonoBehaviour
{
    public int maxHealth=100;
    public int Health {get; private set;}
    private float invulnerability;
    private void Awake(){Health=maxHealth;}
    private void Update(){invulnerability-=Time.deltaTime;}
    public void TakeDamage(int amount)
    {
        if(invulnerability>0||GameBootstrap.IsGameOver)return;
        Health=Mathf.Max(0,Health-Mathf.Max(1,amount));invulnerability=.45f;
        if(Health<=0)GameBootstrap.EndGame();
    }
    public void Heal(int amount){Health=Mathf.Min(maxHealth,Health+Mathf.Max(0,amount));}
}
