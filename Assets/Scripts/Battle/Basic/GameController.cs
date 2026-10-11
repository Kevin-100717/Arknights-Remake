using GameData.MapData;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    [Header("UI")]
    public Text enemyCountText;
    public Text lifeText;
    public List<Sprite> speedImage;
    public Image speedBtn;
    public Image costImg;
    public Text costText;
    public GameObject pauseFrame;
    public Image pauseImage;
    public Sprite pauseSpr;
    public Sprite runSpr;

    [Header("Battle State")]
    public int enemyCount;
    public int allEnemyCount;
    public int life = 3;
    public int cost = 20;
    public float cost_speed = 0.8f;   // 每点费用的回复时间（秒）
    public float moveMultiplier = 0.5f;
    public int characterLimit = 0;    // 0 表示不限制
    public bool is_placing = false;
    public float battleTime;          // 战斗经过时间（暂停/部署时随 timeScale 变慢）

    private int maxLife = 3;
    private int maxCost = 99;
    private float cost_timer = 0;
    private float speed = 1;
    private bool pause = false;
    private float timeTemp;

    void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// 应用关卡 options 中的战斗参数。
    /// </summary>
    public void ApplyOptions(OptionsEntity options)
    {
        if (options == null) return;

        if (options.MaxLifePoint > 0)
        {
            maxLife = Mathf.RoundToInt(options.MaxLifePoint);
            life = maxLife;
        }
        if (options.InitialCost > 0)
            cost = Mathf.RoundToInt(options.InitialCost);
        // 官方默认最大费用上限为 99
        maxCost = options.MaxCost > 0 ? Mathf.RoundToInt(options.MaxCost) : 99;
        if (options.CostIncreaseTime > 0)
            cost_speed = options.CostIncreaseTime;
        if (options.MoveMultiplier > 0)
            moveMultiplier = (float)options.MoveMultiplier;
        characterLimit = Mathf.RoundToInt(options.CharacterLimit);

        UpdateGameBar();
    }

    public void UpdateGameBar()
    {
        enemyCountText.text = enemyCount + "/" + allEnemyCount;
        lifeText.text = life.ToString();
    }

    public void UpdateGameSpeed()
    {
        if (is_placing) return;

        speed++;
        if (speed > 3)
            speed = 1;
        speedBtn.sprite = speedImage[(int)speed - 1];
    }

    /// <summary>敌人漏过终点时扣除生命。</summary>
    public void ReduceLife(int amount = 1)
    {
        life = Mathf.Max(0, life - amount);
        UpdateGameBar();
    }

    void UpdateCost()
    {
        // 官方：到达最大费用上限后停止回复并显示 MAX，溢出的自然回复丢弃
        costText.text = cost >= maxCost ? "MAX" : cost.ToString();

        if (cost >= maxCost)
        {
            cost_timer = cost_speed;
            costImg.transform.localScale = Vector3.one;
            return;
        }

        cost_timer += Time.deltaTime;
        while (cost_timer >= cost_speed && cost < maxCost)
        {
            cost_timer -= cost_speed;
            cost++;
        }
        costImg.transform.localScale = new Vector3(Mathf.Clamp01(cost_timer / cost_speed), 1, 1);
    }

    public void PauseHandler()
    {
        pause = !pause;
        if (pause)
        {
            pauseFrame.SetActive(true);
            pauseImage.sprite = pauseSpr;
            timeTemp = speed;
            speed = 0;
        }
        else
        {
            pauseFrame.SetActive(false);
            pauseImage.sprite = runSpr;
            speed = timeTemp;
        }
    }

    void Update()
    {
        UpdateGameBar();
        UpdateCost();
        battleTime += Time.deltaTime;

        if (!is_placing)
            Time.timeScale = speed;
        else
            Time.timeScale = 0.1f;
    }
}
