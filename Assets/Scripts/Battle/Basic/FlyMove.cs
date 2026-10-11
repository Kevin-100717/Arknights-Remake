using UnityEngine;

public class FlyMove : MonoBehaviour
{
    public Vector3 target;
    public float speed = 2;
    public bool reachedEndOfPath;
    public float nextWaypointDistance;

    public void SetTarget(Vector3 target)
    {
        reachedEndOfPath = false;
        this.target = target;
    }

    public void Move()
    {
        float reachDist = Mathf.Max(nextWaypointDistance, 0.01f);
        if (Vector3.Distance(transform.position, target) < reachDist)
        {
            reachedEndOfPath = true;
            return;
        }

        // 步长不超过剩余距离：精确停在目标点（支持半格等小数位置）
        Vector3 toTarget = target - transform.position;
        float step = Mathf.Min(speed * Time.deltaTime, toTarget.magnitude);
        transform.position += toTarget.normalized * step;

        transform.eulerAngles = target.x > transform.position.x
            ? new Vector3(-30, 0, 0)
            : new Vector3(30, 180, 0);
    }
}
