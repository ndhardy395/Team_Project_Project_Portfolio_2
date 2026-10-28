using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;






public class ClickCode : MonoBehaviour
{
    public static ClickCode Instance { get; private set; }

    [SerializeField] private GameObject MainManu;
    [SerializeField] private GameObject LevelManu;
    [SerializeField] private GameObject SettingManu;
    [SerializeField] private GameObject PauseManu;
    [SerializeField] private GameObject HUD;
    [SerializeField]
    private List<GameObject> TowerPrefabs;
    private int towerPrefabNumber;

    private GameObject last;
    private string sceneToLoad;
    
    private GameObject towerspace ;
    private string Curentlevel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        towerspace = new GameObject();
        Time.timeScale = 0.0f;
        if (Instance == null)
        {
            
            Instance = this;

            // 2. Tell Unity not to destroy this object when loading a new scene
            DontDestroyOnLoad(gameObject);
        }
      // 3. If an instance already exists, destroy this duplicate copy immediately
        else
            {
                  Destroy(gameObject);
            }

        }

    // Update is called once per frame
    void Update()
    {

    }
    public void SetTowerSpace(GameObject tower)
    {
        towerspace = tower;
    }
    public void SetTowerPrefab(int towerPrefabGet)
    {
      towerPrefabNumber = towerPrefabGet;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Example: re-hook an object by tag or name in the newly loaded scene
        GameObject foundObj = GameObject.FindWithTag("PlayerTarget");
        if (foundObj != null)
        {
           // targetGameObject = foundObj;
        }
    }


    //List to clear canvis
    public void ClearAll()
    {

        MainManu.SetActive(false);
        LevelManu.SetActive(false);
        SettingManu.SetActive(false);
        PauseManu.SetActive(false);
        HUD.SetActive(false);
        
      
    }
    //end to clear canvis
    //Main Manu
    public void OnMainLevelClick()
    {
        last = MainManu;
        ClearAll();
        MainManu.SetActive(false);
        LevelManu.SetActive(true);
    }
  public void OnMainSettingClick()
    {
        last = MainManu;
        ClearAll();
        SettingManu.SetActive(true);
    }
    public void OnMainQuitClick()
    {
        last = MainManu;
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

  
    //
    //Level Manu
    public void OnLevelBackClick()
    {
        last = LevelManu;
        ClearAll();
        MainManu.SetActive(true);
    }
    public void OnLeveLLevel1Click()
    {
        sceneToLoad = "Path Scene_Mikey";
        //sceneToLoad = "Logon";
        last = LevelManu;
        ClearAll();
        LevelManu.SetActive(false);
        HUD.SetActive(true);
        if (!SceneManager.GetSceneByName(sceneToLoad).isLoaded)
        {   
            SceneManager.LoadScene(sceneToLoad);// (LoadSceneMode.Additive)
        }
    }
    //
    //setting Manu
    public void OnSettingBackClick()
    {
       

        ClearAll();

        last.SetActive(true);
        if (last == PauseManu)
        {
            HUD.SetActive(true);
        }
    }
    //
    //HUD
    public void OnHUDPauseClick()
    {
        Time.timeScale = 0.0f;
        ClearAll();
        PauseManu.SetActive(true);
        HUD.SetActive(true);
    }

    public void OnHUDStartClick()
    {
        Transform temp =HUD.transform.Find("Start");
        temp.GameObject().SetActive(false);
        Time.timeScale = 1.0f;
    }

    public void OnHUDNextClick()
    {
        Debug.Log("Next button reached ClickCode.");

        if(WaveSpawner.Instance == null)
        {
            Debug.LogError("No waveSpawner instance found.");
            return;
        }

        if(Time.timeScale == 0f)
        {
            Debug.Log("Click Start or resume before requestin a wave");
            return;
        }

        WaveSpawner.Instance.RequestNextWave();
    }
    
    //Tower HUD
     public void OnTowerHUDTowerClick()
    {
        if (TowerPrefabs != null)
        {
            if (towerPrefabNumber>1)
            {
            Instantiate(TowerPrefabs[towerPrefabNumber-1], towerspace.transform.position,Quaternion.identity);

            }
        }
       towerspace.transform.gameObject.SetActive(false);
    }

    //
    //Pause Manu
    public void OnPauseBackClick()
    {
        Time.timeScale = 1.0f;
        last = PauseManu;
        ClearAll();
        HUD.SetActive(true);

    }
    public void OnPauseSettingClick()
    {
        last = PauseManu;
        ClearAll();
        SettingManu.SetActive(true);
    }
    public void OnPauseResetClick()
    {
        SceneManager.LoadScene(sceneToLoad);
        PauseManu.SetActive(false);
        Transform temp = HUD.transform.Find("Start");
        temp.GameObject().SetActive(true);
    }
    public void OnPauseLeaveClick()
    {
        last = PauseManu;
        ClearAll();
        HUD.SetActive(false);
        LevelManu.SetActive(true);
        Debug.Log("leave" );
        if (SceneManager.GetSceneByName(sceneToLoad).isLoaded)
        {

            SceneManager.LoadScene("UI");
           
        }
        Transform temp = HUD.transform.Find("Start");
        temp.GameObject().SetActive(true);
        Time.timeScale = 0.0f;
        

    }


    // when player click leave in game over scene it take them back to menu

    public void ReturnToMainMenu()
    {
        Time.timeScale = 0f;

        ClearAll();
        MainManu.SetActive(true);
        last = MainManu;

        // Restore the Start button to the next time a level is opened

        Transform startButton = HUD.transform.Find("Start");

        if (startButton != null)
        {
            startButton.gameObject.SetActive(true);
        }

        // Loading UI replaces the level and removes it scene objects
        SceneManager.LoadScene("UI");
    }





}
