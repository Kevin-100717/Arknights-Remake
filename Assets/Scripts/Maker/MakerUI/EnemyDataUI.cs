using GameData.MapData;
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
        return new ActionEntity
        {
            ActionType = "SPAWN",
            Key = enemy_key_input.text,
            RouteIndex = int.TryParse(routeID_input.text, out int route) ? route : 0,
            PreDelay = float.TryParse(time_input.text, out float delay) ? Mathf.Max(0f, delay) : 0f,
            Count = int.TryParse(repeat_input.text, out int count) ? Mathf.Max(1, count) : 1,
            Interval = float.TryParse(interval_input.text, out float interval) ? Mathf.Max(0f, interval) : 0f
        };
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
}
