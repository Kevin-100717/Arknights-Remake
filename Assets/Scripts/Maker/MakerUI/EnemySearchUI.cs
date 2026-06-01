//using GameData.EnemyData;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class EnemySearchUI : MonoBehaviour
//{
//    public Image enemyIcon;
//    public Text enemyName;
//    public Text enemyDescription;
//    public bool isSearchList;
//    public EnemyDataEntity e;
//    private bool flag = false;
//    // Start is called before the first frame update
//    void Start()
//    {
        
//    }
//    public void SetEnemy(string key,EnemyDataEntity val)
//    {
//        e = val;
//        string name = val.Name.MValue;
//        string description = val.Description.MValue;
//        enemyName.text = name;
//        enemyDescription.text = description;
//        enemyIcon.sprite = Resources.Load<Sprite>("Art/enemies_icon/" + key);
//    }
//    public void OnClickEnemy()
//    {
//        if (isSearchList)
//        {
//            if (!CreateUIController.instance.selectedEnemy.Contains(e))
//            {
//                CreateUIController.instance.selectedEnemy.Add(e);
//                GetComponent<Image>().color = new Color(0, 1, 0, 38f / 255f);
//            }
//            else
//            {
//                CreateUIController.instance.selectedEnemy.Remove(e);
//                GetComponent<Image>().color = new Color(1, 1, 1, 38f / 255f);
//            }
//            CreateUIController.instance.UpdateSelectedEnemyUI();
//        }
//        else
//        {
//            flag = !flag;
//            if (flag)
//            {
//                CreateUIController.instance.selectedRemoved.Add(e);
//                GetComponent<Image>().color = new Color(1, 0, 0, 38f / 255f);
//            }
//            else
//            {
//                CreateUIController.instance.selectedRemoved.Remove(e);
//                GetComponent<Image>().color = new Color(1, 1, 1, 38f / 255f);
//            }
//        }
//    }
//    // Update is called once per frame
//    void Update()
//    {
        
//    }
//}
