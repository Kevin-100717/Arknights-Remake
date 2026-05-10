using GameData.MapData;
using System.Collections;
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
    private Enemy enemyController;
    public bool nextEnemyTask = false;
    private bool inTaskPerform = false;
    private Task currentTask;
    private Vector3 startPos;
    private FlyMove flyMove;
    private bool back_to_start = false;
    private void SpawnEnemy(string enemyKey,RouteEntity route)
    {
        GameObject enemy_prefab = Resources.Load<GameObject>($"Prefabs/Enemies/{enemyKey}");
        if (enemy_prefab == null)
        {
            Debug.LogError($"Enemy prefab not found for key: {enemyKey}");
            return;
        }
        RouteEntity routeData = route;
        Vector3 startPoint = new Vector3(routeData.StartPosition.Col, routeData.StartPosition.Row, -0.5f);
        GameObject enemy = Instantiate(enemy_prefab, startPoint, Quaternion.identity);
        Enemy enemyObj = enemy.GetComponent<Enemy>();
        enemyObj.enemyData.type = GameData.Game.EnemyData.EnemyType.Fly;
        enemyObj.transform.localEulerAngles = new Vector3(-30, 0, 0);
        enemyObj.route = routeData;
        enemyObj.route.Checkpoints.Add(new CheckpointEntity { Position = new PositionEntity { Row = 0, Col = 0 } ,Type="WAIT_FOR_SECONDS",Time=999});
    }
    // Start is called before the first frame update
    private void Awake()
    {
        
        if(instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }
    void Start()
    {
        enemyController = GetComponent<Enemy>();
        enemyController.isReWrite = true;
        startPos = transform.position;
        flyMove = GetComponent<FlyMove>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!inTaskPerform)
        {
            if (taskList.Count > 0)
            {
                currentTask = taskList[0];
                taskList.RemoveAt(0);
                inTaskPerform = true;
                if (currentTask.taskType == Task.TaskType.Place)
                {
                    back_to_start = false;
                    flyMove.speed = enemyController.enemyData.speed;
                    flyMove.nextWaypointDistance = currentTask.min_dist;
                    flyMove.SetTarget(startPos);
                    enemyController.state = GameData.Game.EnemyState.Idle;
                }
            }
        }
        else
        {
            if (!back_to_start)
            {
                Debug.Log("Back to start");
                BackToStart();
                return;
            }
            if (currentTask.taskType == Task.TaskType.Place)
            {
                Debug.Log("PerformPlace");
                if (flyMove.reachedEndOfPath)
                {
                    SpawnEnemy(currentTask.actionEntity.Key, EnemySpawner.Instance.battleData.Routes[(int)currentTask.actionEntity.RouteIndex]);
                    enemyController.state = GameData.Game.EnemyState.Idle;
                    inTaskPerform = false;
                }
                else
                {
                    flyMove.Move();
                }
            }
        }
    }
    void BackToStart()
    {
        flyMove.Move();
        if(flyMove.reachedEndOfPath)
        {
            enemyController.state = GameData.Game.EnemyState.Move;
            back_to_start = true;
            RouteEntity routeData = EnemySpawner.Instance.battleData.Routes[(int)currentTask.actionEntity.RouteIndex];
            Vector3 startPos = new Vector3(routeData.StartPosition.Col, routeData.StartPosition.Row, -0.5f);
            flyMove.SetTarget(startPos);
        }
    }
}
