using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneJumper : MonoBehaviour
{
    public static SceneJumper instance;
    private void Awake()
    {
        //Dont destroy on load
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void Jump(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    } 
    // Update is called once per frame
    void Update()
    {
        
    }
}
