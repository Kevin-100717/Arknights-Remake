using GameData.MapData;
using Newtonsoft.Json;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
    public LineRenderer routeLineRenderer;
    public Seeker routeSeeker;
    public enum EditMode
    {
        Normal,
        EditEnemy,
        EditRoute,
        ViewRoute,
    }
    public EditMode editMode = EditMode.Normal;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        mapLoader.mapDataJsonFilePath = "/exports/craft.json";
        mapLoader.enabled = true;
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
        foreach(RouteEntity ret in battleData.Routes)
        {
            CreateUIController.instance.LoadRouteItem(ret);
        }
        foreach(ActionEntity actionEntity in battleData.Waves[0].Fragments[0].Actions)
        {
            CreateUIController.instance.LoadEnemyDataItem(actionEntity);
        }
        CreateNode(width, height);
        for(int i=0;i<nodeObj.Count;i++)
        {
            nodeObj[i].GetComponent<MapNode>().setNodeByTileData(battleData.MapData.Tiles[i]);
        }
        SaveFile("/exports/craft.json", false);
        mapLoader.Load();
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
                    ClearRouteLine();
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
                            RefreshLineByCkpt();
                        }
                    }
                }
                break;
            case EditMode.ViewRoute:
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    foreach (Transform child in checkpointTagFrame)
                    {
                        Destroy(child.gameObject);
                    }
                    ClearRouteLine();
                    editMode = EditMode.Normal;
                }
                if (Input.GetKeyDown(KeyCode.E))
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
                    foreach (Transform child in checkpointTagFrame)
                    {
                        Destroy(child.gameObject);
                    }
                    ClearRouteLine();
                    editMode = EditMode.EditRoute;
                }
                break;
        }
    }
    void RefreshLineByCkpt()
    {
        // DISAPPEAR到APPEAR_AT_POS之间的点不寻路连接，APPEAR_AT_POS后继续正常连接
        const int DISAPPEAR = 2;
        const int APPEAR_AT_POS = 3;

        List<Vector3> points = new List<Vector3>();
        List<int> types = new List<int>();
        points.Add(new Vector3(routeEdited.StartPosition.Col, routeEdited.StartPosition.Row, 0));
        types.Add(0); // 起点type为0

        foreach (var ckpt in routeEdited.Checkpoints)
        {
            points.Add(new Vector3(ckpt.Position.Col, ckpt.Position.Row, 0));
            types.Add(CheckPointConfigUI.instance.getIndexByCkpt(ckpt));
        }
        if (editMode == EditMode.ViewRoute)
        {
            points.Add(new Vector3(routeEdited.EndPosition.Col, routeEdited.EndPosition.Row, 0));
            types.Add(-1); // EndPosition无type
        }

        List<(int, int)> connectSegments = new List<(int, int)>();
        int startIdx = 0;
        int i = 1;
        while (i < points.Count)
        {
            int t = types[i];
            if (t == APPEAR_AT_POS)
            {
                // APPEAR_AT_POS 视为新的起点，不连接上一段
                startIdx = i;
                i++;
                continue;
            }
            if (types[startIdx] == DISAPPEAR)
            {
                // 跳过DISAPPEAR段，直到遇到APPEAR_AT_POS
                i++;
                continue;
            }
            // 连接startIdx到i
            connectSegments.Add((startIdx, i));
            startIdx = i;
            i++;
        }
        StartCoroutine(DrawRouteLineSegments(points, connectSegments));
    }

    IEnumerator DrawRouteLineSegments(List<Vector3> points, List<(int, int)> segments)
    {
        if (segments.Count == 0)
        {
            ClearRouteLine();
            yield break;
        }
        List<Vector3> fullPath = new List<Vector3>();
        foreach (var seg in segments)
        {
            int i = seg.Item1;
            int j = seg.Item2;
            bool finished = false;
            Pathfinding.Path path = null;
            routeSeeker.StartPath(points[i], points[j], p => {
                path = p;
                finished = true;
            });
            while (!finished)
                yield return null;
            if (path.error || path.vectorPath == null || path.vectorPath.Count == 0)
                continue;
            // 避免重复点
            if (fullPath.Count > 0 && fullPath[fullPath.Count - 1] == path.vectorPath[0])
                fullPath.AddRange(path.vectorPath.Skip(1));
            else
                fullPath.AddRange(path.vectorPath);
        }
        routeLineRenderer.positionCount = fullPath.Count;
        routeLineRenderer.SetPositions(fullPath.ToArray());
    }
    void ClearRouteLine()
    {
        routeLineRenderer.positionCount = 0;
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
    public void EnterEditRouteMode(int eid,bool flag,RouteEntity re = null)
    {
        if (flag)
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
            ClearRouteLine();
            editMode = EditMode.EditRoute;
        }
        else
        {
            routeEdited = re;
            routeFlag = true;
            editRouteID = eid;
            foreach (Transform child in checkpointTagFrame)
            {
                Destroy(child.gameObject);
            }
            ClearRouteLine();
            Vector3 startPos = new Vector3(routeEdited.StartPosition.Col, routeEdited.StartPosition.Row, 0);
            CreateCheckPointTTag(0, startPos, 0);
            foreach (var ckpt in routeEdited.Checkpoints)
            {
                Vector3 pos = new Vector3(ckpt.Position.Col, ckpt.Position.Row, 0);
                CreateCheckPointTTag(CheckPointConfigUI.instance.getIndexByCkpt(ckpt), pos, ckpt.Time);
            }
            editMode = EditMode.ViewRoute;
            RefreshLineByCkpt();
        }
    }
}
