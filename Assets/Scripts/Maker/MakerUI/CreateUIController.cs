using GameData.EnemyData;
using GameData.MapData;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class CreateUIController : MonoBehaviour
{
    [DllImport("User32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern int MessageBox(IntPtr handle, String message, String title, int type);
    public static CreateUIController instance;
    public string enemyDataFile;
    public InputField enemySearchBar;
    public RectTransform searchContentTransform;
    public GameObject searchUIPrefab;
    public List<EnemyDataEntity> selectedEnemy = new List<EnemyDataEntity>();
    public RectTransform selectedEnemyContentTransform;
    public GameObject selectedEnemyUIPrefab;
    public List<EnemyDataEntity> selectedRemoved = new List<EnemyDataEntity>();
    public InputField widthInput;
    public InputField heightInput;
    public GameObject panel;
    public GameObject panel1;
    public List<GameObject> routeBtn;
    public GameObject routeBtnPrefab;
    public GameObject routeAndEnemyEditPanel;
    public RectTransform routeListContent;
    private bool panelActionFlag = true;
    public int width;
    public int height;
    public List<GameObject> routeUIItems;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        StartCoroutine(startui());
    }
    IEnumerator startui()
    {
        panel.SetActive(true);
        panel1.SetActive(false);
        yield return null;
        if (File.Exists(Application.streamingAssetsPath + "/exports/temp.json") && MessageBox(IntPtr.Zero, "检测到有存在的地图草稿，是否加载，若确认则加载，否则覆盖写入新地图（请及时备份）", "确认", 1) == 1)
        {
            loadTemp();
        }
        else
        {
            panel.SetActive(true);
            panel1.SetActive(false);
        }
    }
    void loadTemp()
    {
        MakerCore.instance.LoadMap();
        panel.SetActive(false);
        panel1.SetActive(true);
    }
    public void SearchEnemy()
    {
        //clear all search child
        foreach (Transform child in searchContentTransform)
        {
            Destroy(child.gameObject);
        }
        string enemyName = enemySearchBar.text;
        Debug.Log("search Enemy -> " + enemyName);
        EnemyData data = JsonConvert.DeserializeObject<EnemyData>(ReadData(enemyDataFile));
        List<EnemiesEntity> resultEnemies = new List<EnemiesEntity>();
        foreach (EnemiesEntity enemy in data.Enemies)
        {
            if(enemy.Key.Contains(enemyName))
            {
                Debug.Log("Enemy Found -> " + enemy.Key);
                resultEnemies.Add(enemy);
            }
        }
        Debug.Log("Result -> "+ resultEnemies.Count);
        foreach (EnemiesEntity enemy in resultEnemies)
        {
            GameObject searchUI = Instantiate(searchUIPrefab, searchContentTransform);
            searchUI.GetComponent<EnemySearchUI>().SetEnemy(enemy.Key, enemy.Value[0].EnemyData);
            searchUI.GetComponent<EnemySearchUI>().isSearchList = true;
        }
        searchContentTransform.sizeDelta = new Vector2(760,resultEnemies.Count * 178.224f);

    }
    public void UpdateSelectedEnemyUI()
    {
        //update ui
        //delete all child and add current
        foreach(Transform child in selectedEnemyContentTransform)
        {
            Destroy(child.gameObject);
        }
        foreach(EnemyDataEntity e in selectedEnemy)
        {
            GameObject searchUI = Instantiate(selectedEnemyUIPrefab, selectedEnemyContentTransform);
            searchUI.GetComponent<EnemySearchUI>().SetEnemy(e.PrefabKey.MValue, e);
            searchUI.GetComponent<EnemySearchUI>().isSearchList = false;
        }
        selectedEnemyContentTransform.sizeDelta = new Vector2(selectedEnemyContentTransform.sizeDelta.x, selectedEnemy.Count * 178.224f);
    }
    // Update is called once per frame
    void Update()
    {
        
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
    public void RemoveSelected()
    {
        foreach(EnemyDataEntity e in selectedRemoved)
        {
            if (selectedEnemy.Contains(e))
            {
                selectedEnemy.Remove(e);
            }
        }
        selectedRemoved.Clear();
        UpdateSelectedEnemyUI();
    }
    public void CreateMap()
    {
        try
        {
            width = int.Parse(widthInput.text);
            height = int.Parse(heightInput.text);
        }
        catch
        {
            return;
        }
        MakerCore.instance.CreateNode(width, height);
        panel.SetActive(false);
        panel1.SetActive(true);
    }
    public void HideOrShowEditPanel()
    {
        panelActionFlag = !panelActionFlag;
        routeAndEnemyEditPanel.SetActive(panelActionFlag);
    }
    public void NewRouteItem()
    {
        float h = 51.449f;
        GameObject rig = Instantiate(routeBtnPrefab, routeListContent);
        rig.GetComponent<RouteUI>().routeID = routeUIItems.Count;
        routeUIItems.Add(rig);
        routeListContent.sizeDelta = new Vector2(routeListContent.sizeDelta.x, h * routeUIItems.Count);
    }
    public void RouteUIClicked(GameObject g)
    {
        foreach (GameObject rui in routeUIItems) { 
            rui.GetComponent<RouteUI>().SwitchColor(rui == g);
        }
        MakerCore.instance.EnterEditRouteMode(g.GetComponent<RouteUI>().routeID);
    }
    public void SetRouteDataByRouteUIId(int rid,RouteEntity re)
    {
        routeUIItems[rid].GetComponent<RouteUI>().routeData = re;
    }
}
