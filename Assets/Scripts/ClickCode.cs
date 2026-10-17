using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;







public class ClickCode : MonoBehaviour
{
    [SerializeField] private GameObject MainManu;
    [SerializeField] private GameObject LevelManu;
    [SerializeField] private GameObject SettingManu;
    [SerializeField] private GameObject PauseManu;
    [SerializeField] private GameObject HUD;

    private GameObject last;
    private string sceneToLoad;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Time.timeScale = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {

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
        sceneToLoad = "level 1";
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
