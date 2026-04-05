using GameData.MapData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyDataUI : MonoBehaviour
{
    public InputField enemy_key_input;
    public InputField routeID_input;
    public InputField time_input;
    public InputField repeat_input;
    public InputField interval_input;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public ActionEntity GetEnemyData()
    {
        ActionEntity entity = new ActionEntity
        {
            ActionType = "SPAWN",
            Key = enemy_key_input.text,
            RouteIndex = int.Parse(routeID_input.text),
            PreDelay = float.Parse(time_input.text),
            Count = int.Parse(repeat_input.text),
            Interval = float.Parse(interval_input.text)
        };
        return entity;
    }
    public void SetUI(ActionEntity actionEntity)
    {
        //fill the input
        enemy_key_input.text = actionEntity.Key;
        routeID_input.text = actionEntity.RouteIndex.ToString();
        repeat_input.text = actionEntity.Count.ToString();
        interval_input.text = actionEntity.Interval.ToString();
        time_input.text = actionEntity.PreDelay.ToString();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
