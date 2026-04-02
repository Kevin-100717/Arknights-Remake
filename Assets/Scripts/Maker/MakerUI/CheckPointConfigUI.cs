using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using GameData.MapData;
public class CheckPointConfigUI : MonoBehaviour
{
    public static CheckPointConfigUI instance;
    public List<string> checkPointType = new List<string>();
    public Dropdown checkPointTypeSelect;
    public InputField waitTimeInput;
    private void Start()
    {
        instance = this;
    }
    public CheckpointEntity getCurrentConfig()
    {
        CheckpointEntity r = new CheckpointEntity
        {
            RandomizeReachOffset = false,
            ReachDistance = 0.0f,
            ReachOffset=new ReachOffsetEntity { X=0,Y=0 }
        };
        r.Type = checkPointType[checkPointTypeSelect.value];
        if(r.Type == "WAIT_FOR_SECONDS")
        {
            Debug.Log(waitTimeInput.text);
            r.Time = float.Parse(waitTimeInput.text);
        }
        return r;
    } 
    public int getIndexByCkpt(CheckpointEntity ckpt)
    {
        foreach(string ckptt in checkPointType)
        {
            if(ckptt == ckpt.Type)
            {
                return checkPointType.IndexOf(ckptt);
            }
        }
        return 0;
    }
}