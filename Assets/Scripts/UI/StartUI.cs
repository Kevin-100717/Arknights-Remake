using NativeFileBrowser;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
//sing System.Windows.Forms;

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
        //OpenFileDialog dialog = new OpenFileDialog
        //{
        //    Title = "打开的文件",
        //    Filter = "json files(*.json)|*.json",
        //};
        //DialogResult result = dialog.ShowDialog();
        //if (result == DialogResult.OK)
        //{
        var extensions = new[]
{
            new ExtensionFilter("MapData File", "json"),
        };
        var path = StandaloneFileBrowser.OpenFilePanel("打开的文件", extensions, true);
        if (path.Length > 0)
        {
            FileInfo file = new FileInfo(path[0]);
            file.CopyTo(UnityEngine.Application.streamingAssetsPath + "/test/view.json",true);
            SceneJumper.instance.Jump("Scenes/Battle/Maker/BattleView");
        }
    }
}
