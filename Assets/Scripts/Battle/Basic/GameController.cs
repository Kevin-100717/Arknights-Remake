using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public int enemyCount;
    public int allEnemyCount;
    public int life = 3;
    public Text enemyCountText;
    public Text lifeText;
    public List<Sprite> speedImage;
    public float speed = 1;
    public Image speedBtn;
    public Image costImg;
    public Text costText;
    public int cost = 20;
    public float cost_speed = 0.8f;
    public float cost_timer = 0;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }
    public void UpdateGameBar()
    {
        enemyCountText.text = enemyCount.ToString() + "/" + allEnemyCount.ToString();
        lifeText.text = life.ToString();
    }
    public void UpdateGameSpeed()
    {
        speed++;
        if(speed > 3)
        {
            speed = 1;
        }
        speedBtn.sprite = speedImage[(int)speed - 1];
    }
    void UpdateCost()
    {
        cost_timer += Time.deltaTime;
        costText.text = cost.ToString();
        costImg.transform.localScale = new Vector3(cost_timer / cost_speed, 1, 1);
        if (cost_timer >= cost_speed)
        {
            cost_timer = 0;
            cost++;
        }
    }
    // Update is called once per frame
    void Update()
    {
        UpdateGameBar();
        UpdateCost();
        Time.timeScale = speed;
    }
}
