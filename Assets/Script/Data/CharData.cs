using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Linq;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class CharData : MonoBehaviour 
{
    public List<CharInfo> charDatas;
    public static CharData instance;
    public string charDataJson;
    private JObject cdata;
    public List<CharProfession> cps;
    public List<Star_Sprite> sps;
    public string enemyDataPath = "/levels/enemydata/enemy_database.json";
    public string handBookPath = "/enemy_handbook_table.json";
    public List<EnemyData> eds;
    public JObject enemyHandbook;
    private JArray enemies;
    private int total;
    private int current;
    public List<Transform> progressBars;
    public CanvasGroup btn;
    public RectTransform ball;
    public CanvasGroup info;
    public Text prog;
    public Image black;
    private bool isLoading = true;
    private void Awake() {
        if(GameObject.FindGameObjectsWithTag("battleInfo").Length > 1){
            Destroy(gameObject);
        }else{
            DontDestroyOnLoad(gameObject);
        }
    }
    public string ReadData(string path){
        string readData;
        string fileUrl = path;
        using (StreamReader sr = File.OpenText(fileUrl)){
            readData = sr.ReadToEnd();
            sr.Close();
        }
        return readData;
    }
    private void Start() {
        instance = this;
        cdata = JObject.Parse(ReadData(Application.streamingAssetsPath+charDataJson));
        enemies = JArray.Parse(JObject.Parse(ReadData(Application.streamingAssetsPath + enemyDataPath))["enemies"].ToString());
        //ParseCharData();
        //ParseEnemyData()
        total = cdata.Count + enemies.Count;
        info.DOFade(0, 0);
        black.DOFade(0, 0);
        Debug.Log(total);
    }
    public void StartInit()
    {
        ball.DOLocalMoveY(179.73f, 0.6f);
        btn.DOFade(0, 0.8f).OnComplete(() =>
        {
            info.gameObject.SetActive(true);
            info.DOFade(1, 0.5f).OnComplete(() =>
            {
                StartCoroutine(ParseCharData());
                StartCoroutine(ParseEnemyData());
            });
        });
    }
    private void Update()
    {
        if(current == total && isLoading)
        {
            isLoading = false;
            black.gameObject.SetActive(true);
            black.DOFade(1, 0.8f).OnComplete(() =>
            {
                SceneManager.LoadScene("Scenes/UI/LevelList0");
            });
        }
    }
    private void UpdateScale()
    {
        float scale = (float)current / (float)total;
        progressBars[0].localScale = new Vector3(scale, 1, 1);
        progressBars[1].localScale = new Vector3(scale, 1, 1);
        prog.text = (Mathf.RoundToInt(((float)current/total)*100)).ToString() + "%";
    }
    public string getLevel(string type,float value)
    {
        JArray infoList = JArray.Parse(enemyHandbook["levelInfoList"].ToString());
        foreach (JObject obj in infoList)
        {
            if (float.Parse(obj[type]["min"].ToString()) <= value && float.Parse(obj[type]["max"].ToString()) >= value)
            {
                return obj["classLevel"].ToString();
            }
        }
        return "";
    }
    private IEnumerator ParseCharData()
    {
        // 1. 预加载所有 Prefab
        Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();
        foreach (var prop in cdata.Properties())
        {
            string key = prop.Name;
            if (key.StartsWith("char_"))
            {
                string charPrefabName = "Char" + key.Split('_')[1];
                if (!prefabCache.ContainsKey(charPrefabName))
                {
                    var prefab = Resources.Load<GameObject>("Prefab/Battle/Char/" + charPrefabName);
                    prefabCache[charPrefabName] = prefab;
                }
            }
        }

        // 2. 预加载所有头像和半身像
        Dictionary<string, Sprite> iconBoxCache = new Dictionary<string, Sprite>();
        Dictionary<string, Sprite> iconHalfCache = new Dictionary<string, Sprite>();
        foreach (var prop in cdata.Properties())
        {
            string key = prop.Name;
            if (key.StartsWith("char_"))
            {
                if (!iconBoxCache.ContainsKey(key))
                    iconBoxCache[key] = Resources.Load<Sprite>("CharHead/" + key);
                string halfKey = "char_" + key.Split('_')[1];
                if (!iconHalfCache.ContainsKey(halfKey))
                    iconHalfCache[halfKey] = Resources.Load<Sprite>("CharHalf/" + halfKey);
            }
        }

        int count = 0;
        var e = cdata.GetEnumerator();
        while (e.MoveNext())
        {
            if (e.Current.Key.ToString().Split("_")[0] != "char")
            {
                current++;
                UpdateScale();
                continue;
            }
            string charPrefabName = "Char" + e.Current.Key.ToString().Split("_")[1];
            GameObject prefab = prefabCache[charPrefabName];
            if (prefab == null)
            {
                current++;
                UpdateScale();
                continue;
            }
            var phases = e.Current.Value["phases"];
            int l = phases.Count();
            var attr = phases[l - 1]["attributesKeyFrames"];
            JObject val = (JObject)attr.Last["data"];
            string profession = e.Current.Value["profession"].ToString();
            Sprite pi = cps.FirstOrDefault(cp => cp.profession == profession)?.icon;
            int rarityIndex = int.Parse(e.Current.Value["rarity"].ToString().Split('_')[1]) - 1;
            string key = e.Current.Key.ToString();
            string halfKey = "char_" + key.Split('_')[1];
            charDatas.Add(new CharInfo()
            {
                cid = int.Parse(key.Split('_')[1]),
                charType = e.Current.Value["position"].ToString() == "MELEE" ? CharType.LowLand : CharType.HighLand,
                charPrefab = prefab,
                cost = int.Parse(val["cost"].ToString()),
                max_hp = int.Parse(val["maxHp"].ToString()),
                def = int.Parse(val["def"].ToString()),
                atk = int.Parse(val["atk"].ToString()),
                mdef = int.Parse(val["magicResistance"].ToString()),
                replaceTime = int.Parse(val["respawnTime"].ToString()),
                def_num = int.Parse(val["blockCnt"].ToString()),
                name = e.Current.Value["name"].ToString(),
                name_en = e.Current.Value["appellation"].ToString(),
                cui = new CharUIData()
                {
                    icon_box = iconBoxCache[key],
                    icon_half = iconHalfCache[halfKey],
                    icon_type = pi,
                    sp = sps[rarityIndex]
                }
            });
            current++;
            UpdateScale();
            count++;
            if (count % 20 == 0) yield return null; // 每10个yield一次
        }
    }

    private IEnumerator ParseEnemyData()
    {
        // 1. 预加载所有敌人icon
        Dictionary<string, Sprite> enemyIconCache = new Dictionary<string, Sprite>();
        foreach (JObject enemy in enemies)
        {
            string key = enemy["Key"].ToString();
            if (!enemyIconCache.ContainsKey(key))
                enemyIconCache[key] = Resources.Load<Sprite>("enemies_icon/" + key);
        }

        enemyHandbook = JObject.Parse(ReadData(Application.streamingAssetsPath + handBookPath));
        int count = 0;
        foreach (JObject enemy in enemies)
        {
            try
            {
                string key = enemy["Key"].ToString();
                var value = enemy["Value"][0]["enemyData"];
                var attr = value["attributes"];
                EnemyData ed = new EnemyData()
                {
                    key = key,
                    icon = enemyIconCache[key],
                    name = value["name"]["m_value"].ToString(),
                    introduction = enemyHandbook["enemyData"][key]["description"].ToString(),
                    atk = getLevel("attack", float.Parse(attr["atk"]["m_value"].ToString())),
                    life = getLevel("maxHP", float.Parse(attr["maxHp"]["m_value"].ToString())),
                    mdef = getLevel("magicRes", float.Parse(attr["magicResistance"]["m_value"].ToString())),
                    def = getLevel("def", float.Parse(attr["def"]["m_value"].ToString())),
                    group = enemyHandbook["enemyData"][key]["enemyIndex"].ToString(),
                };
                eds.Add(ed);
            }
            catch
            {
                Debug.Log("Error");
            }
            current++;
            UpdateScale();
            count++;
            if (count % 20 == 0) yield return null; // 每10个yield一次
        }
    }
}