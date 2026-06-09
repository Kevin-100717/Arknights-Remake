using GameData.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buildUIController : MonoBehaviour
{
    // Start is called before the first frame update
    public static buildUIController instance;
    public List<BuildShowUI> bsui; 
    void Start()
    {
        instance = this;
    }
    public void LoadUI()
    {
        foreach(Transform child in gameObject.transform)
        {
            bsui.Add(child.GetComponent<BuildShowUI>());
        }
    }
    public void ClearAll()
    {
        foreach(BuildShowUI ui in bsui)
        {
            ui.show_range_ui.SetActive(false);
            ui.buildable_ui.SetActive(false);
        }
    }
    public void ShowBuildable(BuildType bt)
    {
        ClearAll();
        string tag = bt == BuildType.Highland ? "highland" : "ground";
        foreach(BuildShowUI bui in bsui)
        {
            if(bui.tag == tag)
            {
                bui.buildable_ui.SetActive(true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
