using NativeFileBrowser;
using System.IO;
using UnityEngine;

public class StartUI : MonoBehaviour
{
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
        var extensions = new[]
        {
            new ExtensionFilter("MapData File", "json"),
        };
        var path = StandaloneFileBrowser.OpenFilePanel("打开的文件", extensions, true);
        if (path.Length > 0)
        {
            FileInfo file = new FileInfo(path[0]);
            file.CopyTo(Application.streamingAssetsPath + "/test/view.json", true);
            SceneJumper.instance.Jump("Scenes/Battle/Maker/BattleView");
        }
    }
}
