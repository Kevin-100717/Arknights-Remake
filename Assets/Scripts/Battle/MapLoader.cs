using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using GameData.MapData;

public class MapLoader : MonoBehaviour
{
    public string mapDataJsonFilePath;
    public BattleData battleData;
    [Header("地图预制件")]
    public GameObject walkableNodePrefab;
    public GameObject wallNodePrefab;
    public AstarPath astarPath;
    public Transform root;
    // Start is called before the first frame update
    void Start()
    {
        battleData = JsonConvert.DeserializeObject<BattleData>(ReadData(mapDataJsonFilePath));
        LoadMap();
        EnemySpawner.Instance.LoadEnemyData(battleData);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void LoadMap()
    {
        MapDataEntity mapData = battleData.MapData;
        List<TileEntity> tiles = mapData.Tiles;
        int x = 0;
        int y = mapData.Map.Count-1;
        foreach (List<long> row in mapData.Map)
        {
            foreach(int cell in row)
            {
                TileEntity tile = tiles[cell];
                GameObject t;
                if(tile.PassableMask == "ALL")
                {
                    t = Instantiate(walkableNodePrefab, new Vector3(x, y,0), Quaternion.identity);
                }
                else
                {
                    t = Instantiate(wallNodePrefab, new Vector3(x, y, 0), Quaternion.identity);
                }
                t.transform.SetParent(root);
                x++;
            }
            x = 0;
            y--;
        }

        astarPath.Scan();
    }
    public string ReadData(string path)
    {
        string fileUrl = Application.streamingAssetsPath+path;
        using (StreamReader sr = new StreamReader(fileUrl))
        {
            string readData = sr.ReadToEnd();
            sr.Close();
            return readData;
        }
    }
}
