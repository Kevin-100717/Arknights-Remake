using GameData.MapData;
using System.Collections.Generic;
using UnityEngine;

public class MpprHand : MonoBehaviour
{
    public static MpprHand instance;

    [System.Serializable]
    public class Task
    {
        public enum TaskType
        {
            Place,
            Active
        }
        public TaskType taskType;
        public ActionEntity actionEntity;
        public Vector3 dist;
        [HideInInspector]
        public float min_dist = 0.05f;
    }

    public List<Task> taskList = new List<Task>();
    public bool nextEnemyTask = false;

    private Enemy enemyController;
    private bool inTaskPerform;
    private Task currentTask;
    private Vector3 startPos;
    private FlyMove flyMove;
    private bool back_to_start;

    private void SpawnEnemy(string enemyKey, RouteEntity route, bool unharmfulCountAsKilled)
    {
        GameObject enemyPrefab = Resources.Load<GameObject>($"Prefabs/Enemies/{enemyKey}");
        if (enemyPrefab == null)
        {
            Debug.LogError($"Enemy prefab not found for key: {enemyKey}");
            return;
        }

        // 使用路线副本，避免向共享数据追加等待节点
        RouteEntity routeData = route.Clone();
        routeData.Checkpoints.Add(new CheckpointEntity
        {
            Position = new PositionEntity { Row = 0, Col = 0 },
            Type = "WAIT_FOR_SECONDS",
            Time = 999
        });

        // 出生点同样应用路线级 spawnOffset / spawnRandomRange
        Vector3 startPoint = EnemySpawner.SpawnPoint(routeData);
        startPoint.z = -0.5f;
        GameObject enemy = Instantiate(enemyPrefab, startPoint, Quaternion.identity);
        Enemy enemyObj = enemy.GetComponent<Enemy>();
        enemyObj.enemyData.type = GameData.Game.EnemyData.EnemyType.Fly;
        enemyObj.transform.localEulerAngles = new Vector3(-30, 0, 0);
        enemyObj.route = routeData;
        enemyObj.SetUnharmfulCountAsKilled(unharmfulCountAsKilled);
        enemyObj.NotifyAlive();
    }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void Start()
    {
        enemyController = GetComponent<Enemy>();
        enemyController.isReWrite = true;
        startPos = transform.position;
        flyMove = GetComponent<FlyMove>();
    }

    void Update()
    {
        if (!inTaskPerform)
        {
            if (taskList.Count == 0) return;

            currentTask = taskList[0];
            taskList.RemoveAt(0);
            inTaskPerform = true;
            if (currentTask.taskType == Task.TaskType.Place)
            {
                back_to_start = false;
                float moveMultiplier = GameController.instance != null ? GameController.instance.moveMultiplier : 0.5f;
                flyMove.speed = enemyController.enemyData.speed * moveMultiplier;
                flyMove.nextWaypointDistance = currentTask.min_dist;
                flyMove.SetTarget(startPos);
                enemyController.state = GameData.Game.EnemyState.Idle;
            }
            return;
        }

        if (!back_to_start)
        {
            BackToStart();
            return;
        }

        if (currentTask.taskType == Task.TaskType.Place)
        {
            if (flyMove.reachedEndOfPath)
            {
                SpawnEnemy(currentTask.actionEntity.Key,
                    EnemySpawner.Instance.battleData.Routes[(int)currentTask.actionEntity.RouteIndex],
                    currentTask.actionEntity.IsUnharmfulAndAlwaysCountAsKilled);
                enemyController.state = GameData.Game.EnemyState.Idle;
                inTaskPerform = false;
            }
            else
            {
                flyMove.Move();
            }
        }
    }

    void BackToStart()
    {
        flyMove.Move();
        if (!flyMove.reachedEndOfPath) return;

        back_to_start = true;
        RouteEntity routeData = EnemySpawner.Instance.battleData.Routes[(int)currentTask.actionEntity.RouteIndex];
        Vector3 routeStart = new Vector3(
            routeData.StartPosition.Col,
            routeData.StartPosition.Row,
            -0.5f);
        flyMove.SetTarget(routeStart);
        enemyController.state = GameData.Game.EnemyState.Move;
    }
}
