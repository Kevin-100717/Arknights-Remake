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
    // Update is called once per frame
    void Update()
    {
        UpdateGameBar();
        Time.timeScale = speed;
    }
}
