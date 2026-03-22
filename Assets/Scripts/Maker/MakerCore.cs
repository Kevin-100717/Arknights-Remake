using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakerCore : MonoBehaviour
{
    public static MakerCore instance;
    public GameObject nodePrefab;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }
    public void CreateNode(int width,int height)
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                GameObject node = Instantiate(nodePrefab, transform);
                node.transform.localPosition = new Vector3(i, j, 0);
                node.GetComponent<MapNode>().SetNodeModel(0);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
