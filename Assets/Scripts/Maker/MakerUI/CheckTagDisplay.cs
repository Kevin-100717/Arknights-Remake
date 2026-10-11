using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckTagDisplay : MonoBehaviour
{
    public List<GameObject> displays;
    public Text waitTimeText;
    public void SetUI(int index)
    {
        int i = 0;
        foreach(GameObject display in displays)
        {
            display.SetActive(i == index);
            i++;
        }
    }
    public void SetWaitText(float waitTime)
    {
        waitTimeText.text = waitTime.ToString() + "S";
    }
}