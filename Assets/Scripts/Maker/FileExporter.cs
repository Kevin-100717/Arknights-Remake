using GameData.MapData;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class FileExporter : MonoBehaviour
{
    [DllImport("User32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern int MessageBox(IntPtr handle, String message, String title, int type);
    public static FileExporter instance;
    public BattleData battleData = new BattleData();
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        battleData.Options = new OptionsEntity{
            CharacterLimit = 9,
            CostIncreaseTime = 1,
            InitialCost = 10,
            MaxCost = 99,
            MaxLifePoint = 3,
            MoveMultiplier = 0.5,
            SteeringEnabled = true,
            IsHardTrainingLevel = false,
            IsPredefinedCardsSelectable = false,
            IsTrainingLevel = false,
            MaxPlayTime = -1.0f,
            FunctionDisableMask = "None",
            ConfigBlackBoard = null
        };
    }
    public void Save(string path,bool flag)
    {
        string sp;
        if(path == "")
        {
            sp = Application.streamingAssetsPath + "/exports/temp.json";
        }
        else
        {
            sp = path;
        }
        SaveMapInfo();
        SaveRouteInfo();
        SaveEnemyInfo();
        System.IO.File.WriteAllText(sp, JsonConvert.SerializeObject(battleData,Formatting.Indented));
        if (flag) { MessageBox(IntPtr.Zero, "File saved to " + sp, "Save Success", 0); } else
        {
            Debug.Log("File saved to " + sp);
        }
    }
    void SaveMapInfo()
    {
        battleData.MapData = new MapDataEntity{
            Map = new List<List<int>>(),
            Tiles = new List<TileEntity>()
        };
        for(int o=0;o< CreateUIController.instance.width * CreateUIController.instance.height; o++)
        {
            //
            battleData.MapData.Tiles.Add(new TileEntity());
        }
        for (int i=0;i<CreateUIController.instance.height;i++)
        {
            battleData.MapData.Map.Add(new List<int>());
            int n = (CreateUIController.instance.height-1-i) * CreateUIController.instance.width;
            for (int j=0;j<CreateUIController.instance.width;j++)
            {
                battleData.MapData.Map[i].Add(n);
                battleData.MapData.Tiles[n] = (MakerCore.instance.nodeObj[n].GetComponent<MapNode>().GetNodeData());
                n++;
            }
        }
    }
    void SaveRouteInfo()
    {
        battleData.Routes = new List<RouteEntity>();
        foreach(GameObject routeUI in CreateUIController.instance.routeUIItems)
        {
            battleData.Routes.Add(routeUI.GetComponent<RouteUI>().routeData);
        }
    }
    void SaveEnemyInfo()
    {
        battleData.Waves = new List<WaveEntity>{
            new WaveEntity{
                PreDelay = 0,
                PostDelay = 0,
                MaxTimeWaitingForNextWave = -1,
                Fragments = new List<FragmentEntity>
                {
                    new FragmentEntity{
                        PreDelay = 0,
                        Actions = new List<ActionEntity>{
                        }
                    }
                },
                AdvancedWaveTag = null
            }
        };
        foreach(GameObject eui in CreateUIController.instance.enemyDataUIItems)
        {
            EnemyDataUI edui = eui.GetComponent<EnemyDataUI>();
            if (edui != null) {
                battleData.Waves[0].Fragments[0].Actions.Add(edui.GetEnemyData());
            }
         }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
