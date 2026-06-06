using GameData.MapData;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapViewer : MonoBehaviour
{
    public string map_data = "";
    public BattleData battleData;
    public GameObject nodePrefab;
    // Start is called before the first frame update
    void Start()
    {
        map_data = Application.streamingAssetsPath + "/test/view.json";
        battleData = JsonConvert.DeserializeObject<BattleData>(ReadData(map_data));
        InitMap();
    }
    public string ReadData(string path)
    {
        string fileUrl = path;
        using (StreamReader sr = new StreamReader(fileUrl))
        {
            string readData = sr.ReadToEnd();
            sr.Close();
            return readData;
        }
    }
    void InitMap()
    {
        int width = battleData.MapData.Map[0].Count;
        int height = battleData.MapData.Map.Count;
        int id = 0;
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                GameObject node = Instantiate(nodePrefab, transform);
                node.transform.localPosition = new Vector3(j, i, 0);
                node.GetComponent<NodeShow>().setNodeByTileData(battleData.MapData.Tiles[id]);
                id++;
            }
        }
    }
            // Update is called once per frame
    void Update()
    {
        
    }
}
