using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
