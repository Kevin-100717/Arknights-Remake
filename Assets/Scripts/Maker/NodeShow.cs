using GameData.MapData;
using System.Collections.Generic;
using UnityEngine;

public class NodeShow : MonoBehaviour
{
    public int currentModelIndex;
    public List<GameObject> map_node_model;
    public List<TileEntity> tileEntity = new List<TileEntity>();

    public void setNodeByTileData(TileEntity t)
    {
        for (int i = 0; i < tileEntity.Count; i++)
        {
            if (tileEntity[i].TileKey == t.TileKey &&
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
    public void SetNodeModel(int index)
    {
        currentModelIndex = index;
        for (int i = 0; i < map_node_model.Count; i++)
        {
            map_node_model[i].SetActive(i == index);
        }
    }
}
