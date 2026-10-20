using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public sealed class AnimePlayerController : MonoBehaviour
{
    public float moveSpeed=5.8f, sprintMultiplier=1.55f, jumpForce=6.2f, attackRange=2.6f, dodgeSpeed=11f;
    public static int TouchMove;
    public static bool TouchJump, TouchAttack, TouchDodge;
    private Rigidbody body;
    private bool grounded;
    private float attackCooldown, dodgeTimer;
    private Vector3 dodgeDirection=Vector3.forward;
    private Transform visualRoot;
    private void Awake(){body=GetComponent<Rigidbody>();visualRoot=transform.Find("Stylized Hero");}
    private void Update()
    {
        if(GameBootstrap.IsGameOver)return;
        attackCooldown-=Time.deltaTime;
        float x=Input.GetAxisRaw("Horizontal"), z=Input.GetAxisRaw("Vertical");
        if(TouchMove!=0)x=TouchMove;
        Vector3 input=new Vector3(x,0,z).normalized;
        Camera cam=Camera.main;
        Vector3 forward=cam?cam.transform.forward:Vector3.forward, right=cam?cam.transform.right:Vector3.right;
        forward.y=0;right.y=0;forward.Normalize();right.Normalize();
        Vector3 move=(forward*input.z+right*input.x).normalized;
        if(move.sqrMagnitude>.01f){transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(move),14f*Time.deltaTime);dodgeDirection=move;}
        bool sprint=Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift);
        Vector3 velocity=body.linearVelocity;
        float speed=moveSpeed*(sprint?sprintMultiplier:1f);
        if(dodgeTimer>0){dodgeTimer-=Time.deltaTime;body.linearVelocity=new Vector3(dodgeDirection.x*dodgeSpeed,velocity.y,dodgeDirection.z*dodgeSpeed);}
        else body.linearVelocity=new Vector3(move.x*speed,velocity.y,move.z*speed);
        bool jump=Input.GetKeyDown(KeyCode.Space)||TouchJump;TouchJump=false;
        if(jump&&grounded){body.AddForce(Vector3.up*jumpForce,ForceMode.VelocityChange);grounded=false;}
        bool dodge=Input.GetKeyDown(KeyCode.K)||TouchDodge;TouchDodge=false;
        if(dodge&&grounded&&dodgeTimer<=0)dodgeTimer=.22f;
        bool attack=Input.GetKeyDown(KeyCode.J)||Input.GetMouseButtonDown(0)||TouchAttack;TouchAttack=false;
        if(attack&&attackCooldown<=0){attackCooldown=.42f;Attack();}
        if(visualRoot)visualRoot.localRotation=Quaternion.Euler(0,0,Mathf.Sin(Time.time*8f)*Mathf.Min(input.magnitude,.08f));
    }
    private void Attack()
    {
        var enemies=FindObjectsByType<TrainingEnemy>(FindObjectsSortMode.None);
        TrainingEnemy closest=null;float best=attackRange;
        foreach(var enemy in enemies){if(!enemy)continue;Vector3 d=enemy.transform.position-transform.position;d.y=0;float dist=d.magnitude;
            if(dist<best&&Vector3.Dot(transform.forward,d.normalized)>.05f){best=dist;closest=enemy;}}
        if(closest)closest.TakeHit();
    }
    private void OnCollisionStay(Collision c){foreach(ContactPoint p in c.contacts)if(p.normal.y>.55f)grounded=true;}
    private void OnCollisionExit(Collision c){grounded=false;}
}
