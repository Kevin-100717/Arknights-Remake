using GameData.Game;
using Spine;
using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class Shoot : MonoBehaviour
{
    [Header("射击参数")]
    public float atk_range = 5f;      // 攻击距离
    public float atk_interval = 1f;   // 攻击间隔
    public GameObject bulletPrefab;
    [SpineBone(dataField: "skeletonRenderer")]
    public string bone;

    private Enemy enemy;              // 对父级 Enemy 脚本的引用
    private float attackTimer;        // 攻击冷却计时器
    private Character currentTarget;  // 本次射击锁定的目标
    private bool isAttacking;         // 是否正在执行攻击动画
    private bool hasShootAnim;
    private readonly List<Character> detectedCharacters = new List<Character>();

    private void Start()
    {
        enemy = GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogError("Shoot 组件必须附加到拥有 Enemy 脚本的游戏对象上！", this);
            enabled = false;
            return;
        }
        hasShootAnim = !string.IsNullOrEmpty(enemy.GetAnimationName(EnemyState.Shoot));

        InvokeRepeating(nameof(DetectCharactersInRange), 0.1f, 0.5f);
    }

    private void OnEnable()
    {
        isAttacking = false;
        attackTimer = 0f;
    }

    private void Update()
    {
        if (enemy == null || enemy.isDead || enemy.state == EnemyState.Die || enemy.IsDisappeared)
            return;

        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        TryAcquireAndAttackTarget();
    }

    /// <summary>检测攻击范围内的我方角色（远程攻击不受阻挡数限制）。</summary>
    private void DetectCharactersInRange()
    {
        if (!isActiveAndEnabled || enemy == null || enemy.isDead) return;

        detectedCharacters.Clear();

        foreach (Character character in FindObjectsOfType<Character>())
        {
            if (character == null || character.isDead ||
                character.state == CharacterState.Die ||
                character.state == CharacterState.Start ||
                character.state == CharacterState.Place)
                continue;

            if (PlanarDistance(character.transform.position) <= atk_range)
                detectedCharacters.Add(character);
        }
    }

    private float PlanarDistance(Vector3 other)
    {
        Vector3 a = new Vector3(transform.position.x, transform.position.y, 0);
        Vector3 b = new Vector3(other.x, other.y, 0);
        return Vector3.Distance(a, b);
    }

    private void TryAcquireAndAttackTarget()
    {
        if (!isActiveAndEnabled || isAttacking || attackTimer > 0) return;

        // 官方敌方索敌：优先攻击最后部署的我方单位（仇恨机制基础规则）
        Character target = null;
        foreach (var character in detectedCharacters)
        {
            if (!ValidTarget(character)) continue;
            if (PlanarDistance(character.transform.position) > atk_range) continue;
            if (target == null || character.deployOrder > target.deployOrder)
                target = character;
        }

        if (target != null)
            PerformAttack(target);
    }

    private void PerformAttack(Character target)
    {
        currentTarget = target;
        attackTimer = atk_interval;

        // 没有射击动画时直接结算，避免攻击动作卡死
        if (!hasShootAnim)
        {
            FireRangedAttack();
            return;
        }

        if (enemy.state != EnemyState.Shoot)
        {
            enemy.stateBeforeShoot = enemy.state;
            enemy.state = EnemyState.Shoot;
            isAttacking = true;
        }
    }

    /// <summary>
    /// 攻击回调：由 Spine 动画事件绑定调用。
    /// </summary>
    public void OnAttack()
    {
        if (!isActiveAndEnabled) return;

        FireRangedAttack();

        if (enemy != null && !enemy.isDead)
        {
            enemy.state = enemy.stateBeforeShoot;
            isAttacking = false;
        }
    }

    /// <summary>发射子弹；无子弹配置时直接结算伤害。</summary>
    private void FireRangedAttack()
    {
        Character target = ValidTarget(currentTarget) ? currentTarget : FindFallbackTarget();
        if (target == null) return;
        currentTarget = target;

        if (bulletPrefab != null && !string.IsNullOrEmpty(bone) && enemy.spineAnimation != null)
        {
            Bone b = enemy.spineAnimation.Skeleton.FindBone(bone);
            if (b != null)
            {
                Vector3 pos = b.GetWorldPosition(enemy.spineAnimation.transform);
                GameObject bp = Instantiate(bulletPrefab, pos, Quaternion.identity);
                BulletController bullet = bp.GetComponent<BulletController>();
                bullet.dist = target.transform.position;
                bullet.target = target;
                bullet.damage = enemy.enemyData.damage;
                return;
            }
        }

        target.TakeDamage(enemy.enemyData.damage);
    }

    private static bool ValidTarget(Character c)
    {
        return c != null && !c.isDead && c.state != CharacterState.Die;
    }

    /// <summary>目标中途失效时的兜底选择：同样按“最后部署”规则。</summary>
    private Character FindFallbackTarget()
    {
        Character best = null;
        foreach (var c in detectedCharacters)
        {
            if (!ValidTarget(c)) continue;
            if (PlanarDistance(c.transform.position) > atk_range) continue;
            if (best == null || c.deployOrder > best.deployOrder)
                best = c;
        }
        return best;
    }

    private void OnDrawGizmosSelected()
    {
        if (isActiveAndEnabled)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, atk_range);
        }
    }
}
