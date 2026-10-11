using System.Collections.Generic;
using UnityEngine;

public class CharAttackController : MonoBehaviour
{
    public List<EnemyCollector> enemyCollectors;

    public List<GameObject> GetEnemies()
    {
        List<GameObject> enemies = new List<GameObject>();
        foreach (EnemyCollector collector in enemyCollectors)
        {
            if (collector == null) continue;
            foreach (GameObject enemy in collector.collectList)
            {
                if (enemy != null && !enemies.Contains(enemy))
                    enemies.Add(enemy);
            }
        }
        return enemies;
    }
}
