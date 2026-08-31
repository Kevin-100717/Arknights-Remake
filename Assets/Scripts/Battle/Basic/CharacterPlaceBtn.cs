using GameData.Game;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class CharacterPlaceBtn : MonoBehaviour
{

    // Use this for initialization
    public CharacterUIData cuid;
    public GameObject respawnFrame;
    public Image respawnCircle;
    public Text respawnTime;
    private bool isRespawning = false;
    private float rspawnTime;
    private float rspawnTimeTotal;
    void Start()
    {

    }
    public void OnClick()
    {
        if(GameController.instance.cost >= cuid.cost && !GameController.instance.is_placing && !isRespawning)
        {
            GameController.instance.cost -= cuid.cost;
            gameObject.SetActive(false);
            GameObject character = Instantiate(cuid.characterPrefab);
            character.GetComponent<Character>().btn = this;
        }
    }
    public void StartRespawn(int time)
    {
        isRespawning = true;
        rspawnTime = time;
        rspawnTimeTotal = time;
        respawnFrame.SetActive(true);
    }
    // Update is called once per frame
    void Update()
    {
        if (isRespawning)
        {
            rspawnTime -= Time.deltaTime;
            respawnTime.text = rspawnTime.ToString("F1");
            respawnCircle.fillAmount = rspawnTime/rspawnTimeTotal;
            if(rspawnTime <= 0)
            {
                isRespawning = false;
                respawnFrame.SetActive(false);
                gameObject.SetActive(true);
            }
        }
    }
}