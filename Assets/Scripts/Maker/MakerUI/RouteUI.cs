using GameData.MapData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RouteUI : MonoBehaviour
{
    public int routeID = 0;
    public Text routeText;
    public Image btnBg;
    public Color clickedColor;
    public Color NormalColor;
    public RouteEntity routeData;
    // Start is called before the first frame update
    void Start()
    {
        routeText.text = "ROUTE " + routeID.ToString();
    }
    public void OnClicked()
    {
        CreateUIController.instance.RouteUIClicked(gameObject);
    }
    public void SwitchColor(bool flag)
    {
        Color c = flag ? clickedColor : NormalColor;
        btnBg.color = c;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
