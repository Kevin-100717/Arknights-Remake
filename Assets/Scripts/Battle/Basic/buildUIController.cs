using GameData.Game;
using System.Collections.Generic;
using UnityEngine;

public class buildUIController : MonoBehaviour
{
    public static buildUIController instance;
    public List<BuildShowUI> bsui;
    public bool enable_show_range = false;

    void Awake()
    {
        instance = this;
    }

    public void LoadUI()
    {
        // 重新加载地图时先清空，避免列表重复累加与残留已销毁对象
        bsui.Clear();
        foreach (Transform child in transform)
        {
            BuildShowUI ui = child.GetComponent<BuildShowUI>();
            if (ui != null)
                bsui.Add(ui);
        }
    }

    void PruneDestroyed()
    {
        bsui.RemoveAll(ui => ui == null);
    }

    public void ClearAll()
    {
        PruneDestroyed();
        foreach (BuildShowUI ui in bsui)
        {
            ui.show_range_ui.SetActive(false);
            ui.buildable_ui.SetActive(false);
        }
    }

    public void ShowBuildable(BuildType bt)
    {
        ClearAll();
        string tag = bt == BuildType.Highland ? "highland" : "ground";
        foreach (BuildShowUI bui in bsui)
        {
            if (bui.CompareTag(tag) && bui.canbuild)
                bui.buildable_ui.SetActive(true);
        }
    }
}
