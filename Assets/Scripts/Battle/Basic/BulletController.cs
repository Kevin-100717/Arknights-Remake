using GameData.Game;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public Vector3 dist;
    public Damage damage;
    public Character target;
    public float speed;
    public bool useMove = true;

    private bool hitApplied;
    private bool drivenByPaoDan;

    void Start()
    {
        dist = new Vector3(dist.x, dist.y, transform.position.z);
        drivenByPaoDan = TryGetComponent<PaoDan>(out _);
    }

    void Update()
    {
        if (hitApplied) return;

        // 目标失效：弹道落空
        if (target == null || target.isDead || target.state == CharacterState.Die)
        {
            Destroy(gameObject);
            return;
        }

        // 追踪目标当前位置
        dist = new Vector3(target.transform.position.x, target.transform.position.y, transform.position.z);

        if (useMove)
        {
            if (speed <= 0)
            {
                // 未配置速度视为即时命中，避免弹道永久滞留
                Hit();
                return;
            }
            float step = speed * Time.deltaTime;
            float remain = Vector3.Distance(dist, transform.position);
            if (remain <= Mathf.Max(step, 0.2f))
            {
                Hit();
                return;
            }
            Vector3 dir = (dist - transform.position).normalized;
            transform.position += dir * step;
        }
        else if (!drivenByPaoDan)
        {
            // 既不自移动也无抛物线组件：按即时命中处理
            Hit();
        }
    }

    /// <summary>统一的伤害结算入口，保证一次攻击只结算一次。</summary>
    public void Hit()
    {
        if (hitApplied) return;
        hitApplied = true;

        if (target != null && !target.isDead && target.state != CharacterState.Die)
            target.TakeDamage(damage);

        Destroy(gameObject);
    }
}
