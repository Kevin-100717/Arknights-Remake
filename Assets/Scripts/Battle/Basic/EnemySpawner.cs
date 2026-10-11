using GameData.MapData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    /// <summary>当前 wave / fragment 的开始时刻（battleTime），供路点绝对时间等待使用。</summary>
    public static float CurrentWaveStartTime { get; private set; }
    public static float CurrentFragmentStartTime { get; private set; }

    /// <summary>当前波次序号（0 起），供 WAIT_BOSSRUSH_WAVE 等按波次数等待的路点使用。</summary>
    public static int CurrentWaveIndex { get; private set; }

    /// <summary>场上存活敌人数量（波次清怪等待依据）。</summary>
    public static int AliveEnemyCount { get; private set; }

    public static void NotifyEnemyAdded()
    {
        AliveEnemyCount++;
    }

    public static void NotifyEnemyRemoved()
    {
        AliveEnemyCount = Mathf.Max(0, AliveEnemyCount - 1);
    }

    [Header("Debug")]
    [SerializeField] private bool enableLog = false;

    /// <summary>该 key 的出怪动作不直接生成敌人，而是让 MpprHand 接手运送下一只敌人。</summary>
    private const string MpprHandTaskKey = "enemy_10072_mpprhd";

    public BattleData battleData;
    private int actionFin;
    private bool countInitialized;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void LoadEnemyData(BattleData bd)
    {
        battleData = bd;
        AliveEnemyCount = 0;
        CurrentWaveIndex = 0;
        if (battleData.Waves == null) battleData.Waves = new List<WaveEntity>();
        if (battleData.Routes == null) battleData.Routes = new List<RouteEntity>();
        if (MapCoord.Height < 0 && bd?.MapData?.Map != null)
            MapCoord.Height = bd.MapData.Map.Count;
        StartCoroutine(SpawnEnemies());
    }

    private int GetEnemyCount()
    {
        int count = 0;
        foreach (var wave in battleData.Waves)
        foreach (var fragment in wave.Fragments)
        foreach (var action in fragment.Actions)
            if (action.ActionType == "SPAWN")
                count += (int)action.Count;
        return count;
    }

    private IEnumerator SpawnEnemies()
    {
        int waveIndex = 0;
        foreach (var wave in battleData.Waves)
        {
            // 官方机制：下一波要等上一波敌人全部被处理（击杀或漏掉），
            // maxTimeWaitingForNextWave 为该等待的上限，-1 表示无限等待
            yield return StartCoroutine(WaitForPreviousWaveCleared(wave));
            yield return new WaitForSeconds(wave.PreDelay);

            CurrentWaveIndex = waveIndex++;
            CurrentWaveStartTime = BattleTime();

            foreach (var fragment in wave.Fragments)
            {
                yield return new WaitForSeconds(fragment.PreDelay);
                CurrentFragmentStartTime = BattleTime();

                actionFin = 0;
                foreach (var action in fragment.Actions)
                {
                    // 该动作只负责下达“由 MpprHand 运送”的指令，本身不生成敌人
                    if (action.Key == MpprHandTaskKey && MpprHand.instance != null)
                    {
                        MpprHand.instance.nextEnemyTask = true;
                        actionFin++;
                        continue;
                    }
                    StartCoroutine(HandleAction(action));
                }

                // 等待本 fragment 全部 action 完成（所有路径都会递增 actionFin）
                while (actionFin < fragment.Actions.Count)
                    yield return null;
            }
            yield return new WaitForSeconds(wave.PostDelay);
        }
    }

    private IEnumerator WaitForPreviousWaveCleared(WaveEntity wave)
    {
        // 场上无残留敌人时直接开波
        if (AliveEnemyCount <= 0) yield break;

        float maxWait = wave.MaxTimeWaitingForNextWave;
        if (maxWait < 0)
        {
            while (AliveEnemyCount > 0)
                yield return null;
        }
        else
        {
            float timer = 0f;
            while (AliveEnemyCount > 0 && timer < maxWait)
            {
                timer += Time.deltaTime;
                yield return null;
            }
        }
    }

    private IEnumerator HandleAction(GameData.MapData.ActionEntity action)
    {
        yield return new WaitForSeconds(action.PreDelay);

        if (MpprHand.instance != null && MpprHand.instance.nextEnemyTask)
        {
            MpprHand.instance.nextEnemyTask = false;
            MpprHand.instance.taskList.Add(new MpprHand.Task
            {
                taskType = MpprHand.Task.TaskType.Place,
                actionEntity = action,
                dist = Vector3.zero
            });
        }
        else
        {
            yield return StartCoroutine(SpawnEnemyAction(action.ActionType,
                (int)action.Count,
                action.Key,
                (int)action.RouteIndex,
                action.Interval,
                action.IsUnharmfulAndAlwaysCountAsKilled));
        }
        actionFin++;
    }

    /// <summary>
    /// 出生点 = startPosition + spawnOffset + 随机(±spawnRandomRange)，全部按小数计算。
    /// </summary>
    public static Vector3 SpawnPoint(RouteEntity route)
    {
        PositionEntity start = route.StartPosition;
        float offX = (float)(route.SpawnOffset?.X ?? 0);
        float offY = (float)(route.SpawnOffset?.Y ?? 0);
        float rngX = (float)(route.SpawnRandomRange?.X ?? 0);
        float rngY = (float)(route.SpawnRandomRange?.Y ?? 0);
        float x = start.Col + offX + (rngX > 0f ? Random.Range(-rngX, rngX) : 0f);
        float y = start.Row + offY + (rngY > 0f ? Random.Range(-rngY, rngY) : 0f);
        return new Vector3(x, y, 0);
    }

    private IEnumerator SpawnEnemyAction(string at, int count, string enemyKey, int routeIndex, float interval,
        bool unharmfulCountAsKilled = false)
    {
        if (enableLog)
            Debug.Log($"[EnemySpawner] Do Action '{enemyKey}' on Route {routeIndex} - action {at}");

        if (at != "SPAWN")
            yield break;

        if (routeIndex < 0 || routeIndex >= battleData.Routes.Count)
        {
            Debug.LogError($"[EnemySpawner] RouteIndex 越界: {routeIndex} ({enemyKey})");
            yield break;
        }

        GameObject enemyPrefab = Resources.Load<GameObject>($"Prefabs/Enemies/{enemyKey}");
        if (enemyPrefab == null)
        {
            Debug.LogError($"[EnemySpawner] 未找到敌人预制体: {enemyKey}");
            yield break;
        }

        for (int c = 0; c < count; c++)
        {
            // 每个敌人持有自己的路线副本，避免运行时修改污染共享数据
            RouteEntity routeData = battleData.Routes[routeIndex].Clone();
            Vector3 startPoint = SpawnPoint(routeData);

            GameObject enemy = Instantiate(enemyPrefab, startPoint, Quaternion.identity);
            Enemy enemyObj = enemy.GetComponent<Enemy>();
            enemyObj.transform.localEulerAngles = new Vector3(-30, 0, 0);
            enemyObj.route = routeData;
            enemyObj.SetUnharmfulCountAsKilled(unharmfulCountAsKilled);

            // MpprHand 本体是重写单位，不参与波次清怪计数
            if (enemy.GetComponent<MpprHand>() == null)
                enemyObj.NotifyAlive();

            yield return new WaitForSeconds(interval);
        }
    }

    private static float BattleTime()
    {
        return GameController.instance != null ? GameController.instance.battleTime : 0f;
    }

    private void Update()
    {
        if (!countInitialized && GameController.instance != null)
        {
            countInitialized = true;
            GameController.instance.allEnemyCount = GetEnemyCount();
        }
    }
}
