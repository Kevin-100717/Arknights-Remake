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

    private int currentPathIndex = -1;
    private float waitTime;
    private SpineAnimationData currentAnim;
    private bool hasReachedEnd = false;
    private bool onAttack = false;
    private bool isDead = false;
    private Dictionary<EnemyState, SpineAnimationData> animationMap;
    private EnemyState stateBeforeAttack = EnemyState.Idle; // 记录攻击前的状态

    void Awake()
    {
        InitializeAnimationMap();
    }

    void Start()
    {
        ValidateAndInitialize();
        BindSpineAnimationEvents();
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
            if (atk_target != null)
            {
                atk_target.charData.current_def_num = Mathf.Max(0, atk_target.charData.current_def_num - 1);
            }
        }
    }

    void OnSpineAnimationComplete(Spine.TrackEntry trackEntry)
    {
        string currentAnimationName = trackEntry.Animation.Name;

        if (currentAnimationName == GetAnimationName(EnemyState.Attack))
        {
            waitTime = enemyData.atk_interval;
            state = EnemyState.Attack_interval; // 改为Attack_interval状态
        }
        else if (currentAnimationName == GetAnimationName(EnemyState.Die))
        {
            isDead = true;
            Destroy(gameObject);
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
                state = EnemyState.Idle;
                waitTime = checkpoint.Time;
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
        ProcessState();
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

            case EnemyState.Attack_interval: // 处理Attack_interval状态
                ProcessAttackInterval();
                break;

            case EnemyState.Attack:
                ProcessAttack();
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
        if (waitTime > 0)
        {
            waitTime -= Time.deltaTime;
        }
        else
        {
            UpdateCheckPoint();
        }
    }

    void ProcessAttackInterval()
    {
        // Attack_interval倒计时
        waitTime -= Time.deltaTime;
        if (waitTime <= 0)
        {
            // 检查目标是否还存在且存活
            if (atk_target != null && atk_target.state != CharacterState.Die)
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
                character.charData.current_def_num < character.charData.def_num &&
                character.state != CharacterState.Die)
            {
                // 记录攻击前的状态
                stateBeforeAttack = state;
                character.charData.current_def_num++;
                atk_target = character;
                state = EnemyState.Attack;
                onAttack = true;
                waitTime = 0;
            }
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
                atk_target = null;
                // 恢复到攻击前的状态
                state = stateBeforeAttack;
                onAttack = false;
                waitTime = 0;
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