using GameData.Game;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPlaceBtn : MonoBehaviour
{
    public CharacterUIData cuid;
    public GameObject respawnFrame;
    public Image respawnCircle;
    public Text respawnTime;
    private bool isRespawning = false;
    private float rspawnTime;
    private float rspawnTimeTotal;

    public void OnClick()
    {
        GameController gc = GameController.instance;
        if (gc == null || gc.is_placing || isRespawning) return;
        if (gc.cost < cuid.cost) return;
        if (gc.characterLimit > 0 && FindObjectsOfType<Character>().Length >= gc.characterLimit) return;

        gc.cost -= cuid.cost;
        gameObject.SetActive(false);
        GameObject character = Instantiate(cuid.characterPrefab);
        character.GetComponent<Character>().btn = this;
    }
    public void StartRespawn(int time)
    {
        isRespawning = true;
        rspawnTime = time;
        rspawnTimeTotal = time;
        respawnFrame.SetActive(true);
    }

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