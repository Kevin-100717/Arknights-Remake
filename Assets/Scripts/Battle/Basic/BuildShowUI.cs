using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildShowUI : MonoBehaviour
{
    public GameObject show_range_ui;
    public GameObject buildable_ui;
    public bool canbuild;
    public LayerMask rangeCollectLayer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (buildUIController.instance.enable_show_range)
        {
            Debug.DrawRay(transform.position+new Vector3(0,0,1), Vector3.forward * -100f, Color.red);
            RaycastHit[] hits = Physics.RaycastAll(transform.position + new Vector3(0, 0, 1), Vector3.forward*-1, 100f, rangeCollectLayer);
            if (hits.Length > 0) 
            {
                show_range_ui.SetActive(true);
            }
            else
            {
                show_range_ui.SetActive(false);
            }
        }
        else
        {
            show_range_ui.SetActive(false);
        }
    }
}
