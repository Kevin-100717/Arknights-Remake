using GameData.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 dist;
    public Damage damage;
    public Character target;
    public float speed;
    void Start()
    {
        dist = new Vector3(dist.x,dist.y,transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        if (target.isDead || target.state == CharacterState.Die)
        {
            Destroy(gameObject);
        }
        Vector3 dir = (dist - transform.position).normalized;
        Vector3 velocity = dir * speed;
        transform.position += velocity * Time.deltaTime;
        if(Vector3.Distance(dist,transform.position) < 0.2f)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
