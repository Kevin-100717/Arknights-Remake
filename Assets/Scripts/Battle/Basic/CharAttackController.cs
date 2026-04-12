using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharAttackController : MonoBehaviour
{
    // Start is called before the first frame update
    public List<EnemyCollector> enemyCollectors;
    void Start()
    {
        
    }
    public List<GameObject> GetEnemies()
    {
        List<GameObject> enemies = new List<GameObject>();
        foreach (EnemyCollector collector in enemyCollectors)
        {
            foreach (GameObject enemy in collector.collectList)
            {
                if (!enemies.Contains(enemy))
                {
                    enemies.Add(enemy);
                }
            }
        }
        return enemies;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
