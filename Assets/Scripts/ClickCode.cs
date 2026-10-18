using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;







public class ClickCode : MonoBehaviour
{
    public static ClickCode Instance { get; private set; }

    [SerializeField] private GameObject MainManu;
    [SerializeField] private GameObject LevelManu;
    [SerializeField] private GameObject SettingManu;
    [SerializeField] private GameObject PauseManu;
    [SerializeField] private GameObject HUD;
    [SerializeField] private GameObject TowerHUD;
    [SerializeField] private GameObject Tower1Prefab;

    private GameObject last;
    private string sceneToLoad;

    public GameObject towerspace;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Time.timeScale = 0.0f;
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SetTowerSpace(GameObject tower)
    {
        towerspace = tower;
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
        Time.timeScale = 1.0f;
        last = LevelManu;
        ClearAll();
        LevelManu.SetActive(false);
        HUD.SetActive(true);
        if (!SceneManager.GetSceneByName(sceneToLoad).isLoaded)
        {   
            SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Additive);
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
    //
    //Tower HUD
    public void OnTowerHUDBackClick()
    {
        TowerHUD.SetActive(false);
    }

    public void OnTowerHUDTower1Click()
    {
        TowerHUD.SetActive(false);
       
        Instantiate(Tower1Prefab,towerspace.transform.position,Quaternion.identity);
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
    public void OnPauseLeaveClick()
    {
        last = PauseManu;
        ClearAll();
        HUD.SetActive(false);
        LevelManu.SetActive(true);
        if (SceneManager.GetSceneByName(sceneToLoad).isLoaded)
        {
            SceneManager.UnloadSceneAsync(sceneToLoad);
        }
    }
    //





}
