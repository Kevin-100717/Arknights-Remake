using GameData.Game;
using System.Collections;
using UnityEngine;
public class CharacterPlaceBtn : MonoBehaviour
{

    // Use this for initialization
    public CharacterUIData cuid;
    void Start()
    {

    }
    public void OnClick()
    {
        if(GameController.instance.cost >= cuid.cost && !GameController.instance.is_placing)
        {
            GameController.instance.cost -= cuid.cost;
            gameObject.SetActive(false);
            GameObject character = Instantiate(cuid.characterPrefab);
            character.GetComponent<Character>().btn = this;
        }
    }
    // Update is called once per frame
    void Update()
    {

    }
}