using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class StartUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ToMaker()
    {
        SceneJumper.instance.Jump("Scenes/Battle/Maker/MK_tool");
    }
    public void ToTest()
    {
        SceneJumper.instance.Jump("Scenes/Battle/Maker/Battle-MK-1");
    }
    public void StartView()
    {
        string path = EditorUtility.OpenFilePanel("打开的文件", "", "json");
        if (!string.IsNullOrEmpty(path))
        {
            FileInfo file = new FileInfo(path);
            file.CopyTo(Application.streamingAssetsPath + "/test/view.json",true);
            SceneJumper.instance.Jump("Scenes/Battle/Maker/BattleView");
        }
    }
}
