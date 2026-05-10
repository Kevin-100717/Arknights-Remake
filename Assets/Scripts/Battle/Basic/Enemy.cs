using GameData.MapData;
using GameData.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using DG.Tweening;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public EnemyMove moveController;
    public RouteEntity route;
    public EnemyData.EnemyType enemyType;

    [Header("Animation")]
    public List<SpineAnimationData> animations;
    public SkeletonAnimation spineAnimation;

    [Header("Data")]
    public EnemyData enemyData;
    public HpUIController hpUIController;

    [Header("Runtime State")]
    public EnemyState state;
    public Character atk_target;
    public bool isReWrite = false; // 是否重写Update方法，重写后将不执行默认逻辑

    // 新增：用于记录进入Shoot状态前的状态
    public EnemyState stateBeforeShoot = EnemyState.Idle;

    private int currentPathIndex = -1;
    private float pathWaitTime; // 路径等待时间专用计时器
    private float attackIntervalTimer; // 攻击间隔专用计时器
    private SpineAnimationData currentAnim;
    private bool hasReachedEnd = false;
    private bool onAttack = false;
    public bool isDead = false;
    private Dictionary<EnemyState, SpineAnimationData> animationMap;
    private EnemyState stateBeforeAttack = EnemyState.Idle; // 记录攻击前的状态
    private Shoot shootComponent; // 对Shoot组件的引用
    private bool wasInMeleeState = false; // 记录上一帧是否处于近战状态

    void Awake()
    {
        InitializeAnimationMap();
    }

    void Start()
    {
        if(isReWrite) return; // 如果重写了Update方法，跳过默认初始化逻辑
        ValidateAndInitialize();
        BindSpineAnimationEvents();

        // 获取Shoot组件引用
        shootComponent = GetComponent<Shoot>();
    }

    void InitializeAnimationMap()
    {
        animationMap = new Dictionary<EnemyState, SpineAnimationData>();
        foreach (var data in animations)
        {
            if (!animationMap.ContainsKey(data.state))
                animationMap[data.state] = data;
        }
    }

    void ValidateAndInitialize()
    {
        Debug.Assert(enemyData != null, "EnemyData is missing!", this);
        Debug.Assert(route != null, "RouteEntity is missing!", this);
        Debug.Assert(spineAnimation != null, "SkeletonAnimation component is missing!", this);

        enemyType = enemyData.type;
        enemyData.hp_current = enemyData.hp_total;
        state = EnemyState.Idle;

        if (enemyType == EnemyData.EnemyType.Normal)
        {
            moveController = GetComponent<EnemyMove>();
            if (moveController != null)
                moveController.speed = enemyData.speed * 0.5f;
        }
        else
        {
            FlyMove flyMove = GetComponent<FlyMove>();
            if (flyMove != null)
            {
                flyMove.speed = enemyData.speed * 0.5f;
                transform.position = new Vector3(transform.position.x, transform.position.y, -1);
            }
        }

        if (hpUIController != null)
            hpUIController.hpBar.gameObject.SetActive(false);

        UpdateCheckPoint();
    }

    void BindSpineAnimationEvents()
    {
        spineAnimation.state.Start -= OnSpineAnimationStart;
        spineAnimation.state.Complete -= OnSpineAnimationComplete;

        spineAnimation.state.Start += OnSpineAnimationStart;
        spineAnimation.state.Complete += OnSpineAnimationComplete;
    }

    void OnSpineAnimationStart(Spine.TrackEntry trackEntry)
    {
        if (trackEntry.Animation.Name == GetAnimationName(EnemyState.Die))
        {
            // 在死亡动画开始时也释放阻挡位
            if (atk_target != null)
            {
                if (atk_target.charData.current_def_num > 0)
                {
                    atk_target.charData.current_def_num--;
                }
            }
            // 清空目标引用
            atk_target = null;
        }
    }


    void OnSpineAnimationComplete(Spine.TrackEntry trackEntry)
    {
        string currentAnimationName = trackEntry.Animation.Name;

        if (currentAnimationName == GetAnimationName(EnemyState.Attack))
        {
            attackIntervalTimer = enemyData.atk_interval;
            state = EnemyState.Attack_interval; // 改为Attack_interval状态
        }
        // 注意：Shoot 状态的完成通常不改变状态，
        // 因为它是由 Shoot.cs 脚本控制返回原状态的。
        else if (currentAnimationName == GetAnimationName(EnemyState.Die))
        {
            isDead = true;
            Destroy(gameObject);
        }
        // Shoot动画完成时的处理
        else if (currentAnimationName == GetAnimationName(EnemyState.Shoot))
        {
            // Shoot动画完成后自动回到之前状态已在Shoot.cs中处理
            // 这里可以留空或者做额外处理
        }
    }

    string GetAnimationName(EnemyState state)
    {
        if (animationMap != null && animationMap.TryGetValue(state, out var data))
            return data.animationName;
        return "";
    }

    public void TakeDamage(Damage damage)
    {
        if (isDead || state == EnemyState.Die) return;

        switch (damage.type)
        {
            case DamageType.Physical:
                enemyData.hp_current -= Mathf.Max(damage.damage - enemyData.def, 0.05f * damage.damage);
                break;
            case DamageType.Magic:
                enemyData.hp_current -= Mathf.Max(damage.damage * (1 - 0.01f * enemyData.magic_def), 0.05f * damage.damage);
                break;
        }

        enemyData.hp_current = Mathf.Max(0, enemyData.hp_current);

        if (enemyData.hp_current <= 0)
        {
            Die();
        }
        else
        {
            UpdateHealthBar();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        enemyData.hp_current = 0;
        state = EnemyState.Die;

        // 释放阻挡位 - 这是关键修复！
        if (atk_target != null)
        {
            if (atk_target.charData.current_def_num > 0)
            {
                atk_target.charData.current_def_num--;
            }
        }

        // 重置所有与攻击相关的状态和引用
        atk_target = null;
        onAttack = false;
        stateBeforeAttack = EnemyState.Idle;
        stateBeforeShoot = EnemyState.Idle; // 也重置射击前状态

        // 启用Shoot组件，以防死亡时被禁用
        if (shootComponent != null)
        {
            shootComponent.enabled = true;
        }

        if (hpUIController != null)
            hpUIController.hpBar.gameObject.SetActive(false);

        PlayDeathAnimation();
    }

    void PlayDeathAnimation()
    {
        string deathAnimName = GetAnimationName(EnemyState.Die);
        if (!string.IsNullOrEmpty(deathAnimName))
        {
            spineAnimation.state.SetAnimation(0, deathAnimName, false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void UpdateCheckPoint()
    {
        currentPathIndex++;

        // 如果还没走到最后一个 checkpoint，则按顺序处理
        if (currentPathIndex < route.Checkpoints.Count)
        {
            ProcessCheckpoint();
            return;
        }

        // 已经走过了所有的 checkpoints，现在开始往终点走
        if (!hasReachedEnd)
        {
            Vector3 endPosition = new Vector3(
                route.EndPosition.Col,
                route.EndPosition.Row,
                enemyType == EnemyData.EnemyType.Normal ? 0 : -1);

            if (enemyType == EnemyData.EnemyType.Normal && moveController != null)
            {
                moveController.UpdatePath(endPosition);
                state = EnemyState.Move;
            }
            else
            {
                FlyMove flyMove = GetComponent<FlyMove>();
                if (flyMove != null)
                {
                    flyMove.SetTarget(endPosition);
                    state = EnemyState.Move;
                }
            }

            hasReachedEnd = true;
        }
        else
        {
            // 已经到达过终点 -> 扣血并销毁自己
            GameController.instance.life--;
            Destroy(gameObject);
        }
    }

    void ProcessCheckpoint()
    {
        CheckpointEntity checkpoint = route.Checkpoints[currentPathIndex];

        switch (checkpoint.Type)
        {
            case "MOVE":
                Vector3 targetPosition = GetPosition(currentPathIndex);

                if (enemyType == EnemyData.EnemyType.Normal && moveController != null)
                {
                    moveController.UpdatePath(targetPosition);
                    state = EnemyState.Move;
                }
                else
                {
                    FlyMove flyMove = GetComponent<FlyMove>();
                    if (flyMove != null)
                    {
                        flyMove.SetTarget(new Vector3(targetPosition.x, targetPosition.y, -1));
                        state = EnemyState.Move;
                    }
                }
                break;

            case "WAIT_FOR_SECONDS":
            case "WAIT_CURRENT_WAVE_TIME":
                state = EnemyState.Idle;
                pathWaitTime = checkpoint.Time; // 使用专用计时器
                break;
            case "DISAPPEAR":
                GetComponent<MeshRenderer>().enabled = false;
                UpdateCheckPoint();
                break;
            case "APPEAR_AT_POS":
                GetComponent<MeshRenderer>().enabled = true;
                transform.position = GetPosition(currentPathIndex);
                UpdateCheckPoint();
                break;
        }
    }

    Vector3 GetPosition(int idx)
    {
        CheckpointEntity checkpoint = route.Checkpoints[idx];
        return new Vector3(
            (float)checkpoint.Position.Col + checkpoint.ReachOffset.X,
            (float)checkpoint.Position.Row + checkpoint.ReachOffset.Y,
            0);
    }

    void UpdateAnimation()
    {
        if (currentAnim == null || state != currentAnim.state)
        {
            if (animationMap.TryGetValue(state, out currentAnim))
            {
                spineAnimation.state.SetAnimation(0, currentAnim.animationName, currentAnim.loop);
            }
        }
    }

    void Update()
    {
        if (isDead) return;
        UpdateAnimation();
        if (isReWrite) return;
        // 控制Shoot组件的启用/禁用
        ControlShootComponent();

        UpdateAnimation();
        ProcessState();
    }

    /// <summary>
    /// 控制Shoot组件的启用/禁用
    /// </summary>
    void ControlShootComponent()
    {
        bool isInMeleeState = (enemyData.haveNear &&
                              (state == EnemyState.Attack ||
                               state == EnemyState.Attack_interval ||
                               state == EnemyState.EmptyIdle));

        // 只有在状态真正改变时才操作组件
        if (isInMeleeState != wasInMeleeState)
        {
            if (shootComponent != null)
            {
                if (isInMeleeState)
                {
                    // 进入近战状态，禁用远程攻击
                    shootComponent.enabled = false;
                }
                else
                {
                    // 退出近战状态，启用远程攻击
                    shootComponent.enabled = true;
                }
            }
            wasInMeleeState = isInMeleeState;
        }
    }

    void ProcessState()
    {
        switch (state)
        {
            case EnemyState.Move:
                ProcessMovement();
                break;

            case EnemyState.Idle:
                ProcessIdle();
                break;

            case EnemyState.EmptyIdle: // 新增状态处理
                ProcessEmptyIdle();
                break;

            case EnemyState.Attack_interval: // 处理Attack_interval状态
                ProcessAttackInterval();
                break;

            case EnemyState.Attack:
                ProcessAttack();
                break;

            case EnemyState.Shoot: // 新增状态处理
                ProcessShoot();
                break;

            case EnemyState.Die:
                // Die状态不需要特殊处理，动画完成后会Destroy
                break;
        }
    }

    void ProcessMovement()
    {
        if (enemyType == EnemyData.EnemyType.Normal && moveController != null)
        {
            moveController.NextTarget();
            if (moveController.reachedEndOfPath)
            {
                UpdateCheckPoint();
            }
        }
        else
        {
            FlyMove flyMove = GetComponent<FlyMove>();
            if (flyMove != null)
            {
                flyMove.Move();
                if (flyMove.reachedEndOfPath)
                {
                    UpdateCheckPoint();
                }
            }
        }
    }

    void ProcessIdle()
    {
        if (pathWaitTime > 0)
        {
            pathWaitTime -= Time.deltaTime;
        }
        else
        {
            UpdateCheckPoint();
        }
    }

    // 新增: EmptyIdle 状态下的处理逻辑
    void ProcessEmptyIdle()
    {
        // 在此状态下，敌人什么都不做，只是站着不动。
        // 可以在这里添加一些 idle 的行为，比如播放 idle 动画。
        // 当前逻辑是空的，因为状态本身就意味着"等待"。
    }

    // 新增: Shoot 状态下的处理逻辑 (主要是动画播放)
    void ProcessShoot()
    {
        // 动画播放由 Spine 控制，这里可以留空或做一些辅助逻辑
        // 例如检查是否仍在范围内等，但这主要由 Shoot.cs 脚本管理
    }

    void ProcessAttackInterval()
    {
        // Attack_interval倒计时
        attackIntervalTimer -= Time.deltaTime;
        if (attackIntervalTimer <= 0)
        {
            // 检查目标是否还存在且存活
            if (atk_target != null && !atk_target.isDead && atk_target.state != CharacterState.Die)
            {
                state = EnemyState.Attack; // 倒计时结束，继续攻击
            }
            else
            {
                // 目标已死亡或丢失，恢复到攻击前状态
                state = stateBeforeAttack;
                atk_target = null;
                onAttack = false;
            }
        }
    }

    void ProcessAttack()
    {
        // 攻击逻辑已经在DoDamage中处理
    }

    public void DoDamage()
    {
        if (isDead || atk_target == null) return;

        // 再次检查目标是否存活，防止在动画播放期间目标死亡
        if (atk_target.isDead || atk_target.state == CharacterState.Die)
        {
            // 目标已死亡，停止攻击并恢复状态
            state = stateBeforeAttack;
            atk_target = null;
            onAttack = false;
            return;
        }

        atk_target.TakeDamage(enemyData.damage);
    }

    void UpdateHealthBar()
    {
        if (hpUIController == null) return;

        hpUIController.hpBar.gameObject.SetActive(true);
        float ratio = enemyData.hp_current / enemyData.hp_total;
        hpUIController.white.DOScaleX(ratio, 0.3f);
        hpUIController.red.DOScaleX(ratio, 0.1f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead || state == EnemyState.Die) return;

        if (other.gameObject.CompareTag("Character"))
        {
            Character character = other.gameObject.GetComponent<Character>();
            if (character != null &&
                character.state != CharacterState.Die)
            {
                // 新增逻辑: 检查敌人是否有近战能力
                if (!enemyData.haveNear)
                {
                    // 没有近战能力，被阻挡时进入 EmptyIdle 状态
                    // 检查是否还能被阻挡（def_num限制）
                    if (character.charData.current_def_num < character.charData.def_num)
                    {
                        stateBeforeAttack = GetLastState();
                        character.charData.current_def_num++; // 占用阻挡位
                        atk_target = character; // 保留引用以便在退出时释放
                        state = EnemyState.EmptyIdle; // 切换到空闲状态
                        onAttack = false; // 不进行攻击
                        return; // 结束，不执行下面的攻击逻辑
                    }
                    else
                    {
                        // 无法被阻挡，继续移动
                        return;
                    }
                }

                // 有近战能力，检查是否还能被阻挡
                if (character.charData.current_def_num < character.charData.def_num)
                {
                    // 有近战能力，执行原有攻击逻辑
                    // 记录攻击前的状态
                    stateBeforeAttack = GetLastState();
                    character.charData.current_def_num++;
                    atk_target = character;
                    state = EnemyState.Attack;
                    onAttack = true;
                    attackIntervalTimer = 0; // 重置攻击间隔计时器
                }
            }
        }
    }

    EnemyState GetLastState()
    {
        if (state != EnemyState.EmptyIdle)
        {
            return state;
        }
        else
        {
            return stateBeforeAttack;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isDead || state == EnemyState.Die) return;

        if (other.gameObject.CompareTag("Character"))
        {
            Character character = other.gameObject.GetComponent<Character>();
            if (character == atk_target)
            {
                // 释放阻挡位
                if (character.charData.current_def_num > 0)
                {
                    character.charData.current_def_num--;
                }

                atk_target = null;
                // 恢复到攻击前的状态
                state = stateBeforeAttack;
                onAttack = false;
                attackIntervalTimer = 0; // 重置攻击间隔计时器
            }
        }
    }

    void OnDestroy()
    {
        GameController.instance.enemyCount++;
        if (spineAnimation != null && spineAnimation.state != null)
        {
            spineAnimation.state.Start -= OnSpineAnimationStart;
            spineAnimation.state.Complete -= OnSpineAnimationComplete;
        }
    }
}
