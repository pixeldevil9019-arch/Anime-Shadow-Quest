using UnityEngine;
public sealed class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset=new Vector3(0,4.2f,-6.5f);
    public float smoothSpeed=8f, sensitivity=2.2f;
    private float yaw, pitch=18f;
    private Vector2 lastTouch;
    private bool dragging;
    private void LateUpdate()
    {
        if(!target)return;
        if(Input.GetMouseButtonDown(1))lastTouch=Input.mousePosition;
        if(Input.GetMouseButton(1)){Vector2 now=Input.mousePosition;Vector2 delta=now-lastTouch;lastTouch=now;yaw+=delta.x*sensitivity*.1f;pitch=Mathf.Clamp(pitch-delta.y*sensitivity*.08f,-5,45);}
        Quaternion rotation=Quaternion.Euler(pitch,yaw,0);
        Vector3 desired=target.position+rotation*offset;
        transform.position=Vector3.Lerp(transform.position,desired,1-Mathf.Exp(-smoothSpeed*Time.deltaTime));
        transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(target.position+Vector3.up*1.15f-transform.position),1-Mathf.Exp(-smoothSpeed*Time.deltaTime));
    }
}
