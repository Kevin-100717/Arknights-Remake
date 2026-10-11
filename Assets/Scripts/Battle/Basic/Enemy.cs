using GameData.Game;
using GameData.MapData;
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
    public bool isReWrite = false;          // 重写 Update 后不执行默认逻辑
    public EnemyState stateBeforeShoot = EnemyState.Idle;
    public bool isDead = false;

    private int currentPathIndex = -1;
    private float pathWaitTime;             // 相对等待剩余时间（WAIT_FOR_SECONDS / DISAPPEAR）
    private float pathWaitUntil = -1f;      // 绝对等待时刻（battleTime），-1 表示不使用
    private int pathWaitUntilWave = -1;     // 波次数等待目标（WAIT_BOSSRUSH_WAVE），-1 表示不使用
    private bool pausedForWaveRest;         // 处于 WAIT_BOSSRUSH_WAVE 休整中（暂时移出波次计数）
    private float attackIntervalTimer;
    private SpineAnimationData currentAnim;
    private bool hasReachedEnd = false;
    private bool isInvulnerable = false;    // DISAPPEAR 期间无敌
    private Dictionary<EnemyState, SpineAnimationData> animationMap;
    private EnemyState stateBeforeAttack = EnemyState.Idle;
    private Shoot shootComponent;
    private FlyMove flyMove;
    private MeshRenderer meshRenderer;
    private Collider[] bodyColliders;
    private bool wasInMeleeState = false;
    private bool countedAsAlive = false;        // 是否计入场上存活计数（用于波次清怪等待）
    private bool unharmfulCountAsKilled = false; // 行动完成时不扣血仅计数（isUnharmfulAndAlwaysCountAsKilled）

    /// <summary>消失状态（DISAPPEAR 期间无敌且不可被选中）。</summary>
    public bool IsDisappeared => isInvulnerable;

    void Awake()
    {
        InitializeAnimationMap();
    }

    void Start()
    {
        if (isReWrite) return;
        ValidateAndInitialize();
        BindSpineAnimationEvents();
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
        state = enemyData.haveStart ? EnemyState.Start : EnemyState.Idle;

        flyMove = GetComponent<FlyMove>();
        meshRenderer = GetComponent<MeshRenderer>();
        bodyColliders = GetComponentsInChildren<Collider>(true);

        float moveMultiplier = GameController.instance != null ? GameController.instance.moveMultiplier : 0.5f;
        if (enemyType == EnemyData.EnemyType.Normal)
        {
            moveController = GetComponent<EnemyMove>();
            if (moveController != null)
                moveController.speed = enemyData.speed * moveMultiplier;
        }
        else if (flyMove != null)
        {
            flyMove.speed = enemyData.speed * moveMultiplier;
            transform.position = new Vector3(transform.position.x, transform.position.y, -1);
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
        // 死亡动画开始时兜底释放阻挡位
        if (trackEntry.Animation.Name == GetAnimationName(EnemyState.Die))
            ReleaseBlock();
    }

    void OnSpineAnimationComplete(Spine.TrackEntry trackEntry)
    {
        string currentAnimationName = trackEntry.Animation.Name;

        if (currentAnimationName == GetAnimationName(EnemyState.Attack))
        {
            // 攻击间隔在出手时已开始计时，这里只转入间隔状态
            state = EnemyState.Attack_interval;
        }
        else if (currentAnimationName == GetAnimationName(EnemyState.Die))
        {
            isDead = true;
            Destroy(gameObject);
        }
        else if (currentAnimationName == GetAnimationName(EnemyState.Start))
        {
            state = EnemyState.Idle;
        }
        // Shoot 动画的收尾由 Shoot.cs 处理
    }

    public string GetAnimationName(EnemyState state)
    {
        if (animationMap != null && animationMap.TryGetValue(state, out var data))
            return data.animationName;
        return "";
    }

    /// <summary>按官方公式结算一次伤害/治疗。</summary>
    public void TakeDamage(Damage damage)
    {
        if (isDead || state == EnemyState.Die || isInvulnerable) return;

        switch (damage.type)
        {
            case DamageType.Physical:
                // max(0.05A, A - D)，结果按官方“就近取整”取整
                enemyData.hp_current -= GameFormula.RoundHalfEven(
                    Mathf.Max(damage.damage - enemyData.def, 0.05f * damage.damage));
                break;
            case DamageType.Magic:
                // max(0.05A, 0.01A * max(0, 100 - 法抗))，结果取整
                enemyData.hp_current -= GameFormula.RoundHalfEven(
                    Mathf.Max(0.01f * damage.damage * Mathf.Max(0f, 100f - enemyData.magic_def),
                        0.05f * damage.damage));
                break;
            case DamageType.Real:
                enemyData.hp_current -= GameFormula.RoundHalfEven(damage.damage);
                break;
            case DamageType.Recovery:
                enemyData.hp_current = Mathf.Min(enemyData.hp_total,
                    enemyData.hp_current + GameFormula.RoundHalfEven(damage.damage));
                UpdateHealthBar();
                return;
        }

        enemyData.hp_current = Mathf.Max(0, enemyData.hp_current);

        if (enemyData.hp_current <= 0)
            Die();
        else
            UpdateHealthBar();
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        enemyData.hp_current = 0;
        state = EnemyState.Die;

        ReleaseBlock();

        if (shootComponent != null)
            shootComponent.enabled = true;

        if (hpUIController != null)
            hpUIController.hpBar.gameObject.SetActive(false);

        // 只有真正被击杀才计入歼灭数
        if (enemyData.dieAddToCount && GameController.instance != null)
            GameController.instance.enemyCount++;

        PlayDeathAnimation();
    }

    void PlayDeathAnimation()
    {
        string deathAnimName = GetAnimationName(EnemyState.Die);
        if (!string.IsNullOrEmpty(deathAnimName))
            spineAnimation.state.SetAnimation(0, deathAnimName, false);
        else
            Destroy(gameObject);
    }

    void UpdateCheckPoint()
    {
        // 出场动画结束前不推进路线
        if (state == EnemyState.Start) return;

        currentPathIndex++;

        // 官方 PATROL_MOVE：巡逻点不会离开路径序列，走完后从第一个巡逻点循环
        if (currentPathIndex >= route.Checkpoints.Count)
        {
            int patrol = FirstPatrolIndex();
            if (patrol >= 0)
                currentPathIndex = patrol;
        }

        if (currentPathIndex < route.Checkpoints.Count)
        {
            ProcessCheckpoint();
            return;
        }

        if (!hasReachedEnd)
        {
            // 官方 routes 坐标：row = 世界 y，col = 世界 x，直接使用
            Vector3 endPosition = new Vector3(
                route.EndPosition.Col,
                route.EndPosition.Row,
                enemyType == EnemyData.EnemyType.Normal ? 0 : -1);
            MoveTo(endPosition, true);
            hasReachedEnd = true;
        }
        else
        {
            // 到达终点：只要设置计入歼灭数就计入；isUnharmfulAndAlwaysCountAsKilled 不扣血，
            // 普通漏怪额外扣血
            if (GameController.instance != null)
            {
                if (enemyData.dieAddToCount || unharmfulCountAsKilled)
                    GameController.instance.enemyCount++;
                if (!unharmfulCountAsKilled)
                    GameController.instance.ReduceLife();
            }
            Destroy(gameObject);
        }
    }

    int FirstPatrolIndex()
    {
        for (int i = 0; i < route.Checkpoints.Count; i++)
        {
            if (route.Checkpoints[i].Type == "PATROL_MOVE")
                return i;
        }
        return -1;
    }

    void ProcessCheckpoint()
    {
        CheckpointEntity checkpoint = route.Checkpoints[currentPathIndex];

        switch (checkpoint.Type)
        {
            case "MOVE":
            case "PATROL_MOVE":
                // MOVE 与 PATROL_MOVE 均移动到路点位置（巡逻点额外参与循环）
                MoveTo(GetPosition(currentPathIndex), false);
                break;

            case "WAIT_FOR_SECONDS":
            case "WAIT_FOR_PLAY_TIME":
            case "WAIT_CURRENT_FRAGMENT_TIME":
            case "WAIT_CURRENT_WAVE_TIME":
            case "WAIT_BOSSRUSH_WAVE":
                // 官方：停驻指令的 position 无意义（数据中恒为 0,0），
                // 以前一条移动指令到达的位置为准，原地停留等待
                EnterCheckpointWait();
                break;

            case "DISAPPEAR":
                // 官方：进入消失状态时立即停止攻击并解除阻挡，期间无敌、透明且不可被选中，
                // 等待 Time 后执行下一步（期间发生任意移动则取消消失）
                ReleaseBlock();
                if (state == EnemyState.Attack || state == EnemyState.Attack_interval || state == EnemyState.EmptyIdle)
                    state = EnemyState.Idle;
                SetDisappeared(true);
                ResetWaitFields();
                pathWaitTime = checkpoint.Time;
                state = EnemyState.Idle;
                break;

            case "APPEAR_AT_POS":
                transform.position = GetPosition(currentPathIndex);
                SetDisappeared(false);
                UpdateCheckPoint();
                break;

            default:
                // 未知指令类型（如 ALERT）：警告并直接推进，避免路线卡死
                Debug.LogWarning($"[Enemy] 未知路径点类型 '{checkpoint.Type}'，已跳过 ({gameObject.name})");
                UpdateCheckPoint();
                break;
        }
    }

    /// <summary>移动到目标路点（消失状态在移动开始时取消）。</summary>
    void MoveTo(Vector3 targetPosition, bool isEnd)
    {
        // 官方：消失状态在发生任意移动后取消
        if (isInvulnerable)
            SetDisappeared(false);

        if (enemyType == EnemyData.EnemyType.Normal && moveController != null)
        {
            moveController.UpdatePath(targetPosition);
            state = EnemyState.Move;
        }
        else if (flyMove != null)
        {
            flyMove.SetTarget(new Vector3(targetPosition.x, targetPosition.y, isEnd ? targetPosition.z : -1));
            state = EnemyState.Move;
        }
    }

    /// <summary>停驻指令的计时分类（官方“停驻”按相对时刻划分的各变体）。</summary>
    enum WaitMode
    {
        None,            // 非停驻指令
        RelativeSeconds, // WAIT_FOR_SECONDS：到位置后计时
        PlayTime,        // WAIT_FOR_PLAY_TIME：达到全局时刻
        FragmentTime,    // WAIT_CURRENT_FRAGMENT_TIME：达到本兵团开始后的时刻
        WaveTime,        // WAIT_CURRENT_WAVE_TIME：达到本波次开始后的时刻
        BossRushWave,    // WAIT_BOSSRUSH_WAVE：移出当前波次，下x个波次后被移入
    }

    static WaitMode GetWaitMode(string type)
    {
        switch (type)
        {
            case "WAIT_FOR_SECONDS": return WaitMode.RelativeSeconds;
            case "WAIT_FOR_PLAY_TIME": return WaitMode.PlayTime;
            case "WAIT_CURRENT_FRAGMENT_TIME": return WaitMode.FragmentTime;
            case "WAIT_CURRENT_WAVE_TIME": return WaitMode.WaveTime;
            case "WAIT_BOSSRUSH_WAVE": return WaitMode.BossRushWave;
            default: return WaitMode.None;
        }
    }

    void ResetWaitFields()
    {
        pathWaitTime = 0f;
        pathWaitUntil = -1f;
        pathWaitUntilWave = -1;
    }

    /// <summary>
    /// 抵达停驻路点后按分类开始等待：
    /// WAIT_FOR_SECONDS 为相对时长；PLAY/FRAGMENT/WAVE_TIME 为“达到某时刻”；
    /// WAIT_BOSSRUSH_WAVE 按波次数等待，并在此期间将自身移出波次清怪计数。
    /// </summary>
    void EnterCheckpointWait()
    {
        CheckpointEntity checkpoint = route.Checkpoints[currentPathIndex];
        ResetWaitFields();

        switch (GetWaitMode(checkpoint.Type))
        {
            case WaitMode.RelativeSeconds:
                pathWaitTime = checkpoint.Time;
                break;
            case WaitMode.PlayTime:
                pathWaitUntil = checkpoint.Time;
                break;
            case WaitMode.FragmentTime:
                pathWaitUntil = EnemySpawner.CurrentFragmentStartTime + checkpoint.Time;
                break;
            case WaitMode.WaveTime:
                pathWaitUntil = EnemySpawner.CurrentWaveStartTime + checkpoint.Time;
                break;
            case WaitMode.BossRushWave:
                // 官方：将自身移出当前波次，于下x个波次后被移入，以当前状态重新出现
                pathWaitUntilWave = EnemySpawner.CurrentWaveIndex + Mathf.Max(1, Mathf.RoundToInt(checkpoint.Time));
                if (!pausedForWaveRest)
                {
                    pausedForWaveRest = true;
                    EnemySpawner.NotifyEnemyRemoved(); // 休整期间不阻塞波次清怪等待
                }
                break;
        }
        state = EnemyState.Idle;
    }

    Vector3 GetPosition(int idx)
    {
        CheckpointEntity checkpoint = route.Checkpoints[idx];
        // 官方 routes 坐标：row = 世界 y，col = 世界 x，直接使用（保留小数）；
        // reachOffset 为相对方格中心的偏移，randomizeReachOffset 时在 ±|偏移| 内随机
        float ox = checkpoint.ReachOffset.X;
        float oy = checkpoint.ReachOffset.Y;
        if (checkpoint.RandomizeReachOffset)
        {
            ox = Random.Range(-Mathf.Abs(ox), Mathf.Abs(ox));
            oy = Random.Range(-Mathf.Abs(oy), Mathf.Abs(oy));
        }
        return new Vector3(
            checkpoint.Position.Col + ox,
            checkpoint.Position.Row + oy,
            0);
    }

    void SetDisappeared(bool hidden)
    {
        isInvulnerable = hidden;
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null) meshRenderer.enabled = !hidden;
        if (bodyColliders == null) bodyColliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider col in bodyColliders)
        {
            if (col != null)
                col.enabled = !hidden;
        }
    }

    void UpdateAnimation()
    {
        if (currentAnim != null && state == currentAnim.state) return;

        if (animationMap.TryGetValue(state, out currentAnim))
        {
            spineAnimation.state.SetAnimation(0, currentAnim.animationName, currentAnim.loop);
        }
        else
        {
            Debug.LogError($"[Enemy] 缺少状态动画: {state} ({gameObject.name})");
            // 占位避免同一状态每帧重复报错
            currentAnim = new SpineAnimationData { state = state, animationName = "", loop = false };
        }
    }

    void Update()
    {
        if (isDead || isReWrite) return;

        ValidateBlockTarget();
        ControlShootComponent();
        UpdateAnimation();
        ProcessState();
    }

    /// <summary>目标死亡/失效时释放阻挡并恢复到自由状态，避免卡死。</summary>
    void ValidateBlockTarget()
    {
        bool engaged = state == EnemyState.Attack ||
                       state == EnemyState.Attack_interval ||
                       state == EnemyState.EmptyIdle;

        bool targetValid = atk_target != null &&
                           !atk_target.isDead &&
                           atk_target.state != CharacterState.Die &&
                           atk_target.state != CharacterState.Place &&
                           atk_target.state != CharacterState.Start;

        if (targetValid) return;

        if (atk_target != null)
            ReleaseBlock();

        if (engaged)
        {
            state = GetFreeState();
            attackIntervalTimer = 0f;
        }
    }

    void ReleaseBlock()
    {
        if (atk_target != null && atk_target.charData != null && atk_target.charData.current_def_num > 0)
            atk_target.charData.current_def_num--;
        atk_target = null;
    }

    void ControlShootComponent()
    {
        bool isInMeleeState = enemyData.haveNear &&
                              (state == EnemyState.Attack ||
                               state == EnemyState.Attack_interval ||
                               state == EnemyState.EmptyIdle);

        if (isInMeleeState != wasInMeleeState)
        {
            if (shootComponent != null)
                shootComponent.enabled = !isInMeleeState;
            wasInMeleeState = isInMeleeState;
        }
    }

    void ProcessState()
    {
        switch (state)
        {
            case EnemyState.Start:
                break;
            case EnemyState.Move:
                ProcessMovement();
                break;
            case EnemyState.Idle:
                ProcessIdle();
                break;
            case EnemyState.EmptyIdle:
                // 被阻挡但无攻击能力：原地站定，等待目标失效后由 ValidateBlockTarget 恢复
                break;
            case EnemyState.Attack_interval:
                ProcessAttackInterval();
                break;
            case EnemyState.Attack:
                // 动画期间攻击间隔同步倒计时
                attackIntervalTimer -= Time.deltaTime;
                break;
            case EnemyState.Shoot:
                break;
            case EnemyState.Die:
                break;
        }
    }

    void ProcessMovement()
    {
        bool reached;
        if (enemyType == EnemyData.EnemyType.Normal && moveController != null)
        {
            moveController.NextTarget();
            reached = moveController.reachedEndOfPath;
        }
        else if (flyMove != null)
        {
            flyMove.Move();
            reached = flyMove.reachedEndOfPath;
        }
        else
        {
            return;
        }

        if (!reached) return;

        // 到达后推进：下一条若是停驻指令则由 ProcessCheckpoint 原地进入等待
        UpdateCheckPoint();
    }

    /// <summary>
    /// 停驻计时：相对秒数 → 绝对时刻 → 波次数，三类条件按序判定；
    /// 全部满足后恢复自身波次计数并执行下一条指令。
    /// </summary>
    void ProcessIdle()
    {
        if (pathWaitTime > 0f)
        {
            pathWaitTime -= Time.deltaTime;
            return;
        }

        if (pathWaitUntil >= 0f)
        {
            float now = GameController.instance != null ? GameController.instance.battleTime : Time.time;
            if (now < pathWaitUntil) return;
            pathWaitUntil = -1f;
        }

        if (pathWaitUntilWave >= 0)
        {
            if (EnemySpawner.CurrentWaveIndex < pathWaitUntilWave) return;
            pathWaitUntilWave = -1;
            // 休整结束：以当前状态重新移入波次计数
            if (pausedForWaveRest)
            {
                pausedForWaveRest = false;
                EnemySpawner.NotifyEnemyAdded();
            }
        }

        UpdateCheckPoint();
    }

    void ProcessAttackInterval()
    {
        attackIntervalTimer -= Time.deltaTime;
        if (attackIntervalTimer > 0f) return;

        if (atk_target != null && !atk_target.isDead && atk_target.state != CharacterState.Die)
        {
            state = EnemyState.Attack;
        }
        else
        {
            ReleaseBlock();
            state = GetFreeState();
        }
    }

    /// <summary>由生成方调用，将该敌人计入场上存活数（波次清怪等待依赖此计数）。</summary>
    public void NotifyAlive()
    {
        if (countedAsAlive) return;
        countedAsAlive = true;
        EnemySpawner.NotifyEnemyAdded();
    }

    public void SetUnharmfulCountAsKilled(bool value)
    {
        unharmfulCountAsKilled = value;
    }

    public void DoDamage()
    {
        if (isDead || atk_target == null) return;
        if (atk_target.isDead || atk_target.state == CharacterState.Die)
        {
            ReleaseBlock();
            state = GetFreeState();
            return;
        }

        atk_target.TakeDamage(enemyData.damage);
    }

    void UpdateHealthBar()
    {
        if (hpUIController == null || enemyData.hp_total <= 0) return;

        hpUIController.hpBar.gameObject.SetActive(true);
        float ratio = enemyData.hp_current / enemyData.hp_total;
        hpUIController.white.DOScaleX(ratio, 0.3f);
        hpUIController.red.DOScaleX(ratio, 0.1f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead || state == EnemyState.Die) return;
        if (atk_target != null) return; // 已被阻挡，不覆盖目标
        if (!other.CompareTag("Character")) return;

        Character character = other.GetComponent<Character>();
        if (character == null ||
            character.state == CharacterState.Die ||
            character.state == CharacterState.Start ||
            character.state == CharacterState.Place)
            return;

        // 阻挡位已满则继续移动
        if (character.charData.current_def_num >= character.charData.def_num) return;

        stateBeforeAttack = GetFreeState();
        character.charData.current_def_num++;
        atk_target = character;
        // 官方攻速公式换算本次攻击的间隔
        attackIntervalTimer = GameFormula.AttackInterval(enemyData.atk_interval, enemyData.attack_speed);

        if (enemyData.haveNear)
        {
            state = EnemyState.Attack;
        }
        else
        {
            // 无近战能力：被阻挡后原地站定
            state = EnemyState.EmptyIdle;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isDead || state == EnemyState.Die) return;
        if (!other.CompareTag("Character")) return;

        Character character = other.GetComponent<Character>();
        if (character == null || character != atk_target) return;

        ReleaseBlock();
        state = GetFreeState();
        attackIntervalTimer = 0f;
    }

    /// <summary>得到“未在攻击/射击时”的状态，避免把攻击态记成返回状态造成死循环。</summary>
    EnemyState GetFreeState()
    {
        switch (state)
        {
            case EnemyState.Move:
                return EnemyState.Move;
            case EnemyState.Idle:
            case EnemyState.Start:
                return EnemyState.Idle;
            default:
                return stateBeforeAttack == EnemyState.Move || stateBeforeAttack == EnemyState.Idle
                    ? stateBeforeAttack
                    : EnemyState.Move;
        }
    }

    void OnDestroy()
    {
        // 兜底释放阻挡位（死亡与漏怪销毁路径都会经过这里）
        if (atk_target != null && atk_target.charData != null && atk_target.charData.current_def_num > 0)
            atk_target.charData.current_def_num--;
        atk_target = null;

        // 从场上存活计数中移除（波次清怪等待依赖此计数；休整中已被移出则不再扣减）
        if (countedAsAlive && !pausedForWaveRest)
            EnemySpawner.NotifyEnemyRemoved();

        if (spineAnimation != null && spineAnimation.state != null)
        {
            spineAnimation.state.Start -= OnSpineAnimationStart;
            spineAnimation.state.Complete -= OnSpineAnimationComplete;
        }
    }
}
