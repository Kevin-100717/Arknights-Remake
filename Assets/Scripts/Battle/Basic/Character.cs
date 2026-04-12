using GameData.Game;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Character : MonoBehaviour
{
    [Header("Data References")]
    public CharacterData charData;
    public List<CharacterAnimationData> characterAnimationDatas;

    [Header("Components")]
    public SkeletonAnimation skeletonAnimation;
    public CharAttackController attackController;
    public HpUIController hpUIController;

    [Header("Runtime Data")]
    public CharacterState state;
    public CharacterAnimationData currentAnim;

    private float attackTimer;
    private List<GameObject> enemies = new List<GameObject>();
    private Dictionary<CharacterState, CharacterAnimationData> animationMap;
    public bool isDead = false;

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
        animationMap = new Dictionary<CharacterState, CharacterAnimationData>();
        foreach (var data in characterAnimationDatas)
        {
            if (!animationMap.ContainsKey(data.state))
                animationMap[data.state] = data;
        }
    }

    void ValidateAndInitialize()
    {
        Debug.Assert(charData != null, "CharacterData is missing!", this);
        Debug.Assert(charData.hp_total > 0, "HP Total must be greater than zero!", this);
        Debug.Assert(skeletonAnimation != null, "SkeletonAnimation component is missing!", this);

        state = CharacterState.Idle;
        charData.hp_current = charData.hp_total;

        if (attackController == null)
            attackController = GetComponentInChildren<CharAttackController>();

        Debug.Assert(attackController != null, "CharAttackController component is missing!", this);
    }

    void BindSpineAnimationEvents()
    {
        skeletonAnimation.state.Complete -= OnSpineAnimationComplete;
        skeletonAnimation.state.Complete += OnSpineAnimationComplete;
    }

    void OnSpineAnimationComplete(Spine.TrackEntry trackEntry)
    {
        string currentAnimationName = trackEntry.Animation.Name;

        if (currentAnimationName == GetAnimationName(CharacterState.Attack))
        {
            attackTimer = charData.atk_interval;
            state = CharacterState.Attack_interval;
        }
        else if (currentAnimationName == GetAnimationName(CharacterState.Die))
        {
            isDead = true;
            Destroy(gameObject);
        }
    }

    string GetAnimationName(CharacterState state)
    {
        if (animationMap != null && animationMap.TryGetValue(state, out var data))
            return data.animationName;
        return "";
    }

    void UpdateAnimation()
    {
        if (currentAnim == null || state != currentAnim.state)
        {
            if (animationMap.TryGetValue(state, out currentAnim))
            {
                skeletonAnimation.state.SetAnimation(0, currentAnim.animationName, currentAnim.loop);
            }
        }
    }

    void Update()
    {
        if (isDead) return;

        UpdateAnimation();
        AttackController();
    }

    void AttackController()
    {
        // Attack_interval状态下倒计时
        if (state == CharacterState.Attack_interval)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                state = CharacterState.Idle;
            }
        }

        // 清理死亡的敌人
        CleanDeadEnemies();

        enemies = attackController.GetEnemies();

        // 只有在Idle状态下才能开始攻击
        if (enemies.Count <= 0 || state != CharacterState.Idle)
            return;

        // 检查是否有存活的敌人
        bool hasLivingEnemies = false;
        foreach (GameObject enemyObj in enemies)
        {
            if (enemyObj != null)
            {
                Enemy enemy = enemyObj.GetComponent<Enemy>();
                if (enemy != null && enemy.state != EnemyState.Die && !enemy.isDead)
                {
                    hasLivingEnemies = true;
                    break;
                }
            }
        }

        if (hasLivingEnemies)
        {
            attackTimer = charData.atk_interval;
            state = CharacterState.Attack;
        }
    }

    void CleanDeadEnemies()
    {
        enemies.RemoveAll(obj => {
            if (obj == null) return true;
            Enemy enemy = obj.GetComponent<Enemy>();
            return enemy == null || enemy.state == EnemyState.Die || enemy.isDead;
        });
    }

    public void HandleAttack()
    {
        if (isDead) return;

        CleanDeadEnemies();

        int attackCount = Mathf.Min(enemies.Count, charData.max_atk_num);
        for (int i = 0; i < attackCount; i++)
        {
            GameObject enemyObj = enemies[i];
            if (enemyObj != null)
            {
                Enemy enemy = enemyObj.GetComponent<Enemy>();
                if (enemy != null && enemy.state != EnemyState.Die && !enemy.isDead)
                {
                    enemy.TakeDamage(charData.damage);
                }
            }
        }
    }

    public void TakeDamage(Damage damage)
    {
        if (isDead || state == CharacterState.Die) return;

        switch (damage.type)
        {
            case DamageType.Physical:
                charData.hp_current -= Mathf.Max(damage.damage - charData.def, 0.05f * damage.damage);
                break;
            case DamageType.Magic:
                charData.hp_current -= Mathf.Max(damage.damage * (1 - 0.01f * charData.magic_def), 0.05f * damage.damage);
                break;
        }

        charData.hp_current = Mathf.Max(0, charData.hp_current);

        if (charData.hp_current <= 0)
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
        charData.hp_current = 0;
        state = CharacterState.Die;

        if (hpUIController != null)
            hpUIController.hpBar.gameObject.SetActive(false);

        PlayDeathAnimation();
    }

    void PlayDeathAnimation()
    {
        string deathAnimName = GetAnimationName(CharacterState.Die);
        if (!string.IsNullOrEmpty(deathAnimName))
        {
            skeletonAnimation.state.SetAnimation(0, deathAnimName, false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void UpdateHealthBar()
    {
        if (hpUIController == null) return;

        hpUIController.hpBar.gameObject.SetActive(true);
        float ratio = charData.hp_current / charData.hp_total;
        hpUIController.white.DOScaleX(ratio, 0.3f);
        hpUIController.red.DOScaleX(ratio, 0.1f);
    }

    void OnDestroy()
    {
        if (skeletonAnimation != null && skeletonAnimation.state != null)
        {
            skeletonAnimation.state.Complete -= OnSpineAnimationComplete;
        }
    }
}
