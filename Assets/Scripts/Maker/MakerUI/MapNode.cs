using GameData.MapData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapNode : MonoBehaviour
{
    public int currentModelIndex;
    public List<GameObject> map_node_model;
    public Image selected;
    public List<TileEntity> tileEntity = new List<TileEntity>();
    // Start is called before the first frame update
    void Start()
    {
        selected.gameObject.SetActive(false);
    }
    public void OnClickNode()
    {
        currentModelIndex++;
        if (currentModelIndex == map_node_model.Count) { 
            currentModelIndex = 0;
        }
        SetNodeModel(currentModelIndex);
    }
    public TileEntity GetNodeData()
    {
        return tileEntity[currentModelIndex];
    }
    public void setNodeByTileData(TileEntity t)
    {
        for(int i = 0; i < tileEntity.Count; i++)
        {
            if(tileEntity[i].TileKey == t.TileKey &&
                tileEntity[i].PassableMask == t.PassableMask &&
                tileEntity[i].BuildableType == t.BuildableType &&
                tileEntity[i].HeightType == t.HeightType)
            {
                //Debug.Log("Model index: " + i);
                SetNodeModel(i);
                return;
            }
        }
        Debug.LogError("No matching tile data found for the given TileEntity.");
    }
    public void OnSelected()
    {
        selected.gameObject.SetActive(true);
    }
    public void CancelSelect()
    {
        selected.gameObject.SetActive(false);
    }
    public void SetNodeModel(int index)
    {
        currentModelIndex = index;
        //Debug.Log("Set index -> " + index.ToString());
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
