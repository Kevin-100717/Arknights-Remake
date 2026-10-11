using GameData.MapData;
using Newtonsoft.Json;
using System.IO;
using UnityEngine;

public class MapViewer : MonoBehaviour
{
    public string map_data = "";
    public BattleData battleData;
    public GameObject nodePrefab;

    void Start()
    {
        map_data = Application.streamingAssetsPath + "/test/view.json";
        battleData = JsonConvert.DeserializeObject<BattleData>(ReadData(map_data));
        if (battleData?.MapData?.Map != null)
            InitMap();
    }

    public string ReadData(string path)
    {
        using (StreamReader sr = new StreamReader(path))
        {
            return sr.ReadToEnd();
        }
    }

    void InitMap()
    {
        int height = battleData.MapData.Map.Count;
        MapCoord.Height = height;
        for (int row = 0; row < height; row++)
        {
            int y = (int)MapCoord.ToWorldY(row);
            for (int col = 0; col < battleData.MapData.Map[row].Count; col++)
            {
                // tiles 是调色板，map 数组保存的是索引
                int tileIndex = battleData.MapData.Map[row][col];
                GameObject node = Instantiate(nodePrefab, transform);
                node.transform.localPosition = new Vector3(col, y, 0);
                node.GetComponent<NodeShow>().setNodeByTileData(battleData.MapData.Tiles[tileIndex]);
            }
        }
    }
}
