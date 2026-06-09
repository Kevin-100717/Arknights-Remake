using GameData.Game;
using Spine;
using Spine.Unity;
using System.Collections;
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

    [Header("内部引用")]
    private Enemy enemy;              // 对父级 Enemy 脚本的引用
    private float attackTimer = 0f;   // 攻击冷却计时器
    private List<Character> detectedCharacters = new List<Character>(); // 检测到的角色列表
    private bool isAttacking = false; // 是否正在执行攻击动画

    private void Start()
    {
        enemy = GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogError("Shoot 组件必须附加到拥有 Enemy 脚本的游戏对象上！", this);
        }

        // 定期检测范围内的角色
        InvokeRepeating("DetectCharactersInRange", 0.1f, 0.5f);
    }

    private void OnEnable()
    {
        // 组件启用时重置状态
        isAttacking = false;
        attackTimer = 0f;
    }

    private void Update()
    {
        // 基本的生存检查
        if (enemy == null || enemy.isDead || enemy.state == EnemyState.Die)
        {
            return;
        }

        // 更新攻击计时器
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        // 尝试寻找并锁定目标
        TryAcquireAndAttackTarget();
    }

    /// <summary>
    /// 检测范围内的角色
    /// </summary>
    private void DetectCharactersInRange()
    {
        if (!isActiveAndEnabled || enemy == null || enemy.isDead) return;

        detectedCharacters.Clear();

        // 查找场景中所有带有 Character 脚本的对象
        Character[] allCharacters = FindObjectsOfType<Character>();

        foreach (Character character in allCharacters)
        {
            if (character != null && !character.isDead && character.state != CharacterState.Die &&
                character.state != CharacterState.Start &&
                character.state != CharacterState.Place
                )
            {
                float distance = Vector3.Distance(transform.position, character.transform.position);
                if (distance <= atk_range)
                {
                    // 检查角色是否已经被足够多的敌人阻挡
                    if (CanAttackCharacter(character))
                    {
                        detectedCharacters.Add(character);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 检查是否可以攻击指定角色（基于def_num限制）
    /// </summary>
    private bool CanAttackCharacter(Character character)
    {
        // 计算当前有多少敌人正在阻挡这个角色
        int blockingEnemies = 0;

        // 查找所有敌人
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        foreach (Enemy e in allEnemies)
        {
            if (e != null && !e.isDead &&
                (e.state == EnemyState.Attack ||
                 e.state == EnemyState.Attack_interval ||
                 e.state == EnemyState.EmptyIdle) &&
                e.atk_target == character)
            {
                blockingEnemies++;
            }
        }

        // 如果阻挡数量小于角色的最大阻挡数，则可以攻击
        return blockingEnemies < character.charData.def_num;
    }

    /// <summary>
    /// 寻找范围内的玩家并尝试攻击
    /// </summary>
    private void TryAcquireAndAttackTarget()
    {
        // 如果正在攻击动画中，不进行新的攻击
        if (!isActiveAndEnabled || isAttacking) return;

        if (attackTimer <= 0 && detectedCharacters.Count > 0)
        {
            // 从检测到的角色中找到第一个活着的目标
            foreach (var character in detectedCharacters)
            {
                if (character != null && !character.isDead && character.state != CharacterState.Die)
                {
                    // 再次确认距离（因为可能在检测间隔期间移动了）
                    Vector3 a = new Vector3(transform.position.x, transform.position.y,0);
                    Vector3 b = new Vector3(character.transform.position.x, character.transform.position.y, 0);
                    float distance = Vector3.Distance(a,b);
                    if (distance <= atk_range)
                    {
                        // 再次检查是否还能攻击这个角色
                        if (CanAttackCharacter(character))
                        {
                            // 成功找到可攻击的目标
                            PerformAttack(character);
                            return; // 每次Update只攻击一次
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 执行攻击动作
    /// </summary>
    /// <param name="target">被攻击的角色</param>
    private void PerformAttack(Character target)
    {
        // 通知Enemy脚本即将进入射击状态
        if (enemy.state != EnemyState.Shoot)
        {
            enemy.stateBeforeShoot = enemy.state; // 保存当前状态
            enemy.state = EnemyState.Shoot;       // 切换到射击状态
            isAttacking = true;                   // 标记为正在攻击
        }

        // 重置攻击计时器
        attackTimer = atk_interval;
    }

    /// <summary>
    /// 攻击回调函数 - 这个方法将在UI中通过Spine事件绑定调用
    /// 当射击动画播放完成后调用此方法
    /// </summary>
    public void OnAttack()
    {
        // 只有在组件启用时才执行攻击逻辑
        if (!isActiveAndEnabled) return;

        // 实际伤害计算等逻辑在这里实现或由其他系统处理
        Debug.Log($"Enemy [{gameObject.name}] performed a ranged attack!");

        if (bulletPrefab != null && !string.IsNullOrEmpty(bone) && detectedCharacters.Count > 0)
        {
            Bone b = enemy.spineAnimation.Skeleton.FindBone(bone);
            if (b != null)
            {
                Vector3 pos = b.GetWorldPosition(enemy.spineAnimation.transform);
                GameObject bp = Instantiate(bulletPrefab, pos, Quaternion.identity);
                bp.GetComponent<BulletController>().dist = detectedCharacters[0].transform.position;
                bp.GetComponent<BulletController>().target = detectedCharacters[0];
                bp.GetComponent<BulletController>().damage = enemy.enemyData.damage;
            }
        }

        // 攻击结束后，恢复到之前的状态
        if (enemy != null && !enemy.isDead)
        {
            enemy.state = enemy.stateBeforeShoot;
            isAttacking = false; // 标记攻击结束
        }
    }

    // 在Scene视图中可视化攻击范围（仅在编辑器中显示）
    private void OnDrawGizmosSelected()
    {
        if (isActiveAndEnabled)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, atk_range);
        }
    }
}
