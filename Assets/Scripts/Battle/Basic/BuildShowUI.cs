using UnityEngine;

public class BuildShowUI : MonoBehaviour
{
    public GameObject show_range_ui;
    public GameObject buildable_ui;
    public bool canbuild;
    public LayerMask rangeCollectLayer;

    void Update()
    {
        bool show = false;
        if (buildUIController.instance != null && buildUIController.instance.enable_show_range)
        {
            Vector3 origin = transform.position + new Vector3(0, 0, 1);
            show = Physics.Raycast(origin, Vector3.back, 100f, rangeCollectLayer);
        }
        if (show_range_ui.activeSelf != show)
            show_range_ui.SetActive(show);
    }
}
