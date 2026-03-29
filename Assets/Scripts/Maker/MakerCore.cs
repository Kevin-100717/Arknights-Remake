using GameData.MapData;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class MakerCore : MonoBehaviour
{
    public static MakerCore instance;
    public GameObject nodePrefab;
    public bool enterEditMode = false;
    public LayerMask nodeLayer;
    public string save_path = "";
    public List<GameObject> nodeObj = new List<GameObject>();
    public MapLoader mapLoader;
    public int editRouteID;
    public RouteEntity routeEdited;
    private bool routeFlag = false;
    public GameObject routeTagPrefab;
    public Transform checkpointTagFrame;
    public enum EditMode
    {
        Normal,
        EditEnemy,
        EditRoute,
    }
    public EditMode editMode = EditMode.Normal;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }
    public void CreateNode(int width,int height)
    {
        save_path = Application.streamingAssetsPath + "/exports/temp.json";
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                GameObject node = Instantiate(nodePrefab, transform);
                node.transform.localPosition = new Vector3(j, i, 0);
                node.GetComponent<MapNode>().SetNodeModel(0);
                nodeObj.Add(node);
            }
        }
        enterEditMode = true;
    }
    public void LoadMap()
    {
        save_path = Application.streamingAssetsPath + "/exports/temp.json";
        BattleData battleData = JsonConvert.DeserializeObject<BattleData>(ReadData("/exports/temp.json"));
        int width = battleData.MapData.Map[0].Count;
        int height = battleData.MapData.Map.Count;
        CreateUIController.instance.width = width;
        CreateUIController.instance.height = height;
        CreateNode(width, height);
        for(int i=0;i<nodeObj.Count;i++)
        {
            nodeObj[i].GetComponent<MapNode>().setNodeByTileData(battleData.MapData.Tiles[i]);
        }
        SaveFile("/exports/craft.json", false);
        mapLoader.mapDataJsonFilePath = "/exports/craft.json";
        mapLoader.enabled = true;
    }
    // Update is called once per frame
    void Update()
    {
        if (!enterEditMode) return;
        ShowAct();
        switch (editMode)
        {
            case EditMode.Normal:
                if (Input.GetMouseButtonDown(0))
                {
                    MapNode n = getTouchNode();
                    if (n)
                    {
                        n.OnClickNode();
                        SaveFile("/exports/craft.json", false);
                        mapLoader.Load();
                    }
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    MapNode n = getTouchNode();
                    if (n)
                    {
                        n.SetNodeModel(0);
                        SaveFile("/exports/craft.json", false);
                        mapLoader.Load();
                    }
                }
                break;
            case EditMode.EditRoute:
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    foreach (Transform child in checkpointTagFrame)
                    {
                        Destroy(child.gameObject);
                    }
                    CheckpointEntity ckpte = routeEdited.Checkpoints[routeEdited.Checkpoints.Count-1];
                    routeEdited.Checkpoints.Remove(ckpte);
                    routeEdited.EndPosition = ckpte.Position;
                    CreateUIController.instance.SetRouteDataByRouteUIId(editRouteID, routeEdited);
                    editMode = EditMode.Normal;
                }
                if (Input.GetMouseButtonDown(0))
                {
                    MapNode n = getTouchNode();
                    if (n)
                    {
                        Vector3 pos = n.transform.position;
                        if (!routeFlag)
                        {
                            routeFlag = true;
                            routeEdited.StartPosition = vecToPos(pos);
                            CreateCheckPointTTag(0, pos, 0);
                        }
                        else
                        {
                            CheckpointEntity ckpt = CheckPointConfigUI.instance.getCurrentConfig();
                            int pointType = CheckPointConfigUI.instance.checkPointTypeSelect.value;
                            ckpt.Position = vecToPos(pos);
                            routeEdited.Checkpoints.Add(ckpt);
                            CreateCheckPointTTag(pointType, pos, ckpt.Time);
                        }
                    }
                }
                break;
        }
    }
    void CreateCheckPointTTag(int type,Vector3 pos,float wt)
    {
        GameObject ckpttag = Instantiate(routeTagPrefab, pos, Quaternion.identity);
        ckpttag.transform.parent = checkpointTagFrame;
        ckpttag.GetComponent<CheckTagDisplay>().SetUI(type);
        if(type == 1)
        {
            ckpttag.GetComponent<CheckTagDisplay>().SetWaitText(wt);
        }
    }
    PositionEntity vecToPos(Vector3 p)
    {
        Debug.Log(p);
        return new PositionEntity
        {
            Row = p.y,
            Col = p.x,
        };
    }
    MapNode getTouchNode()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, nodeLayer);
        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.gameObject.CompareTag("node_create"))
                {
                    return hits[i].collider.gameObject.GetComponent<MapNode>();
                }
            }
        }
        return null;
    }
    void ShowAct()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, nodeLayer);
        foreach(GameObject obj in nodeObj)
        {
            obj.GetComponent<MapNode>().CancelSelect();
        }
        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.gameObject.CompareTag("node_create"))
                {
                    hits[i].collider.gameObject.GetComponent<MapNode>().OnSelected();
                    break;
                }
            }
        }
    }
    public void SaveFile(string sp,bool flag)
    {
        FileExporter.instance.Save(sp == ""?save_path: Application.streamingAssetsPath + sp,flag);
    }
    public void SaveExport()
    {
        SaveFile("", true);
    }
    public string ReadData(string path)
    {
        string fileUrl = Application.streamingAssetsPath + path;
        using (StreamReader sr = new StreamReader(fileUrl))
        {
            string readData = sr.ReadToEnd();
            sr.Close();
            return readData;
        }
    }
    public void EnterEditRouteMode(int eid)
    {
        routeEdited = new RouteEntity
        {
            MotionMode = "WALK",
            SpawnOffset = new SpawnOffsetEntity { X = 0, Y = 0 },
            SpawnRandomRange = new SpawnRandomRangeEntity { X = 0, Y = 0 },
            AllowDiagonalMove = true,
            VisitEveryCheckPoint = false,
            VisitEveryNodeCenter = false,
            VisitEveryTileCenter = false,
            StartPosition = new PositionEntity(),
            EndPosition = new PositionEntity(),
            Checkpoints = new List<CheckpointEntity>()
        };
        routeFlag = false;
        editRouteID = eid;
        foreach (Transform child in checkpointTagFrame)
        {
            Destroy(child.gameObject);
        }
        editMode = EditMode.EditRoute;
    }
}
