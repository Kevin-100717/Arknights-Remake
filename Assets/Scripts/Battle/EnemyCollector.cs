using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void UpdateCollectList(bool delete, GameObject charObj)
    {
        if (delete && collectList.Contains(charObj))
        {
            collectList.Remove(charObj);
        }
        else if (!delete && !collectList.Contains(charObj))
        {
            collectList.Add(charObj);
        }
    }
    // Update is called once per frame
    void Update()
    {
        List<GameObject> tempList = new List<GameObject>();
        foreach (GameObject obj in collectList)
        {
            if (obj && obj.GetComponent<Enemy>() != null && obj.GetComponent<Enemy>().state == GameData.Game.EnemyState.Die)
            {
                tempList.Add(obj);
            }
        }
        foreach (GameObject obj in tempList)
        {
            collectList.Remove(obj);
        }
    }
    private void OnTriggerEnter(Collider collision)
    {
        //Debug.Log(collision.gameObject.name);
        if (collectType == CollectType.Character && collision.gameObject.tag == "Character")
        {
            UpdateCollectList(false, collision.gameObject);
        }
        else if (collectType == CollectType.Enemy && collision.gameObject.tag == "Enemy")
        {
            UpdateCollectList(false, collision.gameObject);
        }
    }
    private void OnTriggerExit(Collider collision)
    {
        if (collectType == CollectType.Character && collision.gameObject.tag == "Character")
        {
            UpdateCollectList(true, collision.gameObject);
        }
        else if (collectType == CollectType.Enemy && collision.gameObject.tag == "Enemy")
        {
            UpdateCollectList(true, collision.gameObject);
        }
    }
}
