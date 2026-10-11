using Pathfinding;
using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    public float speed = 2;
    public float nextWaypointDistance;
    public bool reachedEndOfPath;
    [HideInInspector]
    public Vector3 nextTargetPosition;
    public Seeker seeker;

    private Path path;
    private int currentWaypoint;
    private Vector3 exactTarget;                                // 精确目标（含 reachOffset 小数）
    private Vector3 targetLastPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock propertyBlock;

    public void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            // 寻路节点是整数格心，需把终点替换为精确目标（保留 reachOffset 等小数），
            // 否则敌人会在格心停下，位置像被 int 取整
            if (p.vectorPath != null && p.vectorPath.Count > 0)
                p.vectorPath[p.vectorPath.Count - 1] = exactTarget;
            path = p;
            currentWaypoint = 0;
        }
    }

    public void UpdatePath(Vector3 targetPosition)
    {
        // 阈值极小：仅拦截完全相同的重复目标，避免 reachOffset 小数差异被误判
        if (Vector3.Distance(targetPosition, targetLastPosition) <= 0.0001f) return;

        targetLastPosition = targetPosition;
        exactTarget = targetPosition;
        reachedEndOfPath = false;
        seeker.StartPath(transform.position, targetPosition, OnPathComplete);
    }

    public void NextTarget()
    {
        if (path == null) return;
        reachedEndOfPath = false;

        float reachDist = Mathf.Max(nextWaypointDistance, 0.01f);
        float distanceToWaypoint;
        while (true)
        {
            distanceToWaypoint = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);
            if (distanceToWaypoint < reachDist)
            {
                if (currentWaypoint + 1 < path.vectorPath.Count)
                    currentWaypoint++;
                else
                {
                    reachedEndOfPath = true;
                    break;
                }
            }
            else
            {
                break;
            }
        }

        if (reachedEndOfPath)
        {
            // 到达时精确落位到目标点（容差内的最后一段直接对齐）
            transform.position = exactTarget;
            path = null;
            return;
        }

        nextTargetPosition = path.vectorPath[currentWaypoint];
        Vector3 toTarget = nextTargetPosition - transform.position;
        // 步长不超过剩余距离：精确停在目标点（支持半格等小数位置），避免冲过后来回抖动
        float step = Mathf.Min(speed * Time.deltaTime, toTarget.magnitude);
        transform.position += toTarget.normalized * step;

        bool movingRight = nextTargetPosition.x > transform.position.x;
        transform.eulerAngles = movingRight ? new Vector3(-30, 0, 0) : new Vector3(30, 180, 0);

        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetFloat("_angle", movingRight ? 60f : -30f);
        meshRenderer.SetPropertyBlock(propertyBlock);
    }
}
