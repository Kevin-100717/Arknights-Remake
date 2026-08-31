using DG.Tweening;
using GameData.Game;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

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
    public CharacterState state = CharacterState.Place;
    public CharacterAnimationData currentAnim;

    private float attackTimer;
    private List<GameObject> enemies = new List<GameObject>();
    private Dictionary<CharacterState, CharacterAnimationData> animationMap;
    public bool isDead = false;
    public enum CharDirection
    {
        Left,
        Right,
        UP,
        DOWN
    }
    [Header("Placing Data")]
    public CharDirection direction = CharDirection.Left;
    public Transform ui_component;
    public Transform collector_component;
    private int place_state = 0;
    public CharacterPlaceBtn btn;
    public LayerMask check_layer;
    public bool buildable = false;
    public Transform placeUI;
    public Transform dir_cursor;
    public LayerMask dir_layer;
    public Transform dir_center;
    public Transform darrow;
    private int angle = 0;
    public Transform hp_ui;

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
        Debug.Assert(skeletonAnimation != null, "SkeletonAnimation component is missing!", this);

        state = CharacterState.Place;
        GameController.instance.is_placing = true;
        buildUIController.instance.ShowBuildable(btn.cuid.buildType);
        charData.hp_current = charData.hp_total;

        if (attackController == null)
            attackController = GetComponentInChildren<CharAttackController>();

        Debug.Assert(attackController != null, "CharAttackController component is missing!", this);
        hpUIController.hpBar.gameObject.SetActive(false);
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
        else if (currentAnimationName == GetAnimationName(CharacterState.Start))
        {
            state = CharacterState.Idle;
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
        if (state != CharacterState.Start && state != CharacterState.Place)
        {
            UpdateDir();
            AttackController();
        }
        else
        {
            if (state == CharacterState.Place && place_state == 0)
            {
                PlaceController();
                if (buildable && Input.GetMouseButtonDown(1))
                {
                    buildUIController.instance.ClearAll();
                    placeUI.gameObject.SetActive(true);
                    place_state++;
                    buildUIController.instance.enable_show_range = true;

                }
            }
            else if (state == CharacterState.Place && place_state == 1)
            {
                DirControl();
                if (Input.GetMouseButtonDown(0))
                {
                    state = CharacterState.Start;
                    GameController.instance.is_placing = false;
                    buildUIController.instance.enable_show_range = false;
                    placeUI.gameObject.SetActive(false);
                }
            }
        }
    }
    void DirControl()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hit = Physics.RaycastAll(ray, 100, dir_layer);
        Vector3 p = hit[0].point;
        if (Vector3.Distance(new Vector3(p.x, p.y, dir_cursor.position.z), dir_center.transform.position) < 1.1f)
        {
            dir_cursor.transform.position = new Vector3(p.x, p.y, dir_cursor.position.z);
        }
        Vector3 d = new Vector3(p.x, p.y, dir_cursor.position.z);
        Vector3 o = dir_center.transform.position;
        if (d.y > o.y && Mathf.Abs(d.y - o.y) > 0.8f)
        {
            Debug.Log("1");
            direction = CharDirection.Right;
            angle = 90;
        }
        else if (d.y < o.y && Mathf.Abs(d.y - o.y) > 0.8f)
        {
            Debug.Log("2");
            direction = CharDirection.Right;
            angle = 270;
        }
        else if (d.x < o.x)
        {
            Debug.Log("3");
            direction = CharDirection.Right;
            angle = 0;
        }
        else if (d.x > o.x)
        {
            Debug.Log("4");
            direction = CharDirection.Left ;
            angle = 180;
        }
        darrow.transform.eulerAngles = new Vector3(0, 0, angle);
        collector_component.localEulerAngles = new Vector3(0, 0, angle);
    }
    void PlaceController()
    {
        if (place_state == 0)
        {
            Vector3 mp = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mp);
            RaycastHit[] hit = Physics.RaycastAll(ray, 100, check_layer);
            if (hit.Length > 0)
            {
                if (hit[0].collider.gameObject.CompareTag(btn.cuid.buildType == BuildType.Highland ? "highland" : "ground"))
                {
                    transform.position = new Vector3(hit[0].transform.position.x, hit[0].transform.position.y, btn.cuid.buildType == BuildType.Highland ? -0.3f : -0.01f);
                    buildable = true;
                }
                else
                {
                    buildable = false;
                }

            }
            else
            {
                Vector3 pos = Camera.main.WorldToScreenPoint(transform.position);
                Vector3 m_MousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, pos.z);
                Vector3 wp = Camera.main.ScreenToWorldPoint(m_MousePos);
                transform.position = new Vector3(wp.x, wp.y, btn.cuid.buildType == BuildType.Highland ? -0.3f : -0.01f);
                buildable = false;
            }
        }
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
        btn.gameObject.SetActive(true);
        btn.StartRespawn(charData.respawn_time);
    }
    void UpdateDir()
    {
        if (direction == CharDirection.Left)
        {
            transform.eulerAngles = new Vector3(-30, 0, 0);
            ui_component.localEulerAngles = new Vector3(60, 0, 0);
            collector_component.localEulerAngles = new Vector3(-30, 0, angle - 180);
            placeUI.localEulerAngles = new Vector3(-60, 0, 0);
            darrow.transform.eulerAngles = new Vector3(0, 0, angle-180);
            hp_ui.transform.eulerAngles = new Vector3(-60, 0, 0);
        }
        else
        {
            transform.eulerAngles = new Vector3(30, 180, 0);
            ui_component.localEulerAngles = new Vector3(0, 0, 0);
            collector_component.localEulerAngles = new Vector3(0, 0, angle);
            placeUI.localEulerAngles = new Vector3(0, 0, 0);
            darrow.transform.eulerAngles = new Vector3(0, 0, 180-angle);
            hp_ui.transform.eulerAngles = new Vector3(-30, 0, 0);

        }
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetFloat("_angle", direction == CharDirection.Right ? -30f : 60f);
        GetComponent<MeshRenderer>().SetPropertyBlock(propertyBlock);
    }
}