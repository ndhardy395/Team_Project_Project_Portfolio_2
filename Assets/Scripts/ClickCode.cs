using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class ClickCode : MonoBehaviour
{
    public static ClickCode Instance { get; private set; }

    [SerializeField] private GameObject MainManu;
    [SerializeField] private GameObject LevelMenu;
    [SerializeField] private GameObject SettingManu;
    [SerializeField] private GameObject PauseManu;
    [SerializeField] private GameObject HUD;
    [SerializeField] private GameObject CreditsManu;
    [SerializeField] private GameObject BaseManu;
    
    [SerializeField] private GameObject TowerManu;
    [SerializeField] private List<GameObject> TowerPrefabs;

    [SerializeField] private RectTransform pauseContent;


    private int towerPrefabNumber;

    public GameObject last;
    private string sceneToLoad;

    private GameObject towerspace;
    private GameObject towerSleted;
    private string Curentlevel;

    private int pauseOpendedFrame = -1;

    private Dictionary<Vector3, int> towersCostList = new Dictionary<Vector3, int>();
    private Dictionary<Vector3, GameObject> towersGameobjectList = new Dictionary<Vector3, GameObject>();
    private Dictionary<Vector3, GameObject> towerBases = new Dictionary<Vector3, GameObject>();   // the hidden base under each built tower, shown again when it's sold

    public GameObject GetMainMenu()
    {
        return MainManu;
    }
    public GameObject GetLevelMenu()
    {
       
        return LevelMenu;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
      
        Time.timeScale = 0.0f;
        if (Instance == null)
        {

            Instance = this;

            // 2. Tell Unity not to destroy this object when loading a new scene
            DontDestroyOnLoad(gameObject);

            // ClickCode outlives levels, but the towers don't, so forget them whenever a level (re)loads.
            SceneManager.sceneLoaded += ForgetTowers;
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


        if (TowerManu.activeSelf && towerspace != null)
        {
            SellTowerUp();


        }
        if (HUD.activeSelf)
        {
            if ( HUD.transform.Find("Start").gameObject.activeSelf)
            {
 HUD.transform.Find("Next").GameObject().SetActive(false);            
            HUD.transform.Find("Spells").GameObject().SetActive(false);
            }
            else
            {
                HUD.transform.Find("Next").GameObject().SetActive(true);
                HUD.transform.Find("Spells").GameObject().SetActive(true);
            }
           
        }




        if (PauseManu == null || !PauseManu.activeInHierarchy || pauseContent == null || Mouse.current == null)
        {
            return;
        }

        // Ignore the clicks that opened the pause menu.
        if(Time.frameCount == pauseOpendedFrame || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        Canvas canvas = pauseContent.GetComponentInParent<Canvas>();

        Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;

        bool clickedInside = RectTransformUtility.RectangleContainsScreenPoint(pauseContent, Mouse.current.position.ReadValue(), uiCamera);

        if(!clickedInside)
        {
            OnPauseBackClick();
        }
    }
    // Shows what the selected tower sells for on the Sell button's label.
    public void SellTowerUp()
    {
        Transform sell = TowerManu.transform.Find("Sell/Sell label");
        TextMeshProUGUI SellText = sell.GetComponent<TextMeshProUGUI>();

        // A tower placed in the scene by hand wasn't bought, so it isn't in the list: it shows 0 and can't be sold.
        towersCostList.TryGetValue(towerspace.transform.position, out int spent);

        string label = "Sell: " + SellValue(spent);
        if (SellText.text != label) SellText.text = label;   // only touch the text when the number changes
    }

    // 70% of everything spent on a spot (build price + upgrades). The label and OnTowerHudSell both use this.
    private int SellValue(int spent) => (int)(spent * .7f);

    private void ForgetTowers(Scene scene, LoadSceneMode mode)
    {
        towersCostList.Clear();
        towersGameobjectList.Clear();
        towerBases.Clear();
    }


    public void SetTowerSpace(GameObject tower)
    {
        if (towerspace != null && tower != null)
        {

            if (towerspace == tower)
            {
               
                    
                towerspace = tower;
            }
            else
            {

                if (towerspace.TryGetComponent<BassClick>(out BassClick temp))
                {
                    towerspace.transform.Find("Cube").GetComponent<Renderer>().material.color = Color.red;

                }
                towerspace = tower;
                towersGameobjectList[tower.transform.position] = tower;

            }
        }
        else
        {
            towerspace = tower;
        }

    }
    public GameObject GetBaseManu()
    {
        return BaseManu;
    }
    public void SetTowerPrefab(int towerPrefabGet)
    {
        towerPrefabNumber = towerPrefabGet;
    }
    public int GetNumberOfPrfabs()
    {
        return TowerPrefabs.Count;
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
        LevelMenu.SetActive(false);
        SettingManu.SetActive(false);
        PauseManu.SetActive(false);
        HUD.SetActive(false);
        CreditsManu.SetActive(false);
        BaseManu.SetActive(false);
        TowerManu.SetActive(false);
    }

    public void ClearAllSettingPanel()
    {
        SettingManu.transform.Find("video Panel").gameObject.SetActive(false);
        SettingManu.transform.Find("Adio Panel").gameObject.SetActive(false);
    }
    //end to clear canvis
    //Main Manu
    public void OnMainLevelClick()
    {
        last = MainManu;
        ClearAll();
        MainManu.SetActive(false);
        LevelMenu.SetActive(true);
    }
    public void OnMainSettingClick()
    {
        last = MainManu;
        ClearAll();
        SettingManu.SetActive(true);
    }
    public void OnMainCreditsClick()
    {
        last = MainManu;
        ClearAll();
        CreditsManu.SetActive(true);
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
        last = LevelMenu;
        ClearAll();
        MainManu.SetActive(true);
    }
    public void OnLeveLLevel1Click()
    {
        StartLevel("Path Scene_Mikey");
        //StartLevel("Logon");
    }

    // The level select (LevelSelectScreen) calls this with the scene name from the level's LevelData.
    public void StartLevel(string levelScene)
    {
        sceneToLoad = levelScene;
        last = LevelMenu;
        ClearAll();
        LevelMenu.SetActive(false);
        HUD.SetActive(true);
        // The level waits for the HUD's Start button (the Shop sets timeScale back to 1).
        Time.timeScale = 0.0f;
        if (!SceneManager.GetSceneByName(sceneToLoad).isLoaded)
        {
            SceneManager.LoadScene(sceneToLoad);// (LoadSceneMode.Additive)
        }
    }

    // LevelSelectScreen shows the 3D level select map whenever this is true.
    public bool IsLevelMenuOpen => LevelMenu != null && LevelMenu.activeSelf;
    //
    //setting Manu
    public void OnSettingBackClick()
    {


        ClearAll();

        last.SetActive(true);
        if (last == PauseManu)
        {
            pauseOpendedFrame = Time.frameCount;
            HUD.SetActive(true);
        }
    }

    public void OnSettingVideoClick()
    {
        ClearAllSettingPanel();
        SettingManu.transform.Find("video Panel").gameObject.SetActive(true);

    }
    public void OnSettingAdioClick()
    {
        ClearAllSettingPanel();
        SettingManu.transform.Find("Adio Panel").gameObject.SetActive(true);

    }



    //
    //Credits Manu

    public void ClearCredits()
    {
        Transform temp = CreditsManu.transform.Find("Coders Panel");
        temp.gameObject.SetActive(false);
        temp = CreditsManu.transform.Find("Assets Panel");
        temp.gameObject.SetActive(false);
    }

    public void OnCreditsBackClick()
    {
        last = CreditsManu;
        ClearAll();
        MainManu.SetActive(true);
    }
    public void OnCreditsCodersClick()
    {
        ClearCredits();
        Transform temp= CreditsManu.transform.Find("Coders Panel");
        temp.gameObject.SetActive(true);
    }
    public void OnCreditsAssetsClick()
    {
        ClearCredits();
        Transform temp= CreditsManu.transform.Find("Assets Panel");
        temp.gameObject.SetActive(true);
    }


    //
    //HUD
    public void OnHUDPauseClick()
    {
        pauseOpendedFrame = Time.frameCount;

        Time.timeScale = 0.0f;
        ClearAll();
        PauseManu.SetActive(true);
        HUD.SetActive(false);
    }

    public void OnHUDStartClick()
    {
        Transform temp = HUD.transform.Find("Start");
        temp.GameObject().SetActive(false);

        




        Time.timeScale = 1.0f;
    }

    public void OnHUDNextClick()
    {
        Debug.Log("Next button reached ClickCode.");

        if (WaveSpawner.Instance == null)
        {
            Debug.LogError("No waveSpawner instance found.");
            return;
        }

        if (Time.timeScale == 0f)
        {
            Debug.Log("Click Start or resume before requestin a wave");
            return;
        }

        WaveSpawner.Instance.ForceNextWave();
    }
    //
    //Tower HUD
    public void UpTower(GameObject tower)
    {
        int val = 0;
        if (tower.TryGetComponent<Gun>(out Gun towerGun))
        {

            val = towerGun.towerCost;



        }
        else if (tower.TryGetComponent<MineTower>(out MineTower towerMine))
        {
            val = towerMine.towerCost;
        }
        else
        {

        }
        if (towersCostList.TryGetValue(tower.transform.position, out int tempint))
        {
            towersCostList[tower.transform.position] = tempint + val;
        }
        else
        {
            towersCostList[tower.transform.position] = val;
        }
        towersGameobjectList[towerspace.transform.position] = tower;
    }
    public void OnTowerHUDTowerClick()
    {
        if (TowerPrefabs != null)
        {
            if (towerPrefabNumber > 1)
            {
                GameObject temp=Instantiate(TowerPrefabs[towerPrefabNumber - 1], towerspace.transform.position, Quaternion.identity);
                UpTower(temp);
                towerBases[towerspace.transform.position] = towerspace;   // the base hidden below; selling shows it again
            }
        }
        towerspace.transform.gameObject.SetActive(false);
    }
    // Sell button: towerspace is the tower the player clicked (towerClick sets it).
    public void OnTowerHudSell()
    {
        if (towerspace == null || towerspace.GetComponent<ITower>() == null) return;   // nothing selected, or a base

        Vector3 spot = towerspace.transform.position;
        if (!towersCostList.TryGetValue(spot, out int spent)) return;   // placed in the scene by hand, not bought

        CurrencyManager.instance.AddCurrency(SellValue(spent));   // 70%, the same number the label shows
        towersCostList.Remove(spot);
        towersGameobjectList.Remove(spot);

        // Show the original base again, so it keeps the size the level gave it. A fresh one only if it's gone.
        if (towerBases.TryGetValue(spot, out GameObject baseSpot) && baseSpot != null)
        {
            baseSpot.SetActive(true);
        }
        else if (TowerPrefabs != null && TowerPrefabs.Count > 0)
        {
            Instantiate(TowerPrefabs[0], spot, Quaternion.identity);
        }
        towerBases.Remove(spot);

        TowerManu.SetActive(false);
        Destroy(towerspace);
        towerspace = null;
    }
    //
    //Pause Manu
    public void OnPauseBackClick()
    {
        
        last = PauseManu;
        ClearAll();
        HUD.SetActive(true);
        Transform temp=HUD.transform.Find("Start");
        if (temp.gameObject.activeSelf)
        {
Time.timeScale = 0.0f;

        }
        else
        {
            Time.timeScale = 1.0f;
        }
    }
    public void OnPauseSettingClick()
    {
        last = PauseManu;
        ClearAll();
        SettingManu.SetActive(true);
    }
    public void OnPauseResetClick()
    {
        Time.timeScale = 0f;

        RestoreStartButton();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void OnPauseLeaveClick()
    {
        last = PauseManu;
        ClearAll();
        HUD.SetActive(false);
        LevelMenu.SetActive(true);
       
        if (SceneManager.GetSceneByName(sceneToLoad).isLoaded)
        {

            SceneManager.LoadScene("Main Game and UI");

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
        SceneManager.LoadScene("Main Game and UI");
    }

    public void RestoreStartButton()
    {
        ClearAll();
        HUD.SetActive(true);

        Transform startButton = HUD.transform.Find("Start");

        if (startButton != null)
        {
            startButton.gameObject.SetActive(true);
        }

        // Reset timers and cooldown labels after showing the HUD.
        SpellSpawner[] spellSpawners =
            UnityEngine.Object.FindObjectsByType<SpellSpawner>(
                FindObjectsInactive.Include);

        foreach (SpellSpawner spellSpawner in spellSpawners)
        {
            spellSpawner.ResetCooldowns();
        }
    }
    public void OpenShop()
    {
        // Clear the selection so the Shop button doesn't stay stuck in its Selected color
        if (UnityEngine.EventSystems.EventSystem.current != null)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
       
        last = MainManu;
        ClearAll();          // hide Main / Level / etc. so they don't cover the Shop scene
        Time.timeScale = 1f;
        SceneManager.LoadScene("Shop");
    }
    public void OpenShopOut()
    {
        // Clear the selection so the Shop button doesn't stay stuck in its Selected color
        if (UnityEngine.EventSystems.EventSystem.current != null)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
       
        //last = MainManu;
        ClearAll();          // hide Main / Level / etc. so they don't cover the Shop scene
        Time.timeScale = 1f;
        SceneManager.LoadScene("Shop");
    }


    public void ReturnToLevelSelect()
    {
        Time.timeScale = 0f;

        ClearAll();
        LevelMenu.SetActive(true);
        last = LevelMenu;

        Transform startButton = HUD.transform.Find("Start");

        if (startButton != null)
        {
            startButton.gameObject.SetActive(true);
        }

        SceneManager.LoadScene("Main Game and UI");
    }



    public void HideGamePlayUI() // To hide any gameplay UI after game is over 
    {
        if(HUD != null)
        {
            HUD.SetActive(false);
        }

        if(PauseManu != null)
        {
            PauseManu.SetActive(false);
        }

        if(BaseManu != null)
        {
            BaseManu.SetActive(false);
        }

        TowerUpgrad upgradepanel = TowerUpgrad.Find();

        if(upgradepanel != null)
        {
            upgradepanel.BackButton();
        }
    }
}
