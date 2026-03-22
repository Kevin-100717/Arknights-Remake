using GameData.MapData;
using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [Header("Debug")]
    [SerializeField] private bool enableLog = false;

    private BattleData battleData;
    private int actionFin = 0;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void LoadEnemyData(BattleData bd)
    {
        battleData = bd;
        GameController.instance.allEnemyCount = GetEnemyCount();
        StartCoroutine(SpawnEnemies());
    }
    private int GetEnemyCount()
    {
        int count = 0;
        foreach (var wave in battleData.Waves)
        {
            foreach (var fragment in wave.Fragments)
            {
                foreach (var action in fragment.Actions)
                {
                    if (action.ActionType == "SPAWN")
                    {
                        count += (int)action.Count;
                    }
                }
            }
        }
        return count;
    }
    private IEnumerator SpawnEnemies()
    {
        foreach (var wave in battleData.Waves)
        {
            yield return new WaitForSeconds(wave.PreDelay);

            foreach (var fragment in wave.Fragments)
            {
                yield return new WaitForSeconds(fragment.PreDelay);
                actionFin = 0;
                foreach (var action in fragment.Actions)
                {
                    StartCoroutine(HandleAction(action));
                }
                while (actionFin < fragment.Actions.Count)
                {
                    yield return null;
                }
            }
            yield return new WaitForSeconds(wave.PostDelay);
        }

        Debug.Log("All enemies spawned.");
    }
    private IEnumerator HandleAction(GameData.MapData.ActionEntity action)
    {
        yield return new WaitForSeconds(action.PreDelay);
        StartCoroutine(SpawnEnemyAction(action.ActionType,
            (int)action.Count,
            action.Key,
            (int)action.RouteIndex,
            action.Interval));
    }
    private IEnumerator SpawnEnemyAction(string at, int count, string enemyKey, int routeIndex, float interval)
    {
        if (enableLog)
        {
            Debug.Log($"[EnemySpawner] Do Action '{enemyKey}' on Route {routeIndex} - action {at}");
        }
        switch (at)
        {
            case "SPAWN":
                for (int c = 0; c < count; c++)
                {
                    GameObject enemy_prefab = Resources.Load<GameObject>($"Prefabs/Enemies/{enemyKey}");
                    if(enemy_prefab == null)
                    {
                        Debug.LogError($"Enemy prefab not found for key: {enemyKey}");
                        actionFin++;
                        yield break;
                    }
                    RouteEntity routeData = battleData.Routes[routeIndex];
                    Vector3 startPoint = new Vector3(routeData.StartPosition.Col,routeData.StartPosition.Row,0);
                    GameObject enemy = Instantiate(enemy_prefab,startPoint,Quaternion.identity);
                    Enemy enemyObj = enemy.GetComponent<Enemy>();
                    enemyObj.route = routeData;
                    yield return new WaitForSeconds(interval);
                }
                break;
        }
        actionFin++;
    }
}
