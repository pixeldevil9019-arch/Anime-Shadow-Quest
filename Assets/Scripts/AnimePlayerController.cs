using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public sealed class AnimePlayerController : MonoBehaviour
{
    public float moveSpeed=5.8f, sprintMultiplier=1.55f, jumpForce=6.2f, attackRange=2.8f, dodgeSpeed=11f;
    public static int TouchMove;
    public static bool TouchJump, TouchAttack, TouchDodge;
    private Rigidbody body;
    private bool grounded;
    private float attackCooldown, dodgeTimer, swingTimer;
    private Vector3 dodgeDirection=Vector3.forward;
    private Transform visualRoot, blade;
    private void Awake()
    {
        body=GetComponent<Rigidbody>();
        visualRoot=transform.Find("Stylized Hero");
        if(visualRoot)blade=visualRoot.Find("Spirit Blade");
    }
    private void Update()
    {
        if(GameBootstrap.IsGameOver)return;
        attackCooldown-=Time.deltaTime;
        if(swingTimer>0)
        {
            swingTimer-=Time.deltaTime;
            if(blade)blade.localRotation=Quaternion.Slerp(blade.localRotation,Quaternion.identity,14f*Time.deltaTime);
        }
        float x=Input.GetAxisRaw("Horizontal"),z=Input.GetAxisRaw("Vertical");
        if(TouchMove!=0)x=TouchMove;
        Vector3 input=new Vector3(x,0,z).normalized;
        Camera cam=Camera.main;
        Vector3 forward=cam?cam.transform.forward:Vector3.forward,right=cam?cam.transform.right:Vector3.right;
        forward.y=0;right.y=0;forward.Normalize();right.Normalize();
        Vector3 move=(forward*input.z+right*input.x).normalized;
        if(move.sqrMagnitude>.01f)
        {
            transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(move),14f*Time.deltaTime);
            dodgeDirection=move;
        }
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
        if(attack&&attackCooldown<=0){attackCooldown=.38f;swingTimer=.18f;Attack();}
        if(visualRoot)visualRoot.localRotation=Quaternion.Euler(0,0,Mathf.Sin(Time.time*8f)*Mathf.Min(input.magnitude,.08f));
    }
    private void Attack()
    {
        if(blade)blade.localRotation=Quaternion.Euler(-18f,0,-72f);
        GameObject slash=GameObject.CreatePrimitive(PrimitiveType.Cube);
        slash.name="Spirit Slash VFX";
        slash.transform.position=transform.position+transform.forward*1.15f+Vector3.up*.85f;
        slash.transform.rotation=Quaternion.LookRotation(transform.forward);
        slash.transform.localScale=new Vector3(1.45f,.07f,.16f);
        Collider collider=slash.GetComponent<Collider>();if(collider)collider.enabled=false;
        Renderer renderer=slash.GetComponent<Renderer>();
        Shader shader=Shader.Find("Universal Render Pipeline/Unlit");if(shader==null)shader=Shader.Find("Unlit/Color");if(shader==null)shader=Shader.Find("Standard");
        Material material=new Material(shader);material.color=new Color(.15f,.9f,1f,.8f);renderer.sharedMaterial=material;
        Destroy(slash,.14f);Destroy(material,.2f);
        TrainingEnemy[] enemies=FindObjectsByType<TrainingEnemy>(FindObjectsSortMode.None);
        foreach(TrainingEnemy enemy in enemies)
        {
            if(!enemy)continue;
            Vector3 delta=enemy.transform.position-transform.position;delta.y=0;
            float distance=delta.magnitude;
            if(distance<attackRange&&distance>.01f&&Vector3.Dot(transform.forward,delta.normalized)>.05f)enemy.TakeHit();
        }
    }
    private void OnCollisionStay(Collision c){foreach(ContactPoint p in c.contacts)if(p.normal.y>.55f)grounded=true;}
    private void OnCollisionExit(Collision c){grounded=false;}
}
