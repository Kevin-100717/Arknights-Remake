using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class MapNode : MonoBehaviour
{
    public List<GameObject> map_node_model;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void SetNodeModel(int index)
    {
        for (int i = 0; i < map_node_model.Count; i++)
        {
            if (i == index)
            {
                map_node_model[i].SetActive(true);
            }
            else
            {
                map_node_model[i].SetActive(false);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
