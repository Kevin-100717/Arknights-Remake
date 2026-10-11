using GameData.MapData;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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

    void Start()
    {
        Load();
    }

    public void Load()
    {
        if (string.IsNullOrEmpty(mapDataJsonFilePath)) return;

        if (!File.Exists(Application.streamingAssetsPath + mapDataJsonFilePath))
        {
            Debug.LogWarning($"[MapLoader] 关卡文件不存在，跳过加载: {mapDataJsonFilePath}");
            return;
        }

        ClearChildren(root);
        ClearChildren(buildRoot);

        string json;
        try
        {
            json = ReadData(mapDataJsonFilePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MapLoader] 读取关卡失败: {mapDataJsonFilePath}\n{e.Message}");
            return;
        }

        battleData = JsonConvert.DeserializeObject<BattleData>(json);
        if (battleData == null || battleData.MapData == null || battleData.MapData.Map == null)
        {
            Debug.LogError($"[MapLoader] 关卡数据无效: {mapDataJsonFilePath}");
            return;
        }

        MapCoord.Height = battleData.MapData.Map.Count;
        LoadMap();

        if (GameController.instance != null)
            GameController.instance.ApplyOptions(battleData.Options);

        if (EnemySpawner.Instance != null)
            EnemySpawner.Instance.LoadEnemyData(battleData);
    }

    static void ClearChildren(Transform parent)
    {
        if (parent == null) return;
        foreach (Transform child in parent)
            Destroy(child.gameObject);
    }

    public void LoadMap()
    {
        MapDataEntity mapData = battleData.MapData;
        List<TileEntity> tiles = mapData.Tiles;
        int height = mapData.Map.Count;

        for (int row = 0; row < height; row++)
        {
            float y = MapCoord.ToWorldY(row);
            for (int col = 0; col < mapData.Map[row].Count; col++)
            {
                TileEntity tile = tiles[mapData.Map[row][col]];
                GameObject prefab = tile.PassableMask == "ALL" ? walkableNodePrefab : wallNodePrefab;
                GameObject t = Instantiate(prefab, new Vector3(col, y, 0), Quaternion.identity);
                t.transform.SetParent(root);

                GameObject plate = Instantiate(
                    tile.HeightType == "HIGHLAND" ? highland_buildable_prefab : ground_buildable_prefab,
                    new Vector3(col, y, 0), Quaternion.identity);
                plate.transform.SetParent(buildRoot);

                BuildShowUI buildUI = plate.GetComponent<BuildShowUI>();
                if (buildUI != null)
                    buildUI.canbuild = tile.BuildableType != "NONE";
            }
        }

        if (buildUIController.instance != null)
            buildUIController.instance.LoadUI();

        StartCoroutine(WaitScan());
    }

    IEnumerator WaitScan()
    {
        yield return null;
        if (astarPath != null)
            astarPath.Scan();
    }

    public string ReadData(string path)
    {
        using (StreamReader sr = new StreamReader(Application.streamingAssetsPath + path))
        {
            return sr.ReadToEnd();
        }
    }
}
