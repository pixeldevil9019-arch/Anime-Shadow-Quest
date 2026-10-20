using UnityEngine;

public sealed class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset=new Vector3(0,4.2f,-6.5f);
    public float smoothSpeed=8f, sensitivity=2.2f;
    private Vector2 lastPointer;
    private bool wasTouching;
    private void LateUpdate()
    {
        if(!target)return;
        if(Input.GetMouseButtonDown(1))lastPointer=Input.mousePosition;
        if(Input.GetMouseButton(1))
        {
            Vector2 now=Input.mousePosition;ApplyOrbit(now-lastPointer);lastPointer=now;
        }
        bool touchOnRight=false;
        if(Input.touchCount==1)
        {
            Touch touch=Input.GetTouch(0);
            touchOnRight=touch.position.x>Screen.width*.35f;
            if(touchOnRight)
            {
                if(touch.phase==TouchPhase.Began)lastPointer=touch.position;
                else if(touch.phase==TouchPhase.Moved)ApplyOrbit(touch.position-lastPointer);
                lastPointer=touch.position;
            }
        }
        wasTouching=touchOnRight;
        Quaternion rotation=Quaternion.Euler(pitch,yaw,0);
        Vector3 desired=target.position+rotation*offset;
        transform.position=Vector3.Lerp(transform.position,desired,1-Mathf.Exp(-smoothSpeed*Time.deltaTime));
        transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(target.position+Vector3.up*1.15f-transform.position),1-Mathf.Exp(-smoothSpeed*Time.deltaTime));
    }
    private float yaw,pitch=18f;
    private void ApplyOrbit(Vector2 delta){yaw+=delta.x*sensitivity*.1f;pitch=Mathf.Clamp(pitch-delta.y*sensitivity*.08f,-5f,45f);}
}
