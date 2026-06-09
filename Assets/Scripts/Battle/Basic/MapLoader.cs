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
    public Transform buildRoot;
    public GameObject ground_buildable_prefab;
    public GameObject highland_buildable_prefab;
    // Start is called before the first frame update
    void Start()
    {
        Load();
    }
    public void Load()
    {
        foreach (Transform child in root)
        {
            Destroy(child.gameObject);
            Debug.Log("clear child");
        }
        battleData = JsonConvert.DeserializeObject<BattleData>(ReadData(mapDataJsonFilePath));
        LoadMap();
        if (EnemySpawner.Instance)
        {
            Debug.Log(battleData.Waves.Count);
            EnemySpawner.Instance.LoadEnemyData(battleData);
        }
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
        foreach (List<int> row in mapData.Map)
        {
            foreach(int cell in row)
            {
                TileEntity tile = tiles[cell];
                GameObject t,t2;
                if(tile.PassableMask == "ALL")
                {
                    t = Instantiate(walkableNodePrefab, new Vector3(x, y,0), Quaternion.identity);
                }
                else
                {
                    //Debug.Log("createWall");
                    t = Instantiate(wallNodePrefab, new Vector3(x, y, 0), Quaternion.identity);
                }
                if (tile.BuildableType == "MELEE" || tile.BuildableType == "ALL")
                {
                    //ground
                    t2 = Instantiate(ground_buildable_prefab, new Vector3(x, y, 0), Quaternion.identity);
                    t2.transform.SetParent(buildRoot);
                }
                else if (tile.BuildableType == "RANGED")
                {
                    //ground
                    t2 = Instantiate(highland_buildable_prefab, new Vector3(x, y, 0), Quaternion.identity);
                    t2.transform.SetParent(buildRoot);
                }
                t.transform.SetParent(root);
                x++;
            }
            x = 0;
            y--;
        }
        buildUIController.instance.LoadUI();
        StartCoroutine(waitScan());
        
    }
    IEnumerator waitScan()
    {
        yield return null;
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
