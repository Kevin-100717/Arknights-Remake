using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyMove : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 target;
    public float speed = 2;
    public bool reachedEndOfPath;
    public float nextWaypointDistance;
    void Start()
    {
        
    }
    public void SetTarget(Vector3 target)
    {
        reachedEndOfPath = false;
        this.target = target;
    }
    public void Move()
    {
        if(Vector3.Distance(transform.position, target) < nextWaypointDistance)
        {
            reachedEndOfPath = true;
            return;
        }
        Vector3 dir = (target - transform.position).normalized;
        Vector3 velocity = dir * speed;
        transform.position += velocity * Time.deltaTime;
        if (target.x > transform.position.x)
        {
            transform.eulerAngles = new Vector3(-30, 0, 0);
        }
        else
        {
            transform.eulerAngles = new Vector3(30, 180, 0);

        }
        //MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        //propertyBlock.SetFloat("_angle", target.x <= transform.position.x ? -30f : 60f);
        //GetComponent<MeshRenderer>().SetPropertyBlock(propertyBlock);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
