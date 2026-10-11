using System.Collections.Generic;
using UnityEngine;

public class EnemyCollector : MonoBehaviour
{
    public enum CollectType
    {
        Enemy,
        Character
    }

    public CollectType collectType;
    public List<GameObject> collectList;

    private void UpdateCollectList(bool remove, GameObject obj)
    {
        if (remove)
            collectList.Remove(obj);
        else if (!collectList.Contains(obj))
            collectList.Add(obj);
    }

    void Update()
    {
        // 清理已死亡或已销毁的目标
        collectList.RemoveAll(obj =>
        {
            if (obj == null) return true;
            Enemy enemy = obj.GetComponent<Enemy>();
            if (enemy != null)
                return enemy.isDead || enemy.state == GameData.Game.EnemyState.Die;
            Character character = obj.GetComponent<Character>();
            if (character != null)
                return character.isDead || character.state == GameData.Game.CharacterState.Die;
            return false;
        });
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collectType == CollectType.Character && collision.CompareTag("Character"))
            UpdateCollectList(false, collision.gameObject);
        else if (collectType == CollectType.Enemy && collision.CompareTag("Enemy"))
            UpdateCollectList(false, collision.gameObject);
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collectType == CollectType.Character && collision.CompareTag("Character"))
            UpdateCollectList(true, collision.gameObject);
        else if (collectType == CollectType.Enemy && collision.CompareTag("Enemy"))
            UpdateCollectList(true, collision.gameObject);
    }
}
